BeforeAll {
    . "$PSScriptRoot/common.ps1"
}

Describe 'Test-ExceptionInBeginAsync' {
    It 'Throws exception from BeginAsync' {
        { Test-ExceptionInBeginAsync -ErrorAction Stop } | Should -Throw '*BeginAsync error*'
    }
}

Describe 'Test-ExceptionInProcessAsync' {
    It 'Throws exception from ProcessAsync' {
        { Test-ExceptionInProcessAsync -ErrorAction Stop } | Should -Throw '*ProcessAsync error*'
    }
}

Describe 'Test-ExceptionInBeforeBegin' {
    It 'Throws exception from BeforeBegin' {
        { Test-ExceptionInBeforeBegin -ErrorAction Stop } | Should -Throw '*BeforeBegin error*'
    }
}

Describe 'Test-TerminatingErrorInAsync' {
    It 'Throws terminating error and does not run After block' {
        { Test-TerminatingErrorInAsync } | Should -Throw '*Terminating error in async block*'
    }
}
