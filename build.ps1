#!/usr/bin/env powershell
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

function exec([string]$_cmd) {
    write-host -ForegroundColor DarkGray ">>> $_cmd $args"
    $ErrorActionPreference = 'Continue'
    & $_cmd @args
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -ne 0) {
        write-error "Failed with exit code $LASTEXITCODE"
        exit 1
    }
}

#
# Main
#

if (!$Configuration) {
    $Configuration = if ($env:CI) { 'Release' } else { 'Debug' }
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet restore
exec dotnet pack -c $Configuration -o $artifacts
exec dotnet test -c $Configuration "$PSScriptRoot/test/CommandLineUtils.Tests/McMaster.Extensions.CommandLineUtils.Tests.csproj"
