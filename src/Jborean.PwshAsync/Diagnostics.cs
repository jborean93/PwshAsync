using Microsoft.CodeAnalysis;

namespace Jborean.PwshAsync;

// RS2008 requires AnalyzerReleases.Shipped/Unshipped.md files for tracking diagnostic rule releases.
// This is primarily for standalone analyzers distributed separately. Since this is a source generator
// tightly coupled with the library (not a standalone analyzer), and the diagnostics version with the
// generator itself, we suppress this requirement.
#pragma warning disable RS2008

internal static class Diagnostics
{
    private const string Category = "PwshAsync";

    // PWSHASYNC001: Class must be partial
    public static readonly DiagnosticDescriptor ClassMustBePartial = new(
        id: "PWSHASYNC001",
        title: "Class must be declared as partial",
        messageFormat: "Class '{0}' with [PSAsyncCmdlet] attribute must be declared as partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator needs to generate a partial class implementation.");

    // PWSHASYNC002: Class must inherit from PSAsyncCmdlet
    public static readonly DiagnosticDescriptor ClassMustInheritPSAsyncCmdlet = new(
        id: "PWSHASYNC002",
        title: "Class must inherit from PSAsyncCmdlet",
        messageFormat: "Class '{0}' with [PSAsyncCmdlet] attribute must inherit from PSAsyncCmdlet or PSAsyncCmdlet<T>",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Classes with [PSAsyncCmdlet] must inherit from PSAsyncCmdlet or PSAsyncCmdlet<T>.");

    // PWSHASYNC003: Class must not be nested
    public static readonly DiagnosticDescriptor ClassMustNotBeNested = new(
        id: "PWSHASYNC003",
        title: "Class must not be nested",
        messageFormat: "Class '{0}' with [PSAsyncCmdlet] attribute must not be a nested class",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "PSAsyncCmdlet classes cannot be nested within other types.");

    // PWSHASYNC004: Class must not be abstract
    public static readonly DiagnosticDescriptor ClassMustNotBeAbstract = new(
        id: "PWSHASYNC004",
        title: "Class with [PSAsyncCmdlet] must not be abstract",
        messageFormat: "Class '{0}' with [PSAsyncCmdlet] attribute must not be abstract. Only classes without the attribute can be abstract base classes.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "PSAsyncCmdlet classes must be concrete (non-abstract) to be instantiated by PowerShell. Use abstract classes without the attribute for base classes.");

    // PWSHASYNC005: Class must not be static
    public static readonly DiagnosticDescriptor ClassMustNotBeStatic = new(
        id: "PWSHASYNC005",
        title: "Class with [PSAsyncCmdlet] must not be static",
        messageFormat: "Class '{0}' with [PSAsyncCmdlet] attribute must not be static",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "PSAsyncCmdlet classes must be instantiable by PowerShell.");

    // PWSHASYNC006: Class must be public
    public static readonly DiagnosticDescriptor ClassMustBePublic = new(
        id: "PWSHASYNC006",
        title: "Class must be public",
        messageFormat: "Class '{0}' with [PSAsyncCmdlet] attribute must be public to be loadable by PowerShell",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "PowerShell requires cmdlet classes to be public.");

    // PWSHASYNC007: Parameter property must have simple get/set accessors
    public static readonly DiagnosticDescriptor ParameterPropertyMustHaveSimpleAccessors = new(
        id: "PWSHASYNC007",
        title: "Parameter property must have simple get; set; accessors",
        messageFormat: "Property '{0}' with [Parameter] attribute must have simple auto-implemented get; set; accessors",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Parameter properties must use simple auto-implemented accessors (get; set;) without custom logic or init-only setters.");
}
