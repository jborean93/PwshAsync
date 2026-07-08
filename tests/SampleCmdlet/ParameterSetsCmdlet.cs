using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: PSAsyncCmdletAttribute with DefaultParameterSetName.
   Verifies: DefaultParameterSetName is set correctly in generated CmdletAttribute. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ParameterSets", DefaultParameterSetName = "ByName")]
public partial class TestParameterSets : PSAsyncCmdlet<string>
{
    [Parameter(ParameterSetName = "ByName", Mandatory = true)]
    public string Name { get; set; } = "";

    [Parameter(ParameterSetName = "ById", Mandatory = true)]
    public int Id { get; set; }

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        string output = DangerousGetCmdlet().ParameterSetName == "ByName"
            ? $"ByName: {Name}"
            : $"ById: {Id}";

        return WriteAsync(output, cancellationToken);
    }
}
