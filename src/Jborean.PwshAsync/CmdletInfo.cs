using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Jborean.PwshAsync;

internal record CmdletInfo(
    string ClassName,
    string Namespace,
    string Verb,
    string Noun,
    ImmutableArray<PropertyInfo> Properties,
    string CmdletAttributeArguments,  // Additional attribute arguments beyond verb/noun (e.g., ", ConfirmImpact = ...")
    ImmutableArray<Diagnostic> Diagnostics);
