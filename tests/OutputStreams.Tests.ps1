param([string]$ModuleConfiguration)

BeforeAll {
    . "$PSScriptRoot/common.ps1" -ModuleConfiguration $ModuleConfiguration
}

Describe 'Test-VerboseOutput' {
    It 'Writes to verbose stream and output' {
        $output = $null
        $verbose = . {
            Test-VerboseOutput -Verbose | Set-Variable -Name output
        } 4>&1

        $output | Should -Be 'Output'
        $verbose.Message | Should -Be 'Verbose message'
    }
}

Describe 'Test-WarningOutput' {
    It 'Writes to warning stream and output' {
        $output = $null
        $warning = . {
            Test-WarningOutput | Set-Variable -Name output
        } 3>&1

        $output | Should -Be 'Output'
        $warning.Message | Should -Be 'Warning message'
    }
}

Describe 'Test-ErrorOutput' {
    It 'Writes to error stream and continues' {
        $output = $null
        $err = . {
            Test-ErrorOutput -ErrorAction Continue | Set-Variable -Name output
        } 2>&1

        $output | Should -Be 'Continued'
        $err.Exception.Message | Should -Be 'Error message'
    }

    It 'Throws with ErrorAction Stop and does not continue' {
        $output = $null
        {
            . {
                Test-ErrorOutput -ErrorAction Stop | Set-Variable -Name output
            } 2>&1
        } | Should -Throw -ErrorId 'TestError,SampleCmdlet.TestErrorOutput_PSCmdlet'

        $output | Should -BeNullOrEmpty
    }
}

Describe 'Test-DebugOutput' {
    It 'Writes to debug stream and output' {
        $output = $null
        $debug = . {
            Test-DebugOutput -Debug | Set-Variable -Name output
        } 5>&1

        $output | Should -Be 'Output'
        $debug.Message | Should -Be 'Debug message'
    }
}

Describe 'Test-InformationOutput' {
    It 'Writes to information stream and output' {
        $output = $null
        $info = . {
            Test-InformationOutput -InformationAction Continue | Set-Variable -Name output
        } 6>&1

        $output | Should -Be 'Output'
        $info.MessageData | Should -Be 'Information message'
    }
}

Describe 'Test-InformationWithTags' {
    It 'Writes information with tags' {
        $output = $null
        $info = . {
            Test-InformationWithTags -InformationAction Continue | Set-Variable -Name output
        } 6>&1

        $output | Should -Be 'Output'
        $info.MessageData | Should -Be 'InfoData'
        $info.Tags | Should -Contain 'Tag1'
        $info.Tags | Should -Contain 'Tag2'
    }
}

Describe 'Test-ProgressOutput' {
    It 'Writes progress and output' {
        $testHost = [PwshAsyncTests.TestHost]::new($Host)

        $result = Invoke-InCustomPowerShell -PSHost $testHost -ScriptBlock {
            Test-ProgressOutput
        }

        $result | Should -Be 'Output'
        $testHost.UI.HostOutput | Should -Match 'PROGRESS: \d+ - Processing - Working on it'
    }
}

Describe 'Test-HostOutput' {
    It 'Writes to host' {
        $testHost = [PwshAsyncTests.TestHost]::new($Host)

        $output = $null
        $result = . {
            Invoke-InCustomPowerShell -PSHost $testHost -ScriptBlock {
                Test-HostOutput
            } | Set-Variable -Name output
        } 6>&1

        $output | Should -Be 'Output'
        $result | Should -BeOfType ([InformationRecord])
        $result.MessageData | Should -Be 'Host message'
        $result.Tags | Should -Contain 'PSHost'
        $testHost.UI.HostOutput.Trim() | Should -Be 'Host message'
    }
}

Describe 'Test-HostOutputNoNewLine' {
    It 'Concatenates with noNewLine true and adds line with noNewLine false' {
        $testHost = [PwshAsyncTests.TestHost]::new($Host)

        $output = $null
        $result = . {
            Invoke-InCustomPowerShell -PSHost $testHost -ScriptBlock {
                Test-HostOutputNoNewLine
            } | Set-Variable -Name output
        } 6>&1

        $output | Should -Be 'Output'
        $result.Count | Should -Be 2
        $result[0] | Should -BeOfType ([InformationRecord])
        $result[0].MessageData | Should -Be 'Part1'
        $result[0].Tags | Should -Contain 'PSHost'
        $result[1] | Should -BeOfType ([InformationRecord])
        $result[1].MessageData | Should -Be 'Part2'
        $result[1].Tags | Should -Contain 'PSHost'

        # Part1 (noNewLine=true) and Part2 (noNewLine=false) should be on same line
        $testHost.UI.HostOutput.Trim() | Should -Be 'Part1Part2'
    }
}
