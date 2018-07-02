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

    $CodeSign = -not $env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT -and ($IsWindows -or -not $IsCoreCLR)

    if ($CodeSign) {
        $astDir = "$PSScriptRoot/.build/tools/store/AzureSignTool/1.0.1/"
        $AzureSignToolPath = "$astDir/AzureSignTool.exe"
        if (-not (Test-Path $AzureSignToolPath)) {
            New-Item $astDir -ItemType Directory -ErrorAction Ignore | Out-Null
            Invoke-WebRequest https://github.com/vcsjones/AzureSignTool/releases/download/1.0.1/AzureSignTool.zip `
                -OutFile "$astDir/AzureSignTool.zip"
            Expand-Archive "$astDir/AzureSignTool.zip" -DestinationPath $astDir
        }

        $nstDir = "$PSScriptRoot/.build/tools/store/NuGetKeyVaultSignTool/1.1.4/"
        $NuGetKeyVaultSignTool = "$nstDir/tools/net471/NuGetKeyVaultSignTool.exe"
        if (-not (Test-Path $NuGetKeyVaultSignTool)) {
            New-Item $nstDir -ItemType Directory -ErrorAction Ignore | Out-Null
            Invoke-WebRequest https://github.com/onovotny/NuGetKeyVaultSignTool/releases/download/v1.1.4/NuGetKeyVaultSignTool.1.1.4.nupkg `
                -OutFile "$nstDir/NuGetKeyVaultSignTool.zip"
            Expand-Archive "$nstDir/NuGetKeyVaultSignTool.zip" -DestinationPath $nstDir
        }

        $MSBuildArgs += "-p:AzureSignToolPath=$AzureSignToolPath"
        $MSBuildArgs += "-p:NuGetKeyVaultSignTool=$NuGetKeyVaultSignTool"
    }
    else {
        $MSBuildArgs += '-p:CodeSign=false'
    }
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet build --configuration $Configuration '-warnaserror:CS1591' @MSBuildArgs
exec dotnet pack --no-restore --no-build --configuration $Configuration -o $artifacts @MSBuildArgs
exec dotnet test --no-restore --no-build --configuration $Configuration '-clp:Summary' `
    "$PSScriptRoot/test/CommandLineUtils.Tests/McMaster.Extensions.CommandLineUtils.Tests.csproj" `
    @MSBuildArgs

write-host -f magenta 'Done'
