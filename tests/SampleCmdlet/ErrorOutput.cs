using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: WriteErrorAsync non-terminating error stream.
   Verifies: Error stream works, cmdlet continues after error */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ErrorOutput")]
public partial class TestErrorOutput : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Message { get; set; } = "Error message";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var error = new ErrorRecord(
            new System.Exception(Message),
            "TestError",
            ErrorCategory.NotSpecified,
            null);

        await WriteErrorAsync(error, cancellationToken);
        await WriteAsync("Continued", cancellationToken);
    }
}
