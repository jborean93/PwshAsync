using System;
using System.Linq;
using System.Threading.Tasks;
using TUnit.Core;
using TUnit.Assertions.Extensions;

namespace Jborean.PwshAsync.Tests;

public class GeneratorTests
{
    [Test]
    public async Task TestAsyncCmdlet_GeneratesBeginAndEndProcessing()
    {
        string modulePath = PowerShellHelper.GetTestProjectPath("SampleCmdlet");

        string output = await PowerShellHelper.InvokeCmdletAsync(
            modulePath,
            "Test-SimpleCmdletBlocks");

        string[] lines = output.ReplaceLineEndings("\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        await Assert.That(lines.Length).IsEqualTo(3);
        await Assert.That(lines[0]).IsEqualTo("BeginAsync");
        await Assert.That(lines[1]).IsEqualTo("ProcessAsync");
        await Assert.That(lines[2]).IsEqualTo("EndAsync");
    }
}
