using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Generic collection parameter type.
   Verifies: Generic types (List<string>) are fully qualified correctly */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "GenericCollection")]
public partial class TestGenericCollection : PSAsyncCmdlet<string>
{
    [Parameter]
    public List<string> Items { get; set; } = new();

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync($"Items: {string.Join(", ", Items)}", cancellationToken);
    }
}
