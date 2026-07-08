using namespace System.Management.Automation

param([string]$ModuleConfiguration)

BeforeAll {
    . "$PSScriptRoot/common.ps1" -ModuleConfiguration $ModuleConfiguration
}

Describe 'Test-NoParameters' {
    It 'Outputs success message' {
        $result = Test-NoParameters
        $result | Should -Be 'Success'
    }

    It 'Has no parameters defined' {
        $cmd = Get-Command Test-NoParameters
        # Should only have common parameters
        $commonParams = @(
            'Verbose', 'Debug', 'ErrorAction', 'WarningAction', 'InformationAction',
            'ErrorVariable', 'WarningVariable', 'InformationVariable',
            'OutVariable', 'OutBuffer', 'PipelineVariable'

            # ProgressAction only exists in PowerShell 7.x+
            if ($PSVersionTable.PSVersion.Major -ge 7) {
                'ProgressAction'
            }
        )

        $cmd.Parameters.Keys.Where({ $_ -notin $commonParams }).Count | Should -Be 0
    }
}

Describe 'Test-SimpleParameter' {
    It 'Has Name parameter defined' {
        $cmd = Get-Command Test-SimpleParameter
        $cmd.Parameters.Keys | Should -Contain 'Name'
    }

    It 'Outputs the parameter value' {
        $result = Test-SimpleParameter -Name 'Foo'
        $result | Should -Be 'Foo'
    }

    It 'Outputs empty string when parameter not provided' {
        $result = Test-SimpleParameter
        $result | Should -Be ''
    }
}

Describe 'Test-MultipleParameters' {
    It 'Has all three parameters defined' {
        $cmd = Get-Command Test-MultipleParameters
        $cmd.Parameters.Keys | Should -Contain 'Name'
        $cmd.Parameters.Keys | Should -Contain 'Count'
        $cmd.Parameters.Keys | Should -Contain 'IsEnabled'
    }

    It 'Outputs all parameter values' {
        $result = Test-MultipleParameters -Name 'Test' -Count 42 -IsEnabled $true
        $result | Should -Be 'Name=Test, Count=42, IsEnabled=True'
    }

    It 'Outputs default values when not provided' {
        $result = Test-MultipleParameters
        $result | Should -Be 'Name=, Count=0, IsEnabled=False'
    }
}

Describe 'Test-MandatoryParameter' {
    It 'Has mandatory attribute in metadata' {
        $cmd = Get-Command Test-MandatoryParameter
        $param = $cmd.Parameters['Required']
        $attr = $param.Attributes.Where({ $_ -is [ParameterAttribute] })[0]
        $attr.Mandatory | Should -BeTrue
    }

    It 'Throws error when parameter not provided' {
        {
            Invoke-InCustomPowerShell -ScriptBlock {
                Test-MandatoryParameter
            }
        } | Should -Throw "*Cannot process command because of one or more missing mandatory parameters: Required*"
    }

    It 'Outputs value when provided' {
        $result = Test-MandatoryParameter -Required 'test'
        $result | Should -Be 'test'
    }
}

Describe 'Test-PositionalParameter' {
    It 'Has correct position attributes' {
        $cmd = Get-Command Test-PositionalParameter
        $firstParam = $cmd.Parameters['First']
        $secondParam = $cmd.Parameters['Second']

        $firstAttr = $firstParam.Attributes.Where({ $_ -is [ParameterAttribute] })[0]
        $secondAttr = $secondParam.Attributes.Where({ $_ -is [ParameterAttribute] })[0]

        $firstAttr.Position | Should -Be 0
        $secondAttr.Position | Should -Be 1
    }

    It 'Accepts positional parameters' {
        $result = Test-PositionalParameter 'Foo' 'Bar'
        $result.Count | Should -Be 2
        $result[0] | Should -Be 'Foo'
        $result[1] | Should -Be 'Bar'
    }

    It 'Accepts named parameters' {
        $result = Test-PositionalParameter -First 'Alpha' -Second 'Beta'
        $result.Count | Should -Be 2
        $result[0] | Should -Be 'Alpha'
        $result[1] | Should -Be 'Beta'
    }
}

Describe 'Test-ValidationAttribute' {
    It 'Has ValidateNotNullOrEmpty attribute' {
        $cmd = Get-Command Test-ValidationAttribute
        $param = $cmd.Parameters['Value']
        $attr = $param.Attributes.Where({ $_ -is [ValidateNotNullOrEmptyAttribute] })
        $attr.Count | Should -Be 1
    }

    It 'Accepts valid non-empty value' {
        $result = Test-ValidationAttribute -Value 'valid'
        $result | Should -Be 'valid'
    }

    It 'Rejects empty string' {
        { Test-ValidationAttribute -Value '' -ErrorAction Stop } | Should -Throw
    }

    It 'Rejects null' {
        { Test-ValidationAttribute -Value $null -ErrorAction Stop } | Should -Throw
    }
}

Describe 'Test-MultipleAttributes' {
    It 'Has both Mandatory and ValidateSet attributes' {
        $cmd = Get-Command Test-MultipleAttributes
        $param = $cmd.Parameters['Color']

        $paramAttr = $param.Attributes.Where({ $_ -is [ParameterAttribute] })[0]
        $validateSetAttr = $param.Attributes.Where({ $_ -is [ValidateSetAttribute] })[0]

        $paramAttr.Mandatory | Should -Be $true
        $validateSetAttr.ValidValues | Should -Contain 'Red'
        $validateSetAttr.ValidValues | Should -Contain 'Green'
        $validateSetAttr.ValidValues | Should -Contain 'Blue'
    }

    It 'Accepts valid value from set' {
        $result = Test-MultipleAttributes -Color 'Red'
        $result | Should -Be 'Red'
    }

    It 'Rejects value not in set' {
        { Test-MultipleAttributes -Color 'Yellow' -ErrorAction Stop } | Should -Throw
    }

    It 'Requires parameter value' {
        {
            Invoke-InCustomPowerShell -ScriptBlock {
                Test-MultipleAttributes
            }
        } | Should -Throw "*Cannot process command because of one or more missing mandatory parameters: Color*"
    }
}

Describe 'Test-AllParameterAttributes' {
    It 'Preserves all ParameterAttribute properties' {
        $cmd = Get-Command Test-AllParameterAttributes
        $param = $cmd.Parameters['InputValue']
        $paramAttr = $param.Attributes.Where({ $_ -is [ParameterAttribute] })[0]

        $paramAttr.Mandatory | Should -Be $true
        $paramAttr.Position | Should -Be 0
        $paramAttr.ValueFromPipeline | Should -Be $true
        $paramAttr.ValueFromPipelineByPropertyName | Should -Be $true
        $paramAttr.ValueFromRemainingArguments | Should -Be $false
        $paramAttr.HelpMessage | Should -Be 'The input value'
        $paramAttr.ParameterSetName | Should -Be 'TestSet'
        $paramAttr.DontShow | Should -Be $false
    }

    It 'Works correctly with all properties set' {
        $result = Test-AllParameterAttributes -InputValue 'Test'
        $result | Should -Be 'Input: Test'
    }
}
