using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Parameters with Position attribute (Position = 0, Position = 1).
   Verifies: Positional parameters work correctly, both positionally and by name. */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "PositionalParameter")]
public partial class TestPositionalParameter : PSAsyncCmdlet<string>
{
    [Parameter(Position = 0)]
    public string First { get; set; } = "";

    [Parameter(Position = 1)]
    public string Second { get; set; } = "";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteAsync(First, cancellationToken);
        await WriteAsync(Second, cancellationToken);
    }
}
