using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: PSAsyncCmdletAttribute with SupportsShouldProcess = true.
   Verifies: ShouldProcessAsync works, WhatIf/Confirm parameters added, prompts captured by TestHost. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ShouldProcess", SupportsShouldProcess = true)]
public partial class TestShouldProcess : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Target { get; set; } = "DefaultTarget";

    [Parameter]
    public string Action { get; set; } = "DefaultAction";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        bool shouldContinue = await ShouldProcessAsync(Target, Action, cancellationToken);

        if (shouldContinue)
        {
            await WriteAsync($"Processed: {Target} with {Action}", cancellationToken);
        }
        else
        {
            await WriteAsync($"Skipped: {Target}", cancellationToken);
        }
    }
}
