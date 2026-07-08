using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: ShouldContinueAsync with user prompts.
   Verifies: ShouldContinueResult works with TestHost */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ShouldContinue")]
public partial class TestShouldContinue : PSAsyncCmdlet<string>
{
    [Parameter(ValueFromPipeline = true)]
    public string Target { get; set; } = string.Empty;

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var result = await ShouldContinueAsync(
            $"Process {Target}?",
            "Confirm",
            cancellationToken);

        if (result)
        {
            await WriteAsync($"Processed: {Target}", cancellationToken);
        }
        else
        {
            await WriteAsync($"Skipped: {Target}", cancellationToken);
        }
    }
}

/* Tests: ShouldContinueAsync with YesToAll/NoToAll.
   Verifies: YesToAll stops prompting for subsequent items */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ShouldContinueYesToAll")]
public partial class TestShouldContinueYesToAll : PSAsyncCmdlet<string>
{
    [Parameter(ValueFromPipeline = true)]
    public string Target { get; set; } = string.Empty;

    private bool _yesToAll;
    private bool _noToAll;

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var result = await ShouldContinueAsync(
            $"Process {Target}?",
            "Confirm",
            _yesToAll,
            _noToAll,
            cancellationToken);

        if (result.ShouldContinue)
        {
            await WriteAsync($"Processed: {Target}", cancellationToken);
        }
        else
        {
            await WriteAsync($"Skipped: {Target}", cancellationToken);
        }

        // Update the flags for next iteration
        _yesToAll = result.YesToAll;
        _noToAll = result.NoToAll;
    }
}
