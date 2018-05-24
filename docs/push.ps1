#requires -version 5
param(
    [string]$SourceCommit,
    [switch]$DryRun,
    [switch]$AppVeyor
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

Import-Module -Force -Scope Local "$PSScriptRoot/../src/common.psm1"

$workdir = "$PSScriptRoot/../.build/docs/gh-pages"
Push-Location $workdir
try {

    if ($AppVeyor) {
        exec git config --global credential.helper store
        Add-Content "$HOME\.git-credentials" "https://$($env:access_token):x-oauth-basic@github.com`n"
        exec git config --global user.email $env:APPVEYOR_REPO_COMMIT_AUTHOR
        exec git config --global user.name $env:APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL
        $SourceCommit = $env:APPVEYOR_REPO_COMMIT
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
