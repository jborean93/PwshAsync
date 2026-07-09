#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Build script for Jborean.PwshAsync
.PARAMETER Configuration
    Build configuration (Debug or Release)
.PARAMETER Task
    Task to run: Build or Test
.PARAMETER UsePackage
    Test against the built NuGet package instead of project reference (default for CI)
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [ValidateSet('Build', 'Test')]
    [string]$Task = 'Build',

    [switch]$UsePackage
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$outputDir = Join-Path $repoRoot 'output'
$testResultsDir = Join-Path $outputDir 'TestResults'
$srcProject = Join-Path $repoRoot 'src/Jborean.PwshAsync/Jborean.PwshAsync.csproj'
$sampleProject = Join-Path $repoRoot 'tests/SampleCmdlet/SampleCmdlet.csproj'
$testsDir = Join-Path $repoRoot 'tests'

function Build-Module {
    param (
        [Parameter(Mandatory)]
        [string]
        $ModuleName
    )

    $csprojPath = Join-Path $PSScriptRoot 'tests' $ModuleName "$ModuleName.csproj"
    [xml]$csharpProjectInfo = Get-Content $csprojPath
    $targetFrameworks = @(
        @($csharpProjectInfo.Project.PropertyGroup)[0].TargetFrameworks.Split(
            ';', [StringSplitOptions]::RemoveEmptyEntries)
    )

    if ($UsePackage) {
        Write-Host "Testing with NuGet package reference..." -ForegroundColor Yellow

        # Get the package version from the built nupkg
        $nupkg = Get-Item (Join-Path $outputDir '*.nupkg') | Select-Object -First 1
        if (-not $nupkg) {
            throw "No NuGet package found. Run 'Build' task first."
        }

        # Extract version from filename (Jborean.PwshAsync.1.0.0.nupkg)
        $version = $nupkg.BaseName -replace '^Jborean\.PwshAsync\.', ''

        # Add local NuGet source (idempotent)
        $sourceName = 'PwshAsync-Local'
        $existingSource = dotnet nuget list source 2>$null | Where-Object { $_ -match $sourceName }
        if ($existingSource) {
            dotnet nuget remove source $sourceName 2>$null | Out-Null
        }
        dotnet nuget add source $outputDir --name $sourceName | Out-Null

        try {
            # Clean to ensure fresh restore with package
            dotnet clean $sampleProject --nologo --verbosity minimal | Out-Null

            foreach ($framework in $targetFrameworks) {
                Write-Host "Building SampleCmdlet for $framework with package reference..." -ForegroundColor Cyan

                # Build with MSBuild properties to use PackageReference
                $dotnetArgs = @(
                    'publish', $sampleProject
                    '-c', $Configuration
                    '-f', $framework
                    '-p:TestWithPackage=true'
                    "-p:PwshAsyncTestVersion=$version"
                    '--output', "$outputDir/SampleCmdlet/$framework"
                    '--nologo'
                )
                dotnet @dotnetArgs
                if ($LASTEXITCODE -ne 0) {
                    throw "SampleCmdlet build failed for $framework with package reference"
                }
            }
        }
        finally {
            # Remove local NuGet source
            dotnet nuget remove source $sourceName 2>$null | Out-Null
        }
    }
    else {
        Write-Host "Testing with project reference..." -ForegroundColor Yellow

        # Build SampleCmdlet with project reference (normal mode)
        dotnet build $sampleProject -c $Configuration --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "SampleCmdlet build failed"
        }
    }
}

function Invoke-Build {
    Write-Host "Building and packaging Jborean.PwshAsync..." -ForegroundColor Cyan

    # Ensure output directory exists
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

    # Build and pack the source generator in one step
    dotnet pack $srcProject -c $Configuration -o $outputDir --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    $nupkg = Get-Item (Join-Path $outputDir '*.nupkg') | Select-Object -First 1
    Write-Host "Created package: $($nupkg.Name)" -ForegroundColor Green
}

function Invoke-Test {
    Write-Host "Running tests..." -ForegroundColor Cyan

    # Ensure test results directory exists
    New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null

    Build-Module -ModuleName 'SampleCmdlet'

    Import-Module -Name Pester -ErrorAction Stop

    $pesterConfig = [PesterConfiguration]::Default
    $pesterConfig.Output.Verbosity = 'Detailed'
    $pesterConfig.Run.Throw = $true
    $pesterConfig.TestResult.Enabled = $true
    $pesterConfig.TestResult.OutputPath = (Join-Path $testResultsDir 'Pester.xml')
    $pesterConfig.TestResult.OutputFormat = 'NUnitXml'

    $testContainer = New-PesterContainer -Path "$testsDir/*.Tests.ps1" -Data @{
        ModuleConfiguration = $UsePackage ? 'Package' : $Configuration
    }
    $pesterConfig.Run.Container = $testContainer

    # Run Pester tests
    Write-Host "Running Pester tests..." -ForegroundColor Cyan
    Invoke-Pester -Configuration $pesterConfig -WarningAction Ignore -ErrorAction Stop
}

# Main execution
try {
    Write-Host "=== Jborean.PwshAsync Build Script ===" -ForegroundColor Magenta
    Write-Host "Configuration: $Configuration" -ForegroundColor Magenta
    Write-Host "Task: $Task" -ForegroundColor Magenta
    Write-Host ""

    switch ($Task) {
        'Build' {
            Invoke-Build
        }
        'Test' {
            Invoke-Test
        }
    }

    Write-Host ""
    Write-Host "=== $Task Successful ===" -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "=== $Task Failed ===" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
