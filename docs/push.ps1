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

    if ($env:CI) {
        exec git config --global credential.helper store
        Add-Content "$HOME\.git-credentials" "https://x-access-token:${ApiToken}@github.com`n"
        exec git config --global user.email 'actions@users.noreply.github.com'
        exec git config --global user.name "GitHub Workflow (${env:GITHUB_ACTOR})"
        $SourceCommit = $env:GITHUB_SHA
    }

    if (-not $SourceCommit) {
        Push-Location $PSScriptRoot
        $SourceCommit = $(git rev-parse HEAD)
        Pop-Location
    }
    exec git commit --allow-empty -m "Generate documentation from $SourceCommit"
    exec git push --quiet origin gh-pages
}
finally {
    Pop-Location
}
