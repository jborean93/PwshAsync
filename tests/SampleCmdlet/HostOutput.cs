using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: WriteHostAsync output.
   Verifies: Host output works */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "HostOutput")]
public partial class TestHostOutput : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Message { get; set; } = "Host message";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteHostAsync(Message, false, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}

/* Tests: WriteHostAsync with noNewLine.
   Verifies: Host output without newline works */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "HostOutputNoNewLine")]
public partial class TestHostOutputNoNewLine : PSAsyncCmdlet<string>
{
    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteHostAsync("Part1", true, cancellationToken);
        await WriteHostAsync("Part2", false, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}
