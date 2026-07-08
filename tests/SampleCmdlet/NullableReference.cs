using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Nullable reference type property.
   Verifies: string? type is handled correctly without default! */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "NullableReference")]
public partial class TestNullableReference : PSAsyncCmdlet<string>
{
    [Parameter]
    public string? OptionalValue { get; set; }

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"Value: {OptionalValue ?? "<null>"}", cancellationToken);
    }
}
