using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

namespace SampleCmdlet;

/* Tests: Custom constructor.
   Verifies: Constructor is called and initializes fields */
[PSAsyncCmdlet(VerbsDiagnostic.Test, "CustomConstructor")]
public partial class TestCustomConstructor : PSAsyncCmdlet<string>
{
    private readonly string _constructorValue;

    public TestCustomConstructor()
    {
        _constructorValue = "ConstructorCalled";
    }

    protected override Task ProcessAsync(CancellationToken cancellationToken)
    {
        return WriteAsync(_constructorValue, cancellationToken);
    }
}
