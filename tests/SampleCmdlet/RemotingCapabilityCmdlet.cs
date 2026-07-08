using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: PSAsyncCmdletAttribute with RemotingCapability value.
   Verifies: RemotingCapability int value is generated correctly. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "RemotingCapability", RemotingCapability = RemotingCapability.PowerShell)]
public partial class TestRemotingCapability : PSAsyncCmdlet<string>
{
    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync("Test", cancellationToken);
    }
}
