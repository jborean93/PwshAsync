using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: WriteVerboseAsync output stream.
   Verifies: Async verbose output marshals to pipeline thread */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "VerboseOutput")]
public partial class TestVerboseOutput : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Message { get; set; } = "Verbose message";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteVerboseAsync(Message, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}
