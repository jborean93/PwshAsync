using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Array parameter type.
   Verifies: Array types (string[]) are fully qualified correctly */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ArrayParameter")]
public partial class TestArrayParameter : PSAsyncCmdlet<string>
{
    [Parameter]
    public string[] Items { get; set; } = System.Array.Empty<string>();

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"Items: {string.Join(", ", Items)}", cancellationToken);
    }
}
