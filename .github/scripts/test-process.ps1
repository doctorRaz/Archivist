param(
    [Parameter(Mandatory)]
    [string]$Name,

    [Parameter(Mandatory)]
    [int]$Number
)

# Получаем значения от вызывающего workflow.
Write-Host "Input name: $Name"
Write-Host "Input number: $Number"

# Изменяем значения, чтобы проверить передачу результата обратно в Actions.
$resultName = "$Name-ps"
$resultNumber = $Number + 10

Write-Host "Output name: $resultName"
Write-Host "Output number: $resultNumber"

# Возвращаем объект вызывающему PowerShell-коду.
[PSCustomObject]@{
    Name   = $resultName
    Number = $resultNumber
}
