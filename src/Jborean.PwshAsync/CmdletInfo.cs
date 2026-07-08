using System.Collections.Immutable;

namespace Jborean.PwshAsync;

internal record CmdletInfo(
    string ClassName,
    string Namespace,
    string Verb,
    string Noun,
    ImmutableArray<PropertyInfo> Properties,
    string? ConfirmImpact,
    string? DefaultParameterSetName,
    string? HelpUri,
    string? RemotingCapability,
    string? SupportsPaging,
    string? SupportsShouldProcess);
