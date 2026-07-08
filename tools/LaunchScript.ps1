using namespace System.IO

[CmdletBinding()]
param(
    [Parameter()]
    [switch]
    $NoExit
)

$ErrorActionPreference = 'Stop'

$targetFramework = 'net10.0'
$projectRoot = [Path]::GetFullPath([Path]::Combine($PSScriptRoot, '..'))
$assemblyPath = [Path]::Combine($projectRoot, 'tests', 'SampleCmdlet', 'bin', 'Debug', $targetFramework, 'SampleCmdlet.dll')
Import-Module -Name $assemblyPath

if ($NoExit) {
    $Host.EnterNestedPrompt()
}
