using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: PSAsyncCmdletAttribute with HelpUri.
   Verifies: HelpUri string is set correctly in generated CmdletAttribute. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "HelpUri", HelpUri = "https://example.com/help")]
public partial class TestHelpUri : PSAsyncCmdlet<string>
{
    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync("Help available", cancellationToken);
    }
}
