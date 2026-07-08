using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Parameter with Mandatory = true attribute.
   Verifies: Mandatory attribute is copied to generated wrapper and enforced by PowerShell. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "MandatoryParameter")]
public partial class TestMandatoryParameter : PSAsyncCmdlet<string>
{
    [Parameter(Mandatory = true)]
    public string Required { get; set; } = "";

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync(Required, cancellationToken);
    }
}
