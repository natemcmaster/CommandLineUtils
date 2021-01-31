#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    [switch]
    $ci
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

Import-Module -Force -Scope Local "$PSScriptRoot/src/common.psm1"

if (!$Configuration) {
    $Configuration = if ($ci) { 'Release' } else { 'Debug' }
}

if (-not (Test-Path variable:\IsCoreCLR)) {
    $IsWindows = $true
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet tool restore

[string[]] $formatArgs=@()
if ($ci) {
    $formatArgs += '--check'
}

exec dotnet tool run dotnet-format -- -v detailed @formatArgs "$PSScriptRoot/CommandLineUtils.sln"
exec dotnet tool run dotnet-format -- -v detailed @formatArgs "$PSScriptRoot/docs/samples/samples.sln"
exec dotnet build --configuration $Configuration '-warnaserror:CS1591'
exec dotnet pack --no-build --configuration $Configuration -o $artifacts
exec dotnet build --configuration $Configuration "$PSScriptRoot/docs/samples/samples.sln"

[string[]] $testArgs=@()
if (-not $IsWindows) {
    $testArgs += '-p:TestFullFramework=false'
}

exec dotnet test --no-build --configuration $Configuration `
    --collect:"XPlat Code Coverage" `
    @testArgs

write-host -f green 'BUILD SUCCEEDED'
