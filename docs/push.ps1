#requires -version 5
param(
    [string]$SourceCommit,
    [string]$ApiToken,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

Import-Module -Force -Scope Local "$PSScriptRoot/../src/common.psm1"

$workdir = "$PSScriptRoot/../.build/docs/gh-pages"
Push-Location $workdir
try {

    if ($env:TF_BUILD) {
        exec git config --global credential.helper store
        Add-Content "$HOME\.git-credentials" "https://$($ApiToken):x-oauth-basic@github.com`n"
        exec git config --global user.email 'azuredevops@microsoft.com'
        exec git config --global user.name 'Azure Pipelines'
        $SourceCommit = $env:BUILD_SOURCEVERSION
    }

    if (-not $SourceCommit) {
        Push-Location $PSScriptRoot
        $SourceCommit = $(git rev-parse HEAD)
        Pop-Location
    }
    exec git commit -m "Generate documentation from $SourceCommit"
    exec git push --quiet origin gh-pages
}
finally {
    Pop-Location
}
