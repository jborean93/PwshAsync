using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Parameter with ValidateNotNullOrEmpty attribute.
   Verifies: Validation attributes are copied and enforced, rejecting null/empty values. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ValidationAttribute")]
public partial class TestValidationAttribute : PSAsyncCmdlet<string>
{
    [Parameter]
    [ValidateNotNullOrEmpty]
    public string Value { get; set; } = "";

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync(Value, cancellationToken);
    }
}
