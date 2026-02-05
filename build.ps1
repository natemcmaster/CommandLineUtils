#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    [switch]
    $ci,
    [switch]
    $skipCoverage
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

Import-Module -Force -Scope Local "$PSScriptRoot/src/common.psm1"

if (!$Configuration) {
    $Configuration = if ($ci) { 'Release' } else { 'Debug' }
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet tool restore

[string[]] $formatArgs=@()
if ($ci) {
    $formatArgs += '--check'
}

[string[]] $testArgs = @('--no-build', '--configuration', $Configuration)
if (!$skipCoverage) {
    $testArgs += '--collect:"XPlat Code Coverage"'
}

exec dotnet tool run dotnet-format -- -v detailed @formatArgs "$PSScriptRoot/CommandLineUtils.sln"
exec dotnet tool run dotnet-format -- -v detailed @formatArgs "$PSScriptRoot/docs/samples/samples.sln"
exec dotnet build --configuration $Configuration '-warnaserror:CS1591'
exec dotnet pack --no-build --configuration $Configuration -o $artifacts
exec dotnet build --configuration $Configuration "$PSScriptRoot/docs/samples/samples.sln"
exec dotnet test @testArgs

write-host -f green 'BUILD SUCCEEDED'
