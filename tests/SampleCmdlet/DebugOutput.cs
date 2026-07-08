using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: WriteDebugAsync output stream.
   Verifies: Async debug output works */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "DebugOutput")]
public partial class TestDebugOutput : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Message { get; set; } = "Debug message";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteDebugAsync(Message, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}
