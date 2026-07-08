using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Mix of static parameter and ValueFromPipeline parameter.
   Verifies: SyncInitialProperties (static param set once) vs SyncPipelineProperties (pipeline param per record).
   Outputs parameter values in all lifecycle hooks to prove synchronization timing. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "MixedPipelineProperties")]
public partial class TestMixedPipelineProperties : PSAsyncCmdlet<string>
{
    [Parameter]
    public string StaticParam { get; set; } = "";

    [Parameter(ValueFromPipeline = true)]
    public string PipelineParam { get; set; } = "";

    protected override void BeforeBegin()
    {
        WriteToOutput($"BeforeBegin: StaticParam={StaticParam}, PipelineParam={PipelineParam}");
    }

    protected override Task BeginAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"BeginAsync: StaticParam={StaticParam}, PipelineParam={PipelineParam}", cancellationToken);
    }

    protected override void AfterBegin()
    {
        WriteToOutput($"AfterBegin: StaticParam={StaticParam}, PipelineParam={PipelineParam}");
    }

    protected override void BeforeProcess()
    {
        WriteToOutput($"BeforeProcess: StaticParam={StaticParam}, PipelineParam={PipelineParam}");
    }

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"ProcessAsync: StaticParam={StaticParam}, PipelineParam={PipelineParam}", cancellationToken);
    }

    protected override void AfterProcess()
    {
        WriteToOutput($"AfterProcess: StaticParam={StaticParam}, PipelineParam={PipelineParam}");
    }

    protected override void BeforeEnd()
    {
        WriteToOutput($"BeforeEnd: StaticParam={StaticParam}, PipelineParam={PipelineParam}");
    }

    protected override Task EndAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"EndAsync: StaticParam={StaticParam}, PipelineParam={PipelineParam}", cancellationToken);
    }

    protected override void AfterEnd()
    {
        WriteToOutput($"AfterEnd: StaticParam={StaticParam}, PipelineParam={PipelineParam}");
    }

    private void WriteToOutput(string message)
    {
        // Use DangerousGetCmdlet to write from synchronous Before/After methods
        DangerousGetCmdlet().WriteObject(message);
    }
}
