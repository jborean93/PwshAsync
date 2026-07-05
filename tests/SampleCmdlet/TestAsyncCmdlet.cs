using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

[PSAsyncCmdlet(VerbsDiagnostic.Test, "SimpleCmdletBlocks")]
public partial class TestSimpleCmdletBlocks : PSAsyncCmdlet<string>
{
    protected override Task BeginAsync(CancellationToken cancellationToken)
        => WriteAsync("BeginAsync", cancellationToken);

    protected override Task ProcessAsync(CancellationToken cancellationToken)
        => WriteAsync("ProcessAsync", cancellationToken);

    protected override Task EndAsync(CancellationToken cancellationToken)
        => WriteAsync("EndAsync", cancellationToken);
}
