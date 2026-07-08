using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: All ParameterAttribute properties are preserved.
   Verifies: All ParameterAttribute properties copied to generated cmdlet */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "AllParameterAttributes")]
public partial class TestAllParameterAttributes : PSAsyncCmdlet<string>
{
    [Parameter(
        Mandatory = true,
        Position = 0,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true,
        ValueFromRemainingArguments = false,
        HelpMessage = "The input value",
        ParameterSetName = "TestSet",
        DontShow = false)]
    public string? InputValue { get; set; }

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"Input: {InputValue}", cancellationToken);
    }
}
