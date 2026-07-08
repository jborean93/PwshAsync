using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: WriteWarningAsync output stream.
   Verifies: Async warning output works */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "WarningOutput")]
public partial class TestWarningOutput : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Message { get; set; } = "Warning message";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteWarningAsync(Message, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}
