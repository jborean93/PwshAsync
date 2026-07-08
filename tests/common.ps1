using namespace System.Collections
using namespace System.Management.Automation
using namespace System.Management.Automation.Host
using namespace System.Threading

Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;

namespace PwshAsyncTests
{

    public class TestHost : PSHost
    {
        private readonly PSHost _origHost;

        public override Guid InstanceId { get; } = Guid.NewGuid();
        public override string Name => "TestHost";
        public override Version Version => new Version(1, 0);
        public override CultureInfo CurrentCulture => _origHost.CurrentCulture;
        public override CultureInfo CurrentUICulture => _origHost.CurrentUICulture;
        public override PSHostUserInterface UI { get; }

        public TestHost(PSHost host)
        {
            _origHost = host;
            UI = new TestHostUserInterface(host.UI);
        }

        public override void SetShouldExit(int exitCode)
        {
            _origHost.SetShouldExit(exitCode);
        }

        public override void EnterNestedPrompt()
        {
            _origHost.EnterNestedPrompt();
        }

        public override void ExitNestedPrompt()
        {
            _origHost.ExitNestedPrompt();
        }

        public override void NotifyBeginApplication()
        {
            _origHost.NotifyBeginApplication();
        }

        public override void NotifyEndApplication()
        {
            _origHost.NotifyEndApplication();
        }
    }

    public class TestHostUserInterface : PSHostUserInterface
    {
        private readonly PSHostUserInterface _origUI;

        public override PSHostRawUserInterface RawUI { get; }
        public Queue<object> PromptResponses { get; } = new Queue<object>();
        public List<object> PromptQueries { get; } = new List<object>();

        private StringBuilder _hostOutput = new StringBuilder();
        public string HostOutput => _hostOutput.ToString();

        public TestHostUserInterface(PSHostUserInterface ui)
        {
            _origUI = ui;
            RawUI = ui.RawUI;
        }

        public override void Write(string value)
        {
            WriteToHost(value, false);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            WriteToHost(value, false);
        }

        public override void WriteLine(string value)
        {
            WriteToHost(value, true);
        }

        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            WriteToHost(value, true);
        }

        public override void WriteErrorLine(string value)
        {
            WriteToHost($"ERROR: {value}", true);
        }

        public override void WriteDebugLine(string message)
        {
            WriteToHost($"DEBUG: {message}", true);
        }

        public override void WriteVerboseLine(string message)
        {
            WriteToHost($"VERBOSE: {message}", true);
        }

        public override void WriteWarningLine(string message)
        {
            WriteToHost($"WARNING: {message}", true);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            WriteToHost($"PROGRESS: {sourceId} - {record.Activity} - {record.StatusDescription}", true);
        }

        private void WriteToHost(string value, bool newLine)
        {
            lock (_hostOutput)
            {
                _hostOutput.Append(value);
                if (newLine)
                {
                    _hostOutput.AppendLine();
                }
            }
        }

        public override string ReadLine()
        {
            return _origUI.ReadLine();
        }

        public override SecureString ReadLineAsSecureString()
        {
            return _origUI.ReadLineAsSecureString();
        }

        public override Dictionary<string, PSObject> Prompt(
            string caption,
            string message,
            Collection<FieldDescription> descriptions)
        {
            return _origUI.Prompt(caption, message, descriptions);
        }

        public override int PromptForChoice(
            string caption,
            string message,
            Collection<ChoiceDescription> choices,
            int defaultChoice)
        {
            lock (PromptQueries)
            {
                PromptQueries.Add(new
                {
                    Type = "PromptForChoice",
                    Caption = caption,
                    Message = message,
                    Choices = choices,
                    DefaultChoice = defaultChoice
                });
            }

            lock (PromptResponses)
            {
                if (PromptResponses.Count == 0)
                {
                    throw new InvalidOperationException("No prompt response queued for PromptForChoice");
                }

                return (int)PromptResponses.Dequeue();
            }
        }

        public override PSCredential PromptForCredential(
            string caption,
            string message,
            string userName,
            string targetName)
        {
            return _origUI.PromptForCredential(caption, message, userName, targetName);
        }

        public override PSCredential PromptForCredential(
            string caption,
            string message,
            string userName,
            string targetName,
            PSCredentialTypes allowedCredentialTypes,
            PSCredentialUIOptions options)
        {
            return _origUI.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
        }

        public void AddPromptResponse(object response)
        {
            lock (PromptResponses)
            {
                PromptResponses.Enqueue(response);
            }
        }
    }
}
'@

