using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Base class with common parameter.
   Tests: Generator includes base class parameters in generated cmdlet */
public abstract class TestInheritanceBase : PSAsyncCmdlet<string>
{
    [Parameter(Mandatory = true)]
    public string? CommonParameter { get; set; }

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteAsync($"Common: {CommonParameter}", cancellationToken);
    }
}

/* Subclass with specific parameter.
   Tests: Generated cmdlet includes both base (CommonParameter) and derived (SpecificParameterA) */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "InheritanceSubclassA")]
public partial class TestInheritanceSubclassA : TestInheritanceBase
{
    [Parameter]
    public string? SpecificParameterA { get; set; }

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await base.ProcessAsync(cancellationToken);
        if (!string.IsNullOrEmpty(SpecificParameterA))
        {
            await WriteAsync($"SpecificA: {SpecificParameterA}", cancellationToken);
        }
    }
}

/* Another subclass with different specific parameter.
   Tests: Generated cmdlet includes both base (CommonParameter) and derived (SpecificParameterB) */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "InheritanceSubclassB")]
public partial class TestInheritanceSubclassB : TestInheritanceBase
{
    [Parameter]
    public int SpecificParameterB { get; set; }

    protected override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await base.ProcessAsync(cancellationToken);
        await WriteAsync($"SpecificB: {SpecificParameterB}", cancellationToken);
    }
}
