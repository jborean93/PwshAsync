using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Multiple parameters with different types (string, int, bool).
   Verifies: Multiple property synchronization and type handling. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "MultipleParameters")]
public partial class TestMultipleParameters : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Name { get; set; } = "";

    [Parameter]
    public int Count { get; set; }

    [Parameter]
    public bool IsEnabled { get; set; }

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        string output = $"Name={Name}, Count={Count}, IsEnabled={IsEnabled}";
        return WriteAsync(output, cancellationToken);
    }
}
