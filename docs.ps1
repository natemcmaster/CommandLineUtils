#requires -version 5
param(
    [switch]$Serve
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

$docfxVersion = '2.28.2'
$docfxRoot = "$PSScriptRoot/.nuget/docfx.console/$docfxVersion"
$docfx = "$docfxRoot/tools/docfx.exe"
if (-not (Test-Path $docfx)) {
    mkdir -p $docfxRoot -ErrorAction Ignore | Out-Null
    $temp = (New-TemporaryFile).FullName + ".zip"
    Invoke-WebRequest "https://www.nuget.org/api/v2/package/docfx.console/$docfxVersion" -O $temp
    Expand-Archive $temp -DestinationPath $docfxRoot
    Remove-Item $temp
}

$arguments = @()
if ($Serve) {
    $arguments += '--serve'
}
& $docfx docfx_project/docfx.json @arguments