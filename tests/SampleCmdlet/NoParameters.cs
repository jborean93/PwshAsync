using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Basic cmdlet with no parameters.
   Verifies: Minimal cmdlet generation works, only common parameters are present. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "NoParameters")]
public partial class TestNoParameters : PSAsyncCmdlet<string>
{
    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync("Success", cancellationToken);
    }
}
