using namespace System.Management.Automation

param([string]$ModuleConfiguration)

BeforeAll {
    . "$PSScriptRoot/common.ps1" -ModuleConfiguration $ModuleConfiguration
}

Describe 'Test-InheritanceSubclassA' {
    It 'Has CommonParameter from base class' {
        $cmd = Get-Command Test-InheritanceSubclassA
        $cmd.Parameters.ContainsKey('CommonParameter') | Should -BeTrue

        $param = $cmd.ImplementingType.GetProperty('CommonParameter').GetCustomAttributes([ParameterAttribute])
        $param | Should -Not -BeNullOrEmpty
        $param.Mandatory | Should -BeTrue
    }

    It 'Has SpecificParameterA from derived class' {
        $cmd = Get-Command Test-InheritanceSubclassA
        $cmd.Parameters.ContainsKey('SpecificParameterA') | Should -BeTrue
        $cmd.Parameters['SpecificParameterA'].IsMandatory | Should -BeFalse
    }

    It 'Works with only CommonParameter' {
        $result = Test-InheritanceSubclassA -CommonParameter 'test'
        $result | Should -HaveCount 1
        $result | Should -Be 'Common: test'
    }

    It 'Works with both CommonParameter and SpecificParameterA' {
        $result = Test-InheritanceSubclassA -CommonParameter 'base' -SpecificParameterA 'derived'
        $result | Should -HaveCount 2
        $result[0] | Should -Be 'Common: base'
        $result[1] | Should -Be 'SpecificA: derived'
    }
}

Describe 'Test-InheritanceSubclassB' {
    It 'Has CommonParameter from base class' {
        $cmd = Get-Command Test-InheritanceSubclassB
        $cmd.Parameters.ContainsKey('CommonParameter') | Should -BeTrue

        $param = $cmd.ImplementingType.GetProperty('CommonParameter').GetCustomAttributes([ParameterAttribute])
        $param | Should -Not -BeNullOrEmpty
        $param.Mandatory | Should -BeTrue
    }

    It 'Has SpecificParameterB from derived class' {
        $cmd = Get-Command Test-InheritanceSubclassB
        $cmd.Parameters.ContainsKey('SpecificParameterB') | Should -BeTrue
        $cmd.Parameters['SpecificParameterB'].ParameterType | Should -Be ([int])
    }

    It 'Works with CommonParameter and SpecificParameterB' {
        $result = @(Test-InheritanceSubclassB -CommonParameter 'base' -SpecificParameterB 42)
        $result | Should -HaveCount 2
        $result[0] | Should -Be 'Common: base'
        $result[1] | Should -Be 'SpecificB: 42'
    }
}

Describe 'Parameter Inheritance - Different Subclasses' {
    It 'SubclassA does not have SubclassB parameters' {
        $cmdA = Get-Command Test-InheritanceSubclassA
        $cmdA.Parameters.ContainsKey('SpecificParameterB') | Should -BeFalse
    }

    It 'SubclassB does not have SubclassA parameters' {
        $cmdB = Get-Command Test-InheritanceSubclassB
        $cmdB.Parameters.ContainsKey('SpecificParameterA') | Should -BeFalse
    }
}
