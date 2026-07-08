using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Parameter with ValueFromPipeline = true.
   Verifies: SyncPipelineProperties is called for each pipeline record, ProcessAsync runs per record. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ValueFromPipeline")]
public partial class TestValueFromPipeline : PSAsyncCmdlet<string>
{
    [Parameter(ValueFromPipeline = true)]
    public string InputObject { get; set; } = "";

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"Input: {InputObject}", cancellationToken);
    }
}
