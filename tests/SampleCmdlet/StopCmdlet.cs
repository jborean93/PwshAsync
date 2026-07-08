using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Stop signalling in async task blocks.
   Verifies: Async cancellation token is tied to pipeline stop trigger. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "StopCmdlet")]
public partial class TestStopCmdlet : PSAsyncCmdlet<string>
{
    public enum StopStage
    {
        Begin,
        Process,
        End,
    }

    [Parameter(Mandatory = true)]
    public ManualResetEventSlim StartTrigger { get; set; } = default!;

    [Parameter(Mandatory = true)]
    public StopStage Stage { get; set; }

    [Parameter(Mandatory = true)]
    public int Delay { get; set; } = 30000;

    protected override async Task BeginAsync(CancellationToken cancellationToken)
    {
        if (Stage == StopStage.Begin)
        {
            StartTrigger.Set();
            await Task.Delay(Delay, cancellationToken);
            await WriteAsync("Begin", cancellationToken);
        }
    }

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        if (Stage == StopStage.Process)
        {
            StartTrigger.Set();
            await Task.Delay(Delay, cancellationToken);
            await WriteAsync("Process", cancellationToken);
        }
    }

    protected override async Task EndAsync(CancellationToken cancellationToken)
    {
        if (Stage == StopStage.End)
        {
            StartTrigger.Set();
            await Task.Delay(Delay, cancellationToken);
            await WriteAsync("End", cancellationToken);
        }
    }
}
