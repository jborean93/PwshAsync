using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Jborean.PwshAsync;

[Generator]
public class PSAsyncCmdletGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<string> namespaceProvider = context
            .AnalyzerConfigOptionsProvider
            .Select((c, _) =>
                c.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? nameSpace)
                    ? nameSpace
                    : "Jborean.PwshAsync");

        // Detect if PipelineStopToken is available in the target System.Management.Automation version
        IncrementalValueProvider<bool> hasPipelineStopTokenProvider = context.CompilationProvider
            .Select((compilation, _) => HasPipelineStopToken(compilation));

        // Register attribute definition
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource(
                "PSAsyncCmdletAttribute.g.cs",
                SourceText.From(Embedded.PSAsyncCmdletAttribute, Encoding.UTF8));
        });

        // Register template files with the assembly-specific namespace and feature detection
        IncrementalValueProvider<(string Namespace, bool HasPipelineStopToken)> templateProvider =
            namespaceProvider.Combine(hasPipelineStopTokenProvider);

        context.RegisterSourceOutput(templateProvider, (spc, data) =>
        {
            (string replacementNamespace, bool hasPipelineStopToken) = data;

            // Add template base classes from embedded resources
            spc.AddSource(
                "IAsyncHelper.g.cs",
                GenerateSourceText(Embedded.IAsyncHelper, replacementNamespace));
            spc.AddSource(
                "PSAsyncCmdlet.g.cs",
                GenerateSourceText(Embedded.PSAsyncCmdlet, replacementNamespace));
            spc.AddSource(
                "PSAsyncCmdletBase.g.cs",
                GenerateSourceText(Embedded.PSAsyncCmdletBase, replacementNamespace));
            spc.AddSource(
                "PSAsyncCmdletBase.PipelineStop.g.cs",
                GeneratePSAsyncCmdletBasePartial(replacementNamespace, hasPipelineStopToken));
        });

        // Find cmdlet classes with [PSAsyncCmdlet]
        IncrementalValuesProvider<CmdletInfo?> cmdletClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Jborean.PwshAsync.PSAsyncCmdletAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => ExtractCmdletInfo(ctx))
            .Where(static info => info is not null);

        // Combine cmdlet classes with the namespace
        IncrementalValuesProvider<(CmdletInfo?, string)> cmdletsWithNamespace = cmdletClasses.Combine(namespaceProvider);

        // Generate PSCmdlet wrapper for each cmdlet
        context.RegisterSourceOutput(cmdletsWithNamespace, static (spc, data) =>
        {
            (CmdletInfo? cmdletInfo, string replacementNamespace) = data;
            if (cmdletInfo is null)
            {
                return;
            }

            string source = GeneratePSCmdletWrapper(cmdletInfo, replacementNamespace);
            spc.AddSource(
                $"{cmdletInfo.ClassName}_PSCmdlet.g.cs",
                SourceText.From(source, Encoding.UTF8));
        });
    }

    private static bool HasPipelineStopToken(Compilation compilation)
    {
        // Look for System.Management.Automation.PSCmdlet type
        INamedTypeSymbol? psCmdletType = compilation.GetTypeByMetadataName("System.Management.Automation.PSCmdlet");
        if (psCmdletType == null)
        {
            return false;
        }

        // Check if it has a PipelineStopToken property
        foreach (ISymbol member in psCmdletType.GetMembers("PipelineStopToken"))
        {
            if (member is IPropertySymbol)
            {
                return true;
            }
        }

        return false;
    }

    private static CmdletInfo? ExtractCmdletInfo(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetNode is not ClassDeclarationSyntax classDecl)
        {
            return null;
        }

        ISymbol? declaredSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
        if (declaredSymbol is not INamedTypeSymbol symbol)
        {
            return null;
        }

        // Get verb and noun from attribute
        ImmutableArray<AttributeData> attributes = context.Attributes;
        if (attributes.Length == 0)
        {
            return null;
        }

        AttributeData attribute = attributes[0];
        if (attribute.ConstructorArguments.Length < 2)
        {
            return null;
        }

        string verb = attribute.ConstructorArguments[0].Value?.ToString() ?? "Get";
        string noun = attribute.ConstructorArguments[1].Value?.ToString() ?? "Item";

        // Extract named properties from the attribute
        int? confirmImpact = null;
        string? defaultParameterSetName = null;
        string? helpUri = null;
        int? remotingCapability = null;
        bool? supportsPaging = null;
        bool? supportsShouldProcess = null;
        bool? supportsTransactions = null;

        foreach (var namedArg in attribute.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "ConfirmImpact":
                    confirmImpact = (int?)namedArg.Value.Value;
                    break;
                case "DefaultParameterSetName":
                    defaultParameterSetName = namedArg.Value.Value?.ToString();
                    break;
                case "HelpUri":
                    helpUri = namedArg.Value.Value?.ToString();
                    break;
                case "RemotingCapability":
                    remotingCapability = (int?)namedArg.Value.Value;
                    break;
                case "SupportsPaging":
                    supportsPaging = (bool?)namedArg.Value.Value;
                    break;
                case "SupportsShouldProcess":
                    supportsShouldProcess = (bool?)namedArg.Value.Value;
                    break;
                case "SupportsTransactions":
                    supportsTransactions = (bool?)namedArg.Value.Value;
                    break;
            }
        }

        // Discover properties
        ImmutableArray<PropertyInfo> properties = DiscoverProperties(symbol);

        return new(
            ClassName: symbol.Name,
            Namespace: symbol.ContainingNamespace.ToDisplayString(),
            Verb: verb,
            Noun: noun,
            Properties: properties,
            ConfirmImpact: confirmImpact,
            DefaultParameterSetName: defaultParameterSetName,
            HelpUri: helpUri,
            RemotingCapability: remotingCapability,
            SupportsPaging: supportsPaging,
            SupportsShouldProcess: supportsShouldProcess,
            SupportsTransactions: supportsTransactions);
    }

    private static ImmutableArray<PropertyInfo> DiscoverProperties(INamedTypeSymbol classSymbol)
    {
        ImmutableArray<PropertyInfo>.Builder properties = ImmutableArray.CreateBuilder<PropertyInfo>();
        HashSet<string> seenProperties = new();
        INamedTypeSymbol? currentType = classSymbol;

        // Walk up inheritance chain until PSAsyncCmdlet or object
        while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
        {
            // Stop at PSAsyncCmdlet base
            if (currentType.Name == "PSAsyncCmdlet" || currentType.Name == "PSAsyncCmdletBase")
            {
                break;
            }

            foreach (ISymbol member in currentType.GetMembers())
            {
                if (member is IPropertySymbol property &&
                    property.DeclaredAccessibility == Accessibility.Public &&
                    !seenProperties.Contains(property.Name))
                {
                    ImmutableArray<string> attributes = GetPropertyAttributes(property);
                    (bool valueFromPipeline, bool valueFromPipelineByPropertyName) = DetectPipelineFlags(property);

                    properties.Add(new(
                        Name: property.Name,
                        Type: property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        Attributes: attributes,
                        HasGetter: property.GetMethod != null,
                        HasSetter: property.SetMethod != null,
                        IsInitOnly: property.SetMethod?.IsInitOnly ?? false,
                        IsRequired: property.IsRequired,
                        ValueFromPipeline: valueFromPipeline,
                        ValueFromPipelineByPropertyName: valueFromPipelineByPropertyName
                    ));

                    seenProperties.Add(property.Name);
                }
            }

            currentType = currentType.BaseType;
        }

        return properties.ToImmutable();
    }

    private static (bool ValueFromPipeline, bool ValueFromPipelineByPropertyName) DetectPipelineFlags(IPropertySymbol property)
    {
        bool valueFromPipeline = false;
        bool valueFromPipelineByPropertyName = false;

        foreach (AttributeData attr in property.GetAttributes())
        {
            if (attr.AttributeClass == null)
            {
                continue;
            }

            // Check if this is a ParameterAttribute
            if (attr.AttributeClass.Name != "ParameterAttribute")
            {
                continue;
            }

            // Check named arguments for ValueFromPipeline and ValueFromPipelineByPropertyName
            foreach (KeyValuePair<string, TypedConstant> namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "ValueFromPipeline" && namedArg.Value.Value is bool vfp)
                {
                    valueFromPipeline = vfp;
                }
                else if (namedArg.Key == "ValueFromPipelineByPropertyName" && namedArg.Value.Value is bool vfpbpn)
                {
                    valueFromPipelineByPropertyName = vfpbpn;
                }
            }
        }

        return (valueFromPipeline, valueFromPipelineByPropertyName);
    }

    private static ImmutableArray<string> GetPropertyAttributes(IPropertySymbol property)
    {
        ImmutableArray<string>.Builder attributes = ImmutableArray.CreateBuilder<string>();

        foreach (AttributeData attr in property.GetAttributes())
        {
            // Skip compiler-generated
            if (attr.AttributeClass == null || attr.AttributeClass.Name.Contains("Compiler"))
            {
                continue;
            }

            StringBuilder sb = new();
            sb.Append('[');
            sb.Append("global::");
            sb.Append(attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)));

            if (attr.ConstructorArguments.Length > 0 || attr.NamedArguments.Length > 0)
            {
                sb.Append('(');

                // Constructor args
                for (int i = 0; i < attr.ConstructorArguments.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(FormatTypedConstant(attr.ConstructorArguments[i]));
                }

                // Named args
                for (int i = 0; i < attr.NamedArguments.Length; i++)
                {
                    if (i > 0 || attr.ConstructorArguments.Length > 0)
                    {
                        sb.Append(", ");
                    }

                    KeyValuePair<string, TypedConstant> namedArg = attr.NamedArguments[i];
                    sb.Append(namedArg.Key);
                    sb.Append(" = ");
                    sb.Append(FormatTypedConstant(namedArg.Value));
                }

                sb.Append(')');
            }

            sb.Append(']');
            attributes.Add(sb.ToString());
        }

        return attributes.ToImmutable();
    }

    private static string FormatTypedConstant(TypedConstant constant)
    {
        if (constant.IsNull)
        {
            return "null";
        }

        if (constant.Kind == TypedConstantKind.Enum && constant.Type != null)
        {
            return $"global::{constant.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}.{constant.Value}";
        }

        if (constant.Type?.SpecialType == SpecialType.System_String)
        {
            return $"\"{constant.Value}\"";
        }

        if (constant.Kind == TypedConstantKind.Type)
        {
            return $"typeof(global::{constant.Value})";
        }

        if (constant.Type?.SpecialType == SpecialType.System_Boolean)
        {
            return constant.Value?.ToString()?.ToLowerInvariant() ?? "false";
        }

        return constant.Value?.ToString() ?? "null";
    }

    private static string GeneratePSCmdletWrapper(CmdletInfo cmdlet, string generatedNamespace)
    {
        StringBuilder sb = new();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// This file was automatically generated by Jborean.PwshAsync.");
        sb.AppendLine("// Do not edit this file manually as your changes will be overwritten on the next build.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        sb.AppendLine($"namespace {generatedNamespace}");
        sb.AppendLine("{");

        // The actual PSCmdlet that PowerShell loads
        sb.Append($"    [global::System.Management.Automation.Cmdlet(\"{cmdlet.Verb}\", \"{cmdlet.Noun}\"");

        // Add optional CmdletAttribute properties if set
        if (cmdlet.ConfirmImpact.HasValue)
        {
            sb.Append($", ConfirmImpact = (global::System.Management.Automation.ConfirmImpact){cmdlet.ConfirmImpact.Value}");
        }
        if (cmdlet.DefaultParameterSetName != null)
        {
            sb.Append($", DefaultParameterSetName = \"{cmdlet.DefaultParameterSetName}\"");
        }
        if (cmdlet.HelpUri != null)
        {
            sb.Append($", HelpUri = \"{cmdlet.HelpUri}\"");
        }
        if (cmdlet.RemotingCapability.HasValue)
        {
            sb.Append($", RemotingCapability = (global::System.Management.Automation.RemotingCapability){cmdlet.RemotingCapability.Value}");
        }
        if (cmdlet.SupportsPaging.HasValue)
        {
            sb.Append($", SupportsPaging = {cmdlet.SupportsPaging.Value.ToString().ToLowerInvariant()}");
        }
        if (cmdlet.SupportsShouldProcess.HasValue)
        {
            sb.Append($", SupportsShouldProcess = {cmdlet.SupportsShouldProcess.Value.ToString().ToLowerInvariant()}");
        }
        if (cmdlet.SupportsTransactions.HasValue)
        {
            sb.Append($", SupportsTransactions = {cmdlet.SupportsTransactions.Value.ToString().ToLowerInvariant()}");
        }

        sb.AppendLine(")]");
        sb.AppendLine($"    public sealed class {cmdlet.ClassName}_PSCmdlet");
        sb.AppendLine($"        : global::{generatedNamespace}.PSAsyncCmdletBase<{cmdlet.ClassName}>");
        sb.AppendLine("    {");

        // Copy all properties with their attributes
        foreach (PropertyInfo prop in cmdlet.Properties)
        {
            foreach (string attr in prop.Attributes)
            {
                sb.AppendLine($"        {attr}");
            }

            sb.Append($"        public {prop.Type} {prop.Name} {{ get;");
            if (prop.IsInitOnly)
            {
                sb.Append(" init;");
            }
            else if (prop.HasSetter)
            {
                sb.Append(" set;");
            }
            sb.Append(" }");

            // Initialize with default! to satisfy nullable reference types
            // The actual default value comes from the user's class
            if (!prop.Type.Contains("?") && !prop.Type.StartsWith("System.Nullable") &&
                !prop.Type.StartsWith("int") && !prop.Type.StartsWith("bool") &&
                !prop.Type.StartsWith("double") && !prop.Type.StartsWith("float") &&
                !prop.Type.StartsWith("decimal") && !prop.Type.StartsWith("long") &&
                !prop.Type.StartsWith("short") && !prop.Type.StartsWith("byte"))
            {
                sb.Append(" = default!;");
            }

            sb.AppendLine();
            sb.AppendLine();
        }

        // Filter properties that need to be synced on each pipeline record
        ImmutableArray<PropertyInfo>.Builder pipelinePropsBuilder = ImmutableArray.CreateBuilder<PropertyInfo>();
        foreach (PropertyInfo prop in cmdlet.Properties)
        {
            if (prop.ValueFromPipeline || prop.ValueFromPipelineByPropertyName)
            {
                pipelinePropsBuilder.Add(prop);
            }
        }
        ImmutableArray<PropertyInfo> pipelineProps = pipelinePropsBuilder.ToImmutable();
        ImmutableArray<PropertyInfo> allProps = cmdlet.Properties;

        // SyncInitialProperties - called once in BeginProcessing - syncs ALL properties
        sb.AppendLine("        protected override void SyncInitialProperties()");
        sb.AppendLine("        {");
        foreach (PropertyInfo prop in allProps)
        {
            // Only sync if PowerShell bound this parameter
            sb.AppendLine($"            if (MyInvocation.BoundParameters.ContainsKey(\"{prop.Name}\"))");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                _asyncCmdlet.{prop.Name} = this.{prop.Name};");
            sb.AppendLine($"            }}");
        }
        sb.AppendLine("        }");
        sb.AppendLine();

        // SyncPipelineProperties - called per record in ProcessRecord - syncs ValueFromPipeline properties
        sb.AppendLine("        protected override void SyncPipelineProperties()");
        sb.AppendLine("        {");
        foreach (PropertyInfo prop in pipelineProps)
        {
            sb.AppendLine($"            if (MyInvocation.BoundParameters.ContainsKey(\"{prop.Name}\"))");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                _asyncCmdlet.{prop.Name} = this.{prop.Name};");
            sb.AppendLine($"            }}");
        }
        sb.AppendLine("        }");

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static SourceText GenerateSourceText(string source, string replacementNamespace)
    {
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// This file was automatically generated by Jborean.PwshAsync.");
        sb.AppendLine("// Do not edit this file manually as your changes will be overwritten on the next build.");
        sb.AppendLine();

        string replacedSource = source.Replace("NamespaceReplaceMe", replacementNamespace);
        sb.Append(replacedSource);

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private static SourceText GeneratePSAsyncCmdletBasePartial(string replacementNamespace, bool hasPipelineStopToken)
    {
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("// This file was automatically generated by Jborean.PwshAsync.");
        sb.AppendLine("// Do not edit this file manually as your changes will be overwritten on the next build.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {replacementNamespace}");
        sb.AppendLine("{");

        if (hasPipelineStopToken)
        {
            // S.M.A 7.7.0+ has PipelineStopToken - no shim needed
            sb.AppendLine("    public abstract partial class PSAsyncCmdletBase<TAsyncCmdlet>");
            sb.AppendLine("    {");
            sb.AppendLine("        // PipelineStopToken is provided by PSCmdlet base class (S.M.A 7.7.0+)");
            sb.AppendLine("        // No additional implementation needed");
            sb.AppendLine("    }");
        }
        else
        {
            // S.M.A < 7.7.0 - provide PipelineStopToken shim using CancellationTokenSource
            sb.AppendLine("    public abstract partial class PSAsyncCmdletBase<TAsyncCmdlet>");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly global::System.Threading.CancellationTokenSource _cancelSource = new();");
            sb.AppendLine();
            sb.AppendLine("        // PipelineStopToken shim for S.M.A < 7.7.0");
            sb.AppendLine("        internal global::System.Threading.CancellationToken PipelineStopToken => _cancelSource.Token;");
            sb.AppendLine();
            sb.AppendLine("        protected override void StopProcessing()");
            sb.AppendLine("        {");
            sb.AppendLine("            _cancelSource.Cancel();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        partial void DisposeInternal()");
            sb.AppendLine("        {");
            sb.AppendLine("            _cancelSource.Dispose();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }
}
