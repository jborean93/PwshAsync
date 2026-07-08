using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: PSAsyncCmdletAttribute with SupportsPaging = true.
   Verifies: SupportsPaging bool value is generated correctly. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "SupportsPaging", SupportsPaging = true)]
public partial class TestSupportsPaging : PSAsyncCmdlet<string>
{
    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync("Paging enabled", cancellationToken);
    }
}
