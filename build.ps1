#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
	[switch]
	$IsOfficialBuild,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$MSBuildArgs
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

Import-Module -Force -Scope Local "$PSScriptRoot/src/common.psm1"

#
# Main
#

if (!$Configuration) {
    $Configuration = if ($env:CI -or $IsOfficialBuild) { 'Release' } else { 'Debug' }
}

if ($IsOfficialBuild) {
	$MSBuildArgs += '-p:CI=true'
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet build --configuration $Configuration '-warnaserror:CS1591' @MSBuildArgs
exec dotnet pack --no-restore --no-build --configuration $Configuration -o $artifacts @MSBuildArgs
exec dotnet test --no-restore --no-build --configuration $Configuration '-clp:Summary' `
    "$PSScriptRoot/test/CommandLineUtils.Tests/McMaster.Extensions.CommandLineUtils.Tests.csproj" `
    @MSBuildArgs

write-host -f magenta 'Done'
