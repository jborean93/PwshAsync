using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Parameter with multiple attributes (Mandatory + ValidateSet).
   Verifies: Multiple attributes are reconstructed correctly, including array values in ValidateSet. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "MultipleAttributes")]
public partial class TestMultipleAttributes : PSAsyncCmdlet<string>
{
    [Parameter(Mandatory = true)]
    [ValidateSet("Red", "Green", "Blue")]
    public string Color { get; set; } = "";

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync(Color, cancellationToken);
    }
}
