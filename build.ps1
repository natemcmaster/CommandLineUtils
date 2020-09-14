#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    [switch]
    $ci,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$MSBuildArgs
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

Import-Module -Force -Scope Local "$PSScriptRoot/src/common.psm1"

#
# Main
#

$isPr = $env:BUILD_REASON -eq 'PullRequest'

if ($env:CI -eq 'true') {
    $ci = $true
    & dotnet --info
}

if (!$Configuration) {
    $Configuration = if ($ci) { 'Release' } else { 'Debug' }
}

if ($ci) {
    $MSBuildArgs += '-p:CI=true'
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
exec dotnet build --configuration $Configuration '-warnaserror:CS1591' @MSBuildArgs
exec dotnet pack --no-restore --no-build --configuration $Configuration -o $artifacts @MSBuildArgs
exec dotnet build --configuration $Configuration "$PSScriptRoot/docs/samples/samples.sln"

[string[]] $testArgs=@()
if (-not $IsWindows) {
    $testArgs += '-p:TestFullFramework=false'
}
if ($env:TF_BUILD) {
    $testArgs += '--logger', 'trx'
}

exec dotnet test --no-restore --no-build --configuration $Configuration `
    --collect:"XPlat Code Coverage" `
    @testArgs `
    @MSBuildArgs

if ($ci) {
    exec dotnet tool run reportgenerator `
        "-reports:$PSScriptRoot/**/coverage.cobertura.xml" `
        "-targetdir:$PSScriptRoot/coverlet/reports" `
        "-reporttypes:Cobertura"
}

write-host -f magenta 'Done'
