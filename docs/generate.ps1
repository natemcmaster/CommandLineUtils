#!/usr/bin/env pwsh
param(
    [switch]$Serve,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

Import-Module -Force -Scope Local "$PSScriptRoot/../src/common.psm1"

Push-Location "$PSScriptRoot/../"

try {

    if (-not $NoBuild) {
        exec dotnet build "$PSScriptRoot/samples/samples.sln"
    }

    $buildRoot = "$PSScriptRoot/../.build/docs"
    $targetDir = "$buildRoot/gh-pages"
    mkdir -p $buildRoot -ErrorAction Ignore | Out-Null

    exec git worktree prune 2>&1 | out-null
    if (-not (git worktree list --porcelain | Select-String 'gh-pages')) {
        exec git fetch --quiet -u origin gh-pages:gh-pages
        exec git worktree add $targetDir gh-pages 2>&1 | out-null
    }

    # Restore docfx as a local .NET tool
    exec dotnet tool restore

    Push-Location $targetDir
    exec git reset --hard
    exec git rm --quiet --force -r .
    Pop-Location

    if (-not $NoBuild) {
        exec dotnet build -c Release
    }

    [string[]] $arguments = @()
    try {
        if ($Serve) {
            $arguments += '--serve'
        }

        exec dotnet tool run docfx docs/docfx.json @arguments
    }
    finally {
        Push-Location $targetDir
        exec git config core.safecrlf false # suppress warning about CRLF
        exec git add ./ 2>&1 | Out-Null
        Pop-Location
    }
}
finally {
    Pop-Location
}
