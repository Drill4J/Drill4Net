Param (
[string]$path
)

if ($path -eq $null -or $path -eq ""){
$path=$PSScriptRoot
}
$path='C:\Users\Viktoria_Nikolaeva\Documents\sources\Drill4Net3\'
Write-Host("path "+$path)

Get-ChildItem $path -include obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

Start-Sleep -Seconds 5