function Invoke-InCustomPowerShell {
    <#
    .SYNOPSIS
    Executes a ScriptBlock in a separate PowerShell runspace with optional timeout management.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ScriptBlock]$ScriptBlock,

        [Parameter()]
        [int]$Timeout = -1,

        [Parameter()]
        [PSHost]$PSHost,

        [Parameter()]
        [switch]
        $StopOnStartup,

        [Parameter()]
        [IDictionary]$State
    )

    if ($StopOnStartup -and $Timeout -eq -1) {
        throw "Timeout must be specified when StopOnStartup is used."
    }

    $ps = $beginTask = $stopTask = $startEvent = $null
    try {
        $startEvent = [ManualResetEventSlim]::new($false)

        $ps = [PowerShell]::Create()
        $null = $ps.AddScript({
            param ($Assembly, $ScriptBlock)

            $ErrorActionPreference = 'Stop'

            $Assembly | ForEach-Object {
                Import-Module -Assembly $_ -Force
            }

            ${function:<Invoke>} = $ScriptBlock.Ast.GetScriptBlock()
        }).AddParameters(@{
            Assembly = @(
                (Get-Module -Name SampleCmdlet).ImplementingAssembly
            )
            ScriptBlock = $ScriptBlock
        }).AddStatement()

        # We use a separate command as we can get a better error message if the
        # ScriptBlock fails.
        $null = $ps.AddCommand('<Invoke>')
        if ($null -ne $State) {
            $null = $ps.AddParameter('State', $State)
        }
        if ($StopOnStartup) {
            $null = $ps.AddParameter('StartTrigger', $startEvent)
        }

        $inputCollection = [PSDataCollection[PSObject]]::new()
        $invocationSettings = [PSInvocationSettings]::new()
        if ($PSHost) {
            $invocationSettings.Host = $PSHost
        }
        $beginTask = $ps.BeginInvoke($inputCollection, $invocationSettings, $null, $null)

        if ($StopOnStartup) {
            $start = Get-Date
            while ($true) {
                if ($beginTask.IsCompleted) {
                    throw "PowerShell task completed before Stop could be triggered"
                }

                if ($startEvent.IsSet) {
                    break
                }

                $elapsed = (Get-Date) - $start
                if ($elapsed.TotalSeconds -gt $Timeout) {
                    throw "Test timed out waiting for PowerShell task to start"
                }

                Start-Sleep -Milliseconds 100
            }

            $stopTask = $ps.BeginStop($null, $null)
        }

        $start = Get-Date
        while (-not $beginTask.AsyncWaitHandle.WaitOne(100)) {
            if ($Timeout -gt -1 -and (((Get-Date) - $start).TotalSeconds -gt $Timeout)) {
                throw "Test timed out waiting for PowerShell task to complete"
            }
        }

        if ($stopTask) {
            $ps.EndStop($stopTask)
            $stopTask = $null
        }

        try {
            $ps.EndInvoke($beginTask)
        }
        catch [PipelineStoppedException] {
            # Expected with Stop, this is ignored.
        }

        # This can happen if the ScriptBlock contained a single command that
        # was not valid. Weird that EAP = 'Stop' doesn't have it through in
        # EndInvoke() but better to check just in case.
        if ($ps.HadErrors -and $ps.Streams.Error[0] -notlike "*The pipeline has been stopped.*") {
            throw "PowerShell reported an error during execution:"
        }

        foreach ($warn in $ps.Streams.Warning) {
            $PSCmdlet.WriteWarning($warn)
        }
        foreach ($verbose in $ps.Streams.Verbose) {
            $PSCmdlet.WriteVerbose($verbose)
        }
        foreach ($debug in $ps.Streams.Debug) {
            $PSCmdlet.WriteDebug($debug)
        }
        foreach ($info in $ps.Streams.Information) {
            $PSCmdlet.WriteInformation($info)
        }
    }
    catch {
        $errorDetails = @(
            if ($stopTask -and $stopTask.IsCompleted) {
                try {
                    $ps.EndStop($stopTask)
                }
                catch {
                    "StopTaskException: $_"
                }
            }

            if ($beginTask -and $beginTask.IsCompleted) {
                try {
                    $ps.EndInvoke($beginTask)
                }
                catch {
                    $msg = if ($_.Exception.InnerException) {
                        $_.Exception.InnerException
                    }
                    else {
                        $_
                    }
                    "TaskException: $msg"
                }
            }

            if ($ps -and $ps.Streams.Error.Count -gt 0) {
                "PowerShell had $($ps.Streams.Error.Count) error(s) in the stream:"

                foreach ($err in $ps.Streams.Error) {
                    [string]$err
                    $err.ScriptStackTrace
                }
            }
        )

        [string]$msg = $_
        if ($errorDetails) {
            $msg += "`nErrorDetails:`n$($errorDetails -join "`n")"
        }

        $err = [ErrorRecord]::new(
            [Exception]::new($msg, $_.Exception),
            "PowerShellExecutionFailed",
            [ErrorCategory]::NotSpecified,
            $null)

        $PSCmdlet.ThrowTerminatingError($err)
    }
    finally {
        if ($startEvent) { $startEvent.Dispose() }
        if ($ps) { $ps.Dispose() }
    }
}

function Import-TestModule {
    <#
    .SYNOPSIS
    Imports a test module, automatically detecting the target framework from the .csproj file.

    .PARAMETER ModuleName
    The name of the module to import (must match the .csproj and .dll name).
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ModuleName
    )

    # Find the .csproj file
    $csprojPath = Join-Path $PSScriptRoot "SampleCmdlet" "$ModuleName.csproj"

    if (-not (Test-Path $csprojPath)) {
        throw "Could not find project file at: $csprojPath"
    }

    # Parse the .csproj to get TargetFramework
    [xml]$csproj = Get-Content $csprojPath
    $targetFramework = $csproj.Project.PropertyGroup.TargetFramework

    if (-not $targetFramework) {
        throw "Could not determine TargetFramework from $csprojPath"
    }

    # Build path to the DLL
    $dllPath = Join-Path $PSScriptRoot "SampleCmdlet" "bin" "Debug" $targetFramework "$ModuleName.dll"

    if (-not (Test-Path $dllPath)) {
        throw "Module DLL not found at: $dllPath. Please build the project first with 'dotnet build'."
    }

    # Import the module with -Force to reload after builds
    Import-Module $dllPath -Force
}

if (-not (Get-Module -Name SampleCmdlet -ErrorAction Ignore)) {
    Import-TestModule -ModuleName SampleCmdlet
}
