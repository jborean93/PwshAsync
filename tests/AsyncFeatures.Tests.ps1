BeforeAll {
    . "$PSScriptRoot/common.ps1"
}

Describe 'Test-InvokeInPipelineThread' {
    It 'Executes Func on pipeline thread' {
        $result = Test-InvokeInPipelineThread
        $result | Should -Be 'Match: True'
    }
}

Describe 'Test-InvokeInPipelineThreadAction' {
    It 'Executes Action on pipeline thread' {
        $result = Test-InvokeInPipelineThreadAction
        $result | Should -Be 'Match: True'
    }
}

Describe 'Test-CustomConstructor' {
    It 'Calls custom constructor' {
        $result = Test-CustomConstructor
        $result | Should -Be 'ConstructorCalled'
    }
}

Describe 'Test-Disposable' {
    BeforeEach {
        [SampleCmdlet.TestDisposable]::ResetDisposeCallCount()
    }

    It 'Calls Dispose when cmdlet completes' {
        $result = Test-Disposable
        $result | Should -Be 'Processed'

        # Dispose should have been called
        [SampleCmdlet.TestDisposable]::GetDisposeCallCount() | Should -Be 1
    }
}

Describe 'Test-DisposableWithException' {
    BeforeEach {
        [SampleCmdlet.TestDisposableWithException]::ResetDisposeCallCount()
    }

    It 'Calls Dispose even when exception occurs' {
        { Test-DisposableWithException -ErrorAction Stop } | Should -Throw '*ProcessAsync error*'

        # Dispose should have been called even though exception occurred
        [SampleCmdlet.TestDisposableWithException]::GetDisposeCallCount() | Should -Be 1
    }
}

Describe 'Test-StopCmdlet' {
    It 'Stops processing <Block> block with stop signal' -TestCases @(
        @{ Block = 'Begin' }
        @{ Block = 'Process' }
        @{ Block = 'End' }
    ) {
        param ($State)

        $result = Invoke-InCustomPowerShell -ScriptBlock {
            param($State, $StartTrigger)

            Test-StopCmdlet -StartTrigger $StartTrigger -Stage $State.Block -Delay 30000
        } -StopOnStartup -Timeout 10 -State @{ Block = $Block }

        $result | Should -BeNullOrEmpty
    }
}
