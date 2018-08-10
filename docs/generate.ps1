#requires -version 5
param(
    [switch]$Serve,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

Import-Module -Force -Scope Local "$PSScriptRoot/../src/common.psm1"

Push-Location "$PSScriptRoot/../"

try {

    exec dotnet build "$PSScriptRoot/samples/samples.sln"

    $buildRoot = "$PSScriptRoot/../.build/docs"
    $targetDir = "$buildRoot/gh-pages"
    mkdir -p $buildRoot -ErrorAction Ignore | Out-Null

    if (-not (git worktree list --porcelain | Select-String 'gh-pages')) {
        exec git fetch --quiet -u origin gh-pages:gh-pages
        exec git worktree add $targetDir gh-pages 2>&1 | out-null
    }

    $docfxVersion = '2.37.2'
    $docfxRoot = "$buildRoot/packages/docfx.console/$docfxVersion"
    $docfx = "$docfxRoot/tools/docfx.exe"
    if (-not (Test-Path $docfx)) {
        mkdir -p $docfxRoot -ErrorAction Ignore | Out-Null
        $temp = (New-TemporaryFile).FullName + ".zip"
        Invoke-WebRequest "https://www.nuget.org/api/v2/package/docfx.console/$docfxVersion" -O $temp
        Expand-Archive $temp -DestinationPath $docfxRoot
        Remove-Item $temp
    }

    Push-Location $targetDir
    exec git rm --quiet --force -r .
    Pop-Location

    if (-not $NoBuild) {
        exec dotnet build -c Release
    }

    $arguments = @()
    try {
        if ($Serve) {
            $arguments += '--serve'
        }
        exec $docfx docs/docfx.json @arguments
    }
    finally {
        Push-Location $targetDir
        exec git config core.safecrlf false # suppress warning about CRLF
        exec git add ./
        Write-Host "Files that changed in docs:"
        exec git --no-pager status -s
        Pop-Location
    }
}
finally {
    Pop-Location
}
