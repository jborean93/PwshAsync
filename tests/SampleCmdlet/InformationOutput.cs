using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: WriteInformationAsync output stream with InformationRecord.
   Verifies: Information stream works */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "InformationOutput")]
public partial class TestInformationOutput : PSAsyncCmdlet<string>
{
    [Parameter]
    public string Message { get; set; } = "Information message";

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var infoRecord = new InformationRecord(Message, "TestSource");
        await WriteInformationAsync(infoRecord, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}

/* Tests: WriteInformationAsync with object and tags overload.
   Verifies: Information stream with tags works */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "InformationWithTags")]
public partial class TestInformationWithTags : PSAsyncCmdlet<string>
{
    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteInformationAsync("InfoData", new[] { "Tag1", "Tag2" }, cancellationToken);
        await WriteAsync("Output", cancellationToken);
    }
}
