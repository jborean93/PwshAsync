using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Nullable value type property.
   Verifies: int? (Nullable<int>) type is handled correctly */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "NullableValueType")]
public partial class TestNullableValueType : PSAsyncCmdlet<string>
{
    [Parameter]
    public int? OptionalNumber { get; set; }

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"Number: {OptionalNumber?.ToString() ?? "<null>"}", cancellationToken);
    }
}
