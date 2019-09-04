#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    [switch]
    $ci,
    [switch]
    $sign,
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

$CodeSign = $sign -or ($ci -and -not $isPr -and $IsWindows)

if ($CodeSign) {
    exec dotnet tool restore
    $MSBuildArgs += '-p:CodeSign=true'
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore
if ($ci) {
    exec dotnet msbuild /t:UpdateCiSettings @MSBuildArgs
}
exec dotnet build --configuration $Configuration '-warnaserror:CS1591' @MSBuildArgs
exec dotnet pack --no-restore --no-build --configuration $Configuration -o $artifacts @MSBuildArgs
exec dotnet build --configuration $Configuration "$PSScriptRoot/docs/samples/samples.sln"

[string[]] $testArgs=@()
if ($PSVersionTable.PSEdition -eq 'Core' -and -not $IsWindows) {
    $testArgs += '--framework','netcoreapp2.2'
}
if ($env:TF_BUILD) {
    $testArgs += '--logger', 'trx'
}

exec dotnet test --no-restore --no-build --configuration $Configuration '-clp:Summary' `
    "$PSScriptRoot/test/CommandLineUtils.Tests/McMaster.Extensions.CommandLineUtils.Tests.csproj" `
    @testArgs `
    @MSBuildArgs

exec dotnet test --no-restore --no-build --configuration $Configuration '-clp:Summary' `
    "$PSScriptRoot/test/Hosting.CommandLine.Tests/McMaster.Extensions.Hosting.CommandLine.Tests.csproj" `
    @testArgs `
    @MSBuildArgs

write-host -f magenta 'Done'
