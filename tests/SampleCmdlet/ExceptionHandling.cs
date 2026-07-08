using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Exception in BeginAsync.
   Verifies: AfterBegin is NOT called when async block throws */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ExceptionInBeginAsync")]
public partial class TestExceptionInBeginAsync : PSAsyncCmdlet<string>
{
    protected override Task BeginAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("BeginAsync error");
    }

    protected override void AfterBegin()
    {
        DangerousGetCmdlet().WriteObject("AfterBegin-SHOULD-NOT-APPEAR");
    }
}

/* Tests: Exception in ProcessAsync.
   Verifies: AfterProcess is NOT called when async block throws */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ExceptionInProcessAsync")]
public partial class TestExceptionInProcessAsync : PSAsyncCmdlet<string>
{
    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("ProcessAsync error");
    }

    protected override void AfterProcess()
    {
        DangerousGetCmdlet().WriteObject("AfterProcess-SHOULD-NOT-APPEAR");
    }
}

/* Tests: Exception in BeforeBegin.
   Verifies: BeginAsync and AfterBegin are NOT called */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "ExceptionInBeforeBegin")]
public partial class TestExceptionInBeforeBegin : PSAsyncCmdlet<string>
{
    protected override void BeforeBegin()
    {
        throw new InvalidOperationException("BeforeBegin error");
    }

    protected override Task BeginAsync(CancellationToken cancellationToken)
    {
        return WriteAsync("BeginAsync-SHOULD-NOT-APPEAR", cancellationToken);
    }

    protected override void AfterBegin()
    {
        DangerousGetCmdlet().WriteObject("AfterBegin-SHOULD-NOT-APPEAR");
    }
}

/* Tests: ThrowTerminatingError in async block.
   Verifies: After block does NOT run when ThrowTerminatingError is called in async */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "TerminatingErrorInAsync")]
public partial class TestTerminatingErrorInAsync : PSAsyncCmdlet<string>
{
    protected override async Task BeginAsync(CancellationToken cancellationToken)
    {
        await WriteAsync("Before-ThrowTerminatingError", cancellationToken);

        var errorRecord = new ErrorRecord(
            new InvalidOperationException("Terminating error in async block"),
            "TerminatingErrorInAsync",
            ErrorCategory.InvalidOperation,
            null);
        DangerousGetCmdlet().ThrowTerminatingError(errorRecord);

        await WriteAsync("After-ThrowTerminatingError-SHOULD-NOT-APPEAR", cancellationToken);
    }

    protected override void AfterBegin()
    {
        DangerousGetCmdlet().WriteObject("AfterBegin-SHOULD-NOT-APPEAR");
    }
}
