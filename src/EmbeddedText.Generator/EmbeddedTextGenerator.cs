using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EmbeddedText.Generator;

[Generator]
public class EmbeddedTextGenerator : IIncrementalGenerator
{
    private const string EmbeddedTextAttribute = @"#nullable enable

        namespace EmbeddedText.Generator
        {
            [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
            [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            internal class EmbeddedTextAttribute : global::System.Attribute
            {
            }
        }
    ";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddEmbeddedAttributeDefinition();
            ctx.AddSource(
                "LocalScriptAttribute.g.cs",
                SourceText.From(EmbeddedTextAttribute, Encoding.UTF8));
        });

        IncrementalValuesProvider<EmbeddedClassInfo?> classesToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "EmbeddedText.Generator.EmbeddedTextAttribute",
                predicate: static (s, _) => true,
                transform: static (ctx, _) => ExtractClassInfo(ctx.TargetSymbol))
            .Where(static m => m is not null);

        IncrementalValuesProvider<(EmbeddedClassInfo?, ImmutableArray<AdditionalText>)> combined = classesToGenerate
            .Combine(context.AdditionalTextsProvider.Collect());

        context.RegisterSourceOutput(combined,
            static (spc, source) =>
            {
                (EmbeddedClassInfo? classInfo, ImmutableArray<AdditionalText> files) = source;
                Generate(spc, classInfo, files);
            });
    }

    private static EmbeddedClassInfo? ExtractClassInfo(ISymbol symbol)
    {
        if (symbol is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        return new(
            Name: classSymbol.Name,
            Namespace: classSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : classSymbol.ContainingNamespace.ToDisplayString());
    }

    private static void Generate(
        SourceProductionContext context,
        EmbeddedClassInfo? classInfo,
        ImmutableArray<AdditionalText> files)
    {
        if (classInfo is null)
        {
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine($"// Auto-generated script for {classInfo.Name}");

        if (!string.IsNullOrEmpty(classInfo.Namespace))
        {
            sb.AppendLine($"namespace {classInfo.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine($"partial class {classInfo.Name}");
        sb.AppendLine("{");

        foreach (AdditionalText file in files)
        {
            string fieldName = Path.GetFileNameWithoutExtension(file.Path);
            string scriptText = file.GetText(context.CancellationToken)?.ToString() ?? "";
            string escapedContent = scriptText.Replace("\"", "\"\"");

            sb.AppendLine($"    // Script: {file.Path}");
            sb.AppendLine($"    public const string {fieldName} = @\"{escapedContent}\";");
        }

        sb.AppendLine("}");

        context.AddSource(
            $"{classInfo.Name}.EmbedScripts.g.cs",
            sb.ToString());
    }

    private record EmbeddedClassInfo(string Name, string Namespace);
}
