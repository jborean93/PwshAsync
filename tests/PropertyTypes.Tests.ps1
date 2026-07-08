BeforeAll {
    . "$PSScriptRoot/common.ps1"
}

Describe 'Test-NullableReference' {
    It 'Accepts null value' {
        $result = Test-NullableReference
        $result | Should -Be 'Value: <null>'
    }

    It 'Accepts non-null value' {
        $result = Test-NullableReference -OptionalValue 'Hello'
        $result | Should -Be 'Value: Hello'
    }

    It 'Has string OptionalValue parameter' {
        $cmd = Get-Command Test-NullableReference
        $param = $cmd.Parameters['OptionalValue']
        $param.ParameterType.FullName | Should -Be 'System.String'
    }
}

Describe 'Test-NullableValueType' {
    It 'Accepts null value' {
        $result = Test-NullableValueType
        $result | Should -Be 'Number: <null>'
    }

    It 'Accepts non-null value' {
        $result = Test-NullableValueType -OptionalNumber 42
        $result | Should -Be 'Number: 42'
    }

    It 'Accepts zero' {
        $result = Test-NullableValueType -OptionalNumber 0
        $result | Should -Be 'Number: 0'
    }

    It 'Has Nullable<Int32> OptionalNumber parameter' {
        $cmd = Get-Command Test-NullableValueType
        $param = $cmd.Parameters['OptionalNumber']
        $param.ParameterType.Name | Should -Be 'Nullable`1'
        $param.ParameterType.GenericTypeArguments[0].FullName | Should -Be 'System.Int32'
    }
}

Describe 'Test-ArrayParameter' {
    It 'Accepts empty array' {
        $result = Test-ArrayParameter
        $result | Should -Be 'Items: '
    }

    It 'Accepts single item' {
        $result = Test-ArrayParameter -Items 'One'
        $result | Should -Be 'Items: One'
    }

    It 'Accepts multiple items' {
        $result = Test-ArrayParameter -Items 'Alpha', 'Beta', 'Gamma'
        $result | Should -Be 'Items: Alpha, Beta, Gamma'
    }

    It 'Has string[] Items parameter' {
        $cmd = Get-Command Test-ArrayParameter
        $param = $cmd.Parameters['Items']
        $param.ParameterType.FullName | Should -Be 'System.String[]'
    }
}

Describe 'Test-GenericCollection' {
    It 'Accepts empty collection' {
        $result = Test-GenericCollection
        $result | Should -Be 'Items: '
    }

    It 'Accepts single item' {
        $result = Test-GenericCollection -Items 'One'
        $result | Should -Be 'Items: One'
    }

    It 'Accepts multiple items' {
        $result = Test-GenericCollection -Items 'Red', 'Green', 'Blue'
        $result | Should -Be 'Items: Red, Green, Blue'
    }

    It 'Has List<string> Items parameter' {
        $cmd = Get-Command Test-GenericCollection
        $param = $cmd.Parameters['Items']
        $param.ParameterType.Name | Should -Be 'List`1'
        $param.ParameterType.GenericTypeArguments[0].FullName | Should -Be 'System.String'
    }
}
