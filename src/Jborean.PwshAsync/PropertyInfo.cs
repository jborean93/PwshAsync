using System.Collections.Immutable;

namespace Jborean.PwshAsync;

internal record PropertyInfo(
    string Name,
    string Type,
    ImmutableArray<string> Attributes,
    bool HasGetter,
    bool HasSetter,
    bool IsInitOnly,
    bool IsRequired,
    bool ValueFromPipeline,
    bool ValueFromPipelineByPropertyName);
