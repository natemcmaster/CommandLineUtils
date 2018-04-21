#requires -version 5
param(
    [switch]$Serve,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2

Push-Location "$PSScriptRoot/../"

try {

    $buildRoot = "$PSScriptRoot/../.build/docs"
    $targetDir = "$buildRoot/gh-pages"
    New-Item -ItemType Directory $buildRoot -ErrorAction Ignore

    if (-not (git worktree list --porcelain | Select-String 'gh-pages')) {
        & git worktree add $targetDir gh-pages
    }

    $docfxVersion = '2.33.2'
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
    git rm --quiet --force -r .
    Pop-Location

    if (-not $NoBuild) {
        & dotnet build -c Release
    }

    $arguments = @()
    try {
        if ($Serve) {
            $arguments += '--serve'
        }
        & $docfx docs/docfx.json @arguments
    }
    finally {
        Push-Location $targetDir
        git add ./
        git --no-pager status -s
        Pop-Location
    }
}
finally {
    Pop-Location
}
