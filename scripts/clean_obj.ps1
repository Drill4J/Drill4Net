Param (
[string]$path
)

if ($path -eq $null -or $path -eq ""){
$path=$PSScriptRoot
}

Write-Host("path "+$path)

Get-ChildItem $path -include obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

Start-Sleep -Seconds 5