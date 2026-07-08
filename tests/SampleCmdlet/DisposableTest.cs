using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Dispose is called when cmdlet completes.
   Verifies: Dispose is called */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "Disposable")]
public partial class TestDisposable : PSAsyncCmdlet<string>
{
    private static int _disposeCallCount = 0;

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync("Processed", cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Increment(ref _disposeCallCount);
        }
        base.Dispose(disposing);
    }

    public static int GetDisposeCallCount() => _disposeCallCount;
    public static void ResetDisposeCallCount() => _disposeCallCount = 0;
}

/* Tests: Dispose is called even when exception occurs.
   Verifies: Dispose is called in error scenarios */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "DisposableWithException")]
public partial class TestDisposableWithException : PSAsyncCmdlet<string>
{
    private static int _disposeCallCount = 0;

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("ProcessAsync error");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Increment(ref _disposeCallCount);
        }
        base.Dispose(disposing);
    }

    public static int GetDisposeCallCount() => _disposeCallCount;
    public static void ResetDisposeCallCount() => _disposeCallCount = 0;
}
