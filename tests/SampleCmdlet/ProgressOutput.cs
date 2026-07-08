using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: WriteProgressAsync output stream.
   Verifies: Progress reporting works */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ProgressOutput")]
public partial class TestProgressOutput : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Activity { get; set; } = "Processing";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var progress = new ProgressRecord(1, Activity, "Working on it");
        await WriteProgressAsync(progress, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}
