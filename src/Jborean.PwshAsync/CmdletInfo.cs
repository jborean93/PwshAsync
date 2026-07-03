using System.Collections.Immutable;

namespace Jborean.PwshAsync;

internal record CmdletInfo(
    string ClassName,
    string Namespace,
    string Verb,
    string Noun,
    ImmutableArray<PropertyInfo> Properties,
    int? ConfirmImpact,
    string? DefaultParameterSetName,
    string? HelpUri,
    int? RemotingCapability,
    bool? SupportsPaging,
    bool? SupportsShouldProcess,
    bool? SupportsTransactions);
