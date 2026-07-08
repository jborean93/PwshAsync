using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Parameter with ValueFromPipelineByPropertyName = true.
   Verifies: Property name binding works, values are extracted from pipeline objects by property name. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ValueFromPipelineByPropertyName")]
public partial class TestValueFromPipelineByPropertyName : PSAsyncCmdlet<string>
{
    [Parameter(ValueFromPipelineByPropertyName = true)]
    public string Name { get; set; } = "";

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"Name: {Name}", cancellationToken);
    }
}
