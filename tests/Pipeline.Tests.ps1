using namespace System.Management.Automation

param([string]$ModuleConfiguration)

BeforeAll {
    . "$PSScriptRoot/common.ps1" -ModuleConfiguration $ModuleConfiguration
}

Describe 'Test-ValueFromPipeline' {
    It 'Has ValueFromPipeline attribute' {
        $cmd = Get-Command Test-ValueFromPipeline
        $param = $cmd.Parameters['InputObject']
        $attr = $param.Attributes.Where({ $_ -is [ParameterAttribute] })[0]
        $attr.ValueFromPipeline | Should -Be $true
    }

    It 'Processes single pipeline object' {
        $result = 'TestValue' | Test-ValueFromPipeline
        $result | Should -Be 'Input: TestValue'
    }

    It 'Processes multiple pipeline objects' {
        $result = 'First', 'Second', 'Third' | Test-ValueFromPipeline
        $result.Count | Should -Be 3
        $result[0] | Should -Be 'Input: First'
        $result[1] | Should -Be 'Input: Second'
        $result[2] | Should -Be 'Input: Third'
    }

    It 'Works without pipeline input' {
        $result = Test-ValueFromPipeline -InputObject 'Direct'
        $result | Should -Be 'Input: Direct'
    }
}

Describe 'Test-ValueFromPipelineByPropertyName' {
    It 'Has ValueFromPipelineByPropertyName attribute' {
        $cmd = Get-Command Test-ValueFromPipelineByPropertyName
        $param = $cmd.Parameters['Name']
        $attr = $param.Attributes.Where({ $_ -is [ParameterAttribute] })[0]
        $attr.ValueFromPipelineByPropertyName | Should -Be $true
    }

    It 'Binds property from PSCustomObject' {
        $obj = [PSCustomObject]@{ Name = 'TestName' }
        $result = $obj | Test-ValueFromPipelineByPropertyName
        $result | Should -Be 'Name: TestName'
    }

    It 'Processes multiple objects with Name property' {
        $objects = @(
            [PSCustomObject]@{ Name = 'First' }
            [PSCustomObject]@{ Name = 'Second' }
            [PSCustomObject]@{ Name = 'Third' }
        )
        $result = $objects | Test-ValueFromPipelineByPropertyName
        $result.Count | Should -Be 3
        $result[0] | Should -Be 'Name: First'
        $result[1] | Should -Be 'Name: Second'
        $result[2] | Should -Be 'Name: Third'
    }

    It 'Works with named parameter' {
        $result = Test-ValueFromPipelineByPropertyName -Name 'Direct'
        $result | Should -Be 'Name: Direct'
    }
}

