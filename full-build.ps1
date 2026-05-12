#!/usr/bin/env pwsh
# Full build: compile .NET, regenerate Python type stubs, and run basedpyright.
# Use this when the .NET API surface has changed, or as a pre-commit validation.
#
# For a fast build that only compiles .NET, run:
#   dotnet build src\src_dotnet\JAStudio.slnx -c Debug

param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $ScriptDir

try {
    $Sep = [IO.Path]::DirectorySeparatorChar

    # 1. Build .NET
    Write-Host "`n=== Building .NET ($Configuration) ===" -ForegroundColor Cyan
    #dotnet clean "src${Sep}src_dotnet${Sep}JAStudio.slnx" -c $Configuration
    dotnet build "src${Sep}src_dotnet${Sep}JAStudio.slnx" -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

    # 2. Regenerate Python type stubs
    Write-Host "`n=== Regenerating Python type stubs ===" -ForegroundColor Cyan
    pwsh -NoProfile -ExecutionPolicy Bypass -File "src${Sep}src_dotnet${Sep}regenerate-stubs.ps1" -OnlyTargetTypes -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Stub generation failed" }

    # 3. Run basedpyright
    Write-Host "`n=== Running basedpyright ===" -ForegroundColor Cyan
    if ($IsWindows -or (-not (Test-Path variable:IsWindows))) {
        cmd /c "$ScriptDir\basedpyright-wrapper.bat"
    } else {
        & "$ScriptDir/basedpyright-wrapper.sh"
    }
    if ($LASTEXITCODE -ne 0) { throw "basedpyright reported errors" }

    Write-Host "`n=== Full build succeeded ===" -ForegroundColor Green
}
finally {
    Pop-Location
}
