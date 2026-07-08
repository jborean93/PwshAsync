using namespace System.Management.Automation

param([string]$ModuleConfiguration)

BeforeAll {
    . "$PSScriptRoot/common.ps1" -ModuleConfiguration $ModuleConfiguration
}

Describe 'Test-ShouldProcess' {
    It 'Has SupportsShouldProcess attribute' {
        $cmd = Get-Command Test-ShouldProcess
        $cmdAttr = $cmd.ImplementingType.GetCustomAttributes([CmdletAttribute], $false)[0]
        $cmdAttr.SupportsShouldProcess | Should -Be $true
    }

    It 'Has WhatIf parameter' {
        $cmd = Get-Command Test-ShouldProcess
        $cmd.Parameters.Keys | Should -Contain 'WhatIf'
    }

    It 'Has Confirm parameter' {
        $cmd = Get-Command Test-ShouldProcess
        $cmd.Parameters.Keys | Should -Contain 'Confirm'
    }

    It 'Executes action when user approves' {
        $result = Test-ShouldProcess -Target 'TestTarget' -Action 'TestAction' -Confirm:$false
        $result | Should -Be 'Processed: TestTarget with TestAction'
    }

    It 'Shows what would happen with WhatIf' {
        $testHost = [PwshAsyncTests.TestHost]::new($Host)

        $result = Invoke-InCustomPowerShell -PSHost $testHost -ScriptBlock {
            Test-ShouldProcess -Target 'TestTarget' -Action 'TestAction' -WhatIf
        }
        # WhatIf should skip the action
        $result | Should -Be 'Skipped: TestTarget'

        $testHost.UI.HostOutput.Trim() | Should -Be 'What if: Performing the operation "TestAction" on target "TestTarget".'
    }
}

Describe 'Test-ParameterSets' {
    It 'Has DefaultParameterSetName attribute' {
        $cmd = Get-Command Test-ParameterSets
        $cmdAttr = $cmd.ImplementingType.GetCustomAttributes([CmdletAttribute], $false)[0]
        $cmdAttr.DefaultParameterSetName | Should -Be 'ByName'
    }

    It 'Has two parameter sets' {
        $cmd = Get-Command Test-ParameterSets
        $cmd.ParameterSets.Count | Should -Be 2
        $cmd.ParameterSets.Name | Should -Contain 'ByName'
        $cmd.ParameterSets.Name | Should -Contain 'ById'
    }

    It 'Uses ByName parameter set with Name parameter' {
        $result = Test-ParameterSets -Name 'TestName'
        $result | Should -Be 'ByName: TestName'
    }

    It 'Uses ById parameter set with Id parameter' {
        $result = Test-ParameterSets -Id 42
        $result | Should -Be 'ById: 42'
    }

    It 'Requires either Name or Id' {
        {
            Invoke-InCustomPowerShell -ScriptBlock {
                Test-ParameterSets
            }
        } | Should -Throw '*Cannot process command because of one or more missing mandatory parameters: Name*'
    }
}

Describe 'Test-ConfirmImpact' {
    It 'Has ConfirmImpact.High attribute' {
        $cmd = Get-Command Test-ConfirmImpact
        $cmdAttr = $cmd.ImplementingType.GetCustomAttributes([CmdletAttribute], $false)[0]
        $cmdAttr.ConfirmImpact | Should -Be ([ConfirmImpact]::High)
    }

    It 'Executes normally' {
        $result = Test-ConfirmImpact -Message 'Test'
        $result | Should -Be 'Test'
    }
}

Describe 'Test-HelpUri' {
    It 'Has HelpUri attribute' {
        $cmd = Get-Command Test-HelpUri
        $cmdAttr = $cmd.ImplementingType.GetCustomAttributes([CmdletAttribute], $false)[0]
        $cmdAttr.HelpUri | Should -Be 'https://example.com/help'
    }

    It 'Executes normally' {
        $result = Test-HelpUri
        $result | Should -Be 'Help available'
    }

    It 'Help URI is accessible via Get-Help' {
        $help = Get-Help Test-HelpUri
        $help.relatedLinks.navigationLink.uri | Should -Be 'https://example.com/help'
    }
}

Describe 'Test-RemotingCapability' {
    It 'Has RemotingCapability.PowerShell attribute' {
        $cmd = Get-Command Test-RemotingCapability
        $cmdAttr = $cmd.ImplementingType.GetCustomAttributes([CmdletAttribute], $false)[0]
        $cmdAttr.RemotingCapability | Should -Be ([RemotingCapability]::PowerShell)
    }

    It 'Executes normally' {
        $result = Test-RemotingCapability
        $result | Should -Be 'Test'
    }
}

Describe 'Test-SupportsPaging' {
    It 'Has SupportsPaging attribute' {
        $cmd = Get-Command Test-SupportsPaging
        $cmdAttr = $cmd.ImplementingType.GetCustomAttributes([CmdletAttribute], $false)[0]
        $cmdAttr.SupportsPaging | Should -Be $true
    }

    It 'Has First and Skip parameters' {
        $cmd = Get-Command Test-SupportsPaging
        $cmd.Parameters.Keys | Should -Contain 'First'
        $cmd.Parameters.Keys | Should -Contain 'Skip'
        $cmd.Parameters.Keys | Should -Contain 'IncludeTotalCount'
    }

    It 'Executes normally' {
        $result = Test-SupportsPaging
        $result | Should -Be 'Paging enabled'
    }
}

Describe 'Test-ShouldContinue' {
    It 'Respects user response' {
        $testHost = [PwshAsyncTests.TestHost]::new($Host)
        $testHost.UI.AddPromptResponse(0)  # Yes (first choice)

        $result = Invoke-InCustomPowerShell -PSHost $testHost -ScriptBlock {
            'Item1' | Test-ShouldContinue
        }

        $result | Should -Be 'Processed: Item1'
        $testHost.UI.PromptQueries.Count | Should -Be 1
    }

    It 'Skips when user declines' {
        $testHost = [PwshAsyncTests.TestHost]::new($Host)
        $testHost.UI.AddPromptResponse(1)  # No (second choice)

        $result = Invoke-InCustomPowerShell -PSHost $testHost -ScriptBlock {
            'Item1' | Test-ShouldContinue
        }

        $result | Should -Be 'Skipped: Item1'
    }
}

Describe 'Test-ShouldContinueYesToAll' {
    It 'Uses YesToAll/NoToAll functionality' {
        $testHost = [PwshAsyncTests.TestHost]::new($Host)
        $testHost.UI.AddPromptResponse(1)  # Yes (with YesToAll/NoToAll ref params)

        $result = Invoke-InCustomPowerShell -PSHost $testHost -ScriptBlock {
            'Item1' | Test-ShouldContinueYesToAll
        }

        # Should process the item
        $result | Should -Be 'Processed: Item1'
        $testHost.UI.PromptQueries.Count | Should -Be 1
    }
}

