param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

function exec([string]$_cmd) {
    Write-Host -ForegroundColor DarkGray ">>> $_cmd $args"
    & $_cmd @args
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Command exited with code $LASTEXITCODE"
    }
}

if (!$Configuration) {
    $Configuration = if ($env:CI) { 'Release' } else { 'Debug' }
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet restore /p:BuildNumber=t000
exec dotnet pack -c $Configuration -o $artifacts
exec dotnet test -c $Configuration "$PSScriptRoot/test/CommandLineUtils.Tests/McMaster.Extensions.CommandLineUtils.Tests.csproj"
