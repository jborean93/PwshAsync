using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: PSAsyncCmdletAttribute with ConfirmImpact enum value.
   Verifies: ConfirmImpact enum is cast to int and generated correctly. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ConfirmImpact", ConfirmImpact = ConfirmImpact.High, SupportsShouldProcess = true)]
public partial class TestConfirmImpact : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Message { get; set; } = "High impact action";

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync(Message, cancellationToken);
    }
}
