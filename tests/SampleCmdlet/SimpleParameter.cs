using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Single parameter without attributes.
   Verifies: Basic parameter binding and property synchronization. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "SimpleParameter")]
public partial class TestSimpleParameter : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Name { get; set; } = "";

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync(Name, cancellationToken);
    }
}
