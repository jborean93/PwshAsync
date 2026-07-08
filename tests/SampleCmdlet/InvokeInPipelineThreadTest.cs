using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: InvokeInPipelineThreadAsync for thread marshaling with Func.
   Verifies: Code executes on pipeline thread */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "InvokeInPipelineThread")]
public partial class TestInvokeInPipelineThread : PSAsyncCmdlet<string>
{
    private int _pipelineThreadId;

    protected override void BeforeProcess()
    {
        _pipelineThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        // Get thread ID from async context - should match pipeline thread
        var asyncThreadId = await InvokeInPipelineThreadAsync(() =>
        {
            return Thread.CurrentThread.ManagedThreadId;
        }, cancellationToken);

        await WriteAsync($"Match: {asyncThreadId == _pipelineThreadId}", cancellationToken);
    }
}

/* Tests: InvokeInPipelineThreadAsync with Action overload.
   Verifies: Action executes on pipeline thread */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "InvokeInPipelineThreadAction")]
public partial class TestInvokeInPipelineThreadAction : PSAsyncCmdlet<string>
{
    private int _pipelineThreadId;
    private int _actionThreadId;

    protected override void BeforeProcess()
    {
        _pipelineThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        // Execute action on pipeline thread
        await InvokeInPipelineThreadAsync(() =>
        {
            _actionThreadId = Thread.CurrentThread.ManagedThreadId;
        }, cancellationToken);

        await WriteAsync($"Match: {_actionThreadId == _pipelineThreadId}", cancellationToken);
    }
}