Describe 'Test-MixedPipelineProperties' {
    It 'StaticParam remains constant across pipeline records' {
        $result = 'A', 'B', 'C' | Test-MixedPipelineProperties -StaticParam 'Static'

        # Filter to ProcessAsync outputs
        $processOutputs = $result | Where-Object { $_ -like 'ProcessAsync:*' }
        $processOutputs.Count | Should -Be 3

        # All ProcessAsync calls should have same StaticParam
        $processOutputs[0] | Should -Be 'ProcessAsync: StaticParam=Static, PipelineParam=A'
        $processOutputs[1] | Should -Be 'ProcessAsync: StaticParam=Static, PipelineParam=B'
        $processOutputs[2] | Should -Be 'ProcessAsync: StaticParam=Static, PipelineParam=C'
    }

    It 'StaticParam is set in BeginAsync before any ProcessAsync' {
        $result = 'X' | Test-MixedPipelineProperties -StaticParam 'Foo'

        $beginAsync = $result | Where-Object { $_ -like 'BeginAsync:*' }
        $beginAsync | Should -Be 'BeginAsync: StaticParam=Foo, PipelineParam='
    }

    It 'PipelineParam is unset in Begin phase' {
        $result = 'X' | Test-MixedPipelineProperties -StaticParam 'Test'

        # All Begin phase outputs should have empty PipelineParam
        $beforeBegin = $result | Where-Object { $_ -like 'BeforeBegin:*' }
        $beginAsync = $result | Where-Object { $_ -like 'BeginAsync:*' }
        $afterBegin = $result | Where-Object { $_ -like 'AfterBegin:*' }

        $beforeBegin | Should -Be 'BeforeBegin: StaticParam=Test, PipelineParam='
        $beginAsync | Should -Be 'BeginAsync: StaticParam=Test, PipelineParam='
        $afterBegin | Should -Be 'AfterBegin: StaticParam=Test, PipelineParam='
    }

    It 'PipelineParam is set in all Process phase hooks' {
        $result = 'Value' | Test-MixedPipelineProperties -StaticParam 'S'

        $beforeProcess = $result | Where-Object { $_ -like 'BeforeProcess:*' }
        $processAsync = $result | Where-Object { $_ -like 'ProcessAsync:*' }
        $afterProcess = $result | Where-Object { $_ -like 'AfterProcess:*' }

        $beforeProcess | Should -Be 'BeforeProcess: StaticParam=S, PipelineParam=Value'
        $processAsync | Should -Be 'ProcessAsync: StaticParam=S, PipelineParam=Value'
        $afterProcess | Should -Be 'AfterProcess: StaticParam=S, PipelineParam=Value'
    }

    It 'End phase has last PipelineParam value' {
        $result = 'A', 'B', 'C' | Test-MixedPipelineProperties -StaticParam 'S'

        # End phase should have the last pipeline value
        $beforeEnd = $result | Where-Object { $_ -like 'BeforeEnd:*' }
        $endAsync = $result | Where-Object { $_ -like 'EndAsync:*' }
        $afterEnd = $result | Where-Object { $_ -like 'AfterEnd:*' }

        $beforeEnd | Should -Be 'BeforeEnd: StaticParam=S, PipelineParam=C'
        $endAsync | Should -Be 'EndAsync: StaticParam=S, PipelineParam=C'
        $afterEnd | Should -Be 'AfterEnd: StaticParam=S, PipelineParam=C'
    }

    It 'Verifies complete lifecycle order with multiple pipeline inputs' {
        $result = 'First', 'Second' | Test-MixedPipelineProperties -StaticParam 'Static'

        # Expected order:
        # BeforeBegin -> BeginAsync -> AfterBegin
        # BeforeProcess (First) -> ProcessAsync (First) -> AfterProcess (First)
        # BeforeProcess (Second) -> ProcessAsync (Second) -> AfterProcess (Second)
        # BeforeEnd -> EndAsync -> AfterEnd

        $result.Count | Should -Be 12  # 3 Begin + 6 Process (2 records × 3) + 3 End

        $result[0] | Should -BeLike 'BeforeBegin:*'
        $result[1] | Should -BeLike 'BeginAsync:*'
        $result[2] | Should -BeLike 'AfterBegin:*'
        $result[3] | Should -Be 'BeforeProcess: StaticParam=Static, PipelineParam=First'
        $result[4] | Should -Be 'ProcessAsync: StaticParam=Static, PipelineParam=First'
        $result[5] | Should -Be 'AfterProcess: StaticParam=Static, PipelineParam=First'
        $result[6] | Should -Be 'BeforeProcess: StaticParam=Static, PipelineParam=Second'
        $result[7] | Should -Be 'ProcessAsync: StaticParam=Static, PipelineParam=Second'
        $result[8] | Should -Be 'AfterProcess: StaticParam=Static, PipelineParam=Second'
        $result[9] | Should -Be 'BeforeEnd: StaticParam=Static, PipelineParam=Second'
        $result[10] | Should -Be 'EndAsync: StaticParam=Static, PipelineParam=Second'
        $result[11] | Should -Be 'AfterEnd: StaticParam=Static, PipelineParam=Second'
    }
}
