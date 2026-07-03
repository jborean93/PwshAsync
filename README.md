# Jborean.PwshAsync

A C# source generator that enables writing truly asynchronous PowerShell cmdlets using async/await patterns.

This is still not complete, more testing is required to be done before submitting this to nuget.org.

## Overview

PowerShell cmdlets traditionally run synchronously on the pipeline thread. This generator allows you to write cmdlets with async methods (`BeginAsync`, `ProcessAsync`, `EndAsync`) while maintaining full compatibility with PowerShell's pipeline, progress reporting, and all standard cmdlet features.

## Features

- Write async cmdlets using familiar `async`/`await` patterns
- Full support for all PowerShell cmdlet features (ShouldProcess, WriteVerbose, WriteProgress, etc.)
- Type-safe output validation
- Code is generated at compile time with no extra runtime requirements or runtime assembly conflicts

## Installation

_Note: This is still not released._

Add the package to your PowerShell module project:

```xml
<ItemGroup>
  <PackageReference Include="Jborean.PwshAsync" />
</ItemGroup>
```

## Quick Start

```csharp
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Jborean.PwshAsync;

[AsyncPSCmdlet(VerbsDiagnostic.Test, "AsyncExample")]
public partial class TestAsyncExampleCmdlet : AsyncPSCmdlet<string>
{
    [Parameter(Mandatory = true)]
    public string Name { get; set; } = "";

    public override async Task ProcessAsync(CancellationToken cancellationToken)
    {
        await WriteVerboseAsync($"Processing {Name}...", cancellationToken);

        // Simulate async work
        await Task.Delay(1000, cancellationToken);

        // Write output to pipeline
        await WriteAsync($"Hello, {Name}!", cancellationToken);
    }
}
```

Usage in PowerShell:
```powershell
Test-AsyncExample -Name "World" -Verbose
# Output: Hello, World!
# Verbose: Processing World...
```

## API

### AsyncPSCmdlet

Base class for async cmdlets. Inherit from this for cmdlets without typed output, or use `AsyncPSCmdlet<T>` for typed output.

```csharp
public abstract class AsyncPSCmdlet : IDisposable
{
    // Async block methods, define these to run each block in an async
    // context.
    protected virtual Task BeginAsync(CancellationToken cancellationToken);
    protected virtual Task ProcessAsync(CancellationToken cancellationToken);
    protected virtual Task EndAsync(CancellationToken cancellationToken);

    // Synchronous hooks that run before and after each async belock.
    // Used to more easily perform synchronous actions in the same pipeline
    // thread as a normal cmdlet.
    protected virtual void BeforeBegin();
    protected virtual void AfterBegin();
    protected virtual void BeforeProcess();
    protected virtual void AfterProcess();
    protected virtual void BeforeEnd();
    protected virtual void AfterEnd();

    // Pipeline thread invocation. Used to invoke actions in the same thread
    // as the PowerShell pipeline and await the result.
    protected Task InvokeInPipelineThreadAsync(Action action, CancellationToken cancellationToken);
    protected Task<T> InvokeInPipelineThreadAsync<T>(Func<T> func, CancellationToken cancellationToken);

    // ShouldProcess and ShouldContinue
    protected Task<bool> ShouldProcessAsync(string target, string action, CancellationToken cancellationToken);
    protected Task<bool> ShouldContinueAsync(string query, string caption, CancellationToken cancellationToken);
    protected Task<ShouldContinueResult> ShouldContinueAsync(string query, string caption, bool hasSecurityImpact, bool yesToAll, bool noToAll, CancellationToken cancellationToken);
    protected Task<ShouldContinueResult> ShouldContinueAsync(string query, string caption, bool yesToAll, bool noToAll, CancellationToken cancellationToken);

    // Error handling
    protected Task ThrowTerminatingErrorAsync(ErrorRecord errorRecord, CancellationToken cancellationToken);
    protected Task WriteErrorAsync(ErrorRecord errorRecord, CancellationToken cancellationToken);

    // PowerShell streams
    protected Task WriteWarningAsync(string message, CancellationToken cancellationToken);
    protected Task WriteVerboseAsync(string message, CancellationToken cancellationToken);
    protected Task WriteDebugAsync(string message, CancellationToken cancellationToken);
    protected Task WriteInformationAsync(InformationRecord informationRecord, CancellationToken cancellationToken);
    protected Task WriteInformationAsync(object messageData, string[] tags, CancellationToken cancellationToken);
    protected Task WriteHostAsync(string message, bool noNewLine, CancellationToken cancellationToken);
    protected Task WriteProgressAsync(ProgressRecord progressRecord, CancellationToken cancellationToken);

    // Provides access to the PSCmdlet wrapping this async cmdlet. This is ok
    // to use the Before*/After* virtual methods but be careful using this in
    // the *Async blocks. Use InvokeInPipelineThreadAsync to access this in
    // those scenarios.
    public PSCmdlet DangerousGetCmdlet();

    // Provide custom disposing logic for the cmdlet.
    protected virtual void Dispose(bool disposing);
}
```

### AsyncPSCmdlet&lt;T&gt;

Generic base class for async cmdlets with typed output.

```csharp
public abstract class AsyncPSCmdlet<T> : AsyncPSCmdlet
{
    // WriteObject equivalent but typed to the cmdlet's declared output
    // type. Use PSObject as T if writing multiple output types.
    protected Task WriteAsync(T sendToPipeline, CancellationToken cancellationToken);
    protected Task WriteAsync(T sendToPipeline, bool enumerateCollection, CancellationToken cancellationToken);
}
```

### ShouldContinueResult

Result type for `ShouldContinueAsync` methods with "Yes to All" / "No to All" support.

```csharp
public class ShouldContinueResult
{
    public bool ShouldContinue { get; init; }
    public bool YesToAll { get; init; }
    public bool NoToAll { get; init; }
}
```

### AsyncPSCmdletAttribute

Attribute for marking async cmdlets. Supports all `CmdletAttribute` properties.

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class AsyncPSCmdletAttribute : Attribute
{
    public AsyncPSCmdletAttribute(string verb, string noun);

    public string Verb { get; }
    public string Noun { get; }
    public ConfirmImpact ConfirmImpact { get; set; }
    public string? DefaultParameterSetName { get; set; }
    public string? HelpUri { get; set; }
    public RemotingCapability RemotingCapability { get; set; }
    public bool SupportsPaging { get; set; }
    public bool SupportsShouldProcess { get; set; }
    public bool SupportsTransactions { get; set; }
}
```

## Examples

TODO: Fill this in.

## How It Works

The source generator:

1. Detects classes marked with `[AsyncPSCmdlet]` attribute
2. Generates a PowerShell-compatible `PSCmdlet` wrapper class
3. Calls the required blocks on the user defined `AsyncPSCmdlet` class
4. Provides a mechanism to perform PSCmdlet operations on the PSCmdlet/pipeline thread and await the result

All cmdlet features work as expected:
- Parameter binding (including `ValueFromPipeline`)
- Pipeline processing
- Progress reporting
- Error handling
- ShouldProcess confirmations
- All output streams (Verbose, Debug, Warning, Information, Error)

## Thread Safety

- All `Write*Async` methods marshal calls to the PowerShell pipeline thread
- Use `InvokeInPipelineThreadAsync` to safely access PSCmdlet members from async code
- `DangerousGetCmdlet()` provides direct access but should only be used from `Before*/After*` hooks or when you know you're on the pipeline thread

## Requirements

- .NET SDK 10.0 or later
- PowerShell 5.1, or 7.4+

## License

MIT License - see [LICENSE](./LICENSE) file for details
