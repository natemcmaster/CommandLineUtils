#!/usr/bin/env pwsh
param(
    [switch]$Serve,
    [switch]$Install,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

if (-not (Test-Path variable:\IsCoreCLR)) {
    $IsWindows = $true
}

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

    $docfxVersion = '2.56.6'
    $docfxRoot = "$buildRoot/packages/docfx.console/$docfxVersion"
    $docfx = "$docfxRoot/tools/docfx.exe"
    if (-not (Test-Path $docfx)) {
        mkdir -p $docfxRoot -ErrorAction Ignore | Out-Null
        $temp = (New-TemporaryFile).FullName + ".zip"
        Invoke-WebRequest "https://www.nuget.org/api/v2/package/docfx.console/$docfxVersion" -OutFile $temp
        Expand-Archive $temp -DestinationPath $docfxRoot
        Remove-Item $temp
        if ($Install) {
            exit 1
        }
    }

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

        if ($IsWindows) {
            exec $docfx docs/docfx.json @arguments
        }
        else {
            exec mono $docfx docs/docfx.json @arguments
        }
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
