#!/usr/bin/env pwsh
# Regenerate Python type stubs for JAStudio .NET assemblies
#
# Usage:
#   .\regenerate-stubs.ps1                          # Default Debug config
#   .\regenerate-stubs.ps1 -OnlyTargetTypes           # Fast - only JAStudio types
#   .\regenerate-stubs.ps1 -Configuration Release      # Use Release DLLs

param(
    [switch]$OnlyTargetTypes,
    [string]$Configuration = "Debug"
)

# Always work relative to script location
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $ScriptDir

$Sep = [IO.Path]::DirectorySeparatorChar

$StubGenRoot = Join-Path ".." ".." "submodules" "pythonnet-stub-generator"
$TypingsPath = Join-Path ".." ".." "typings"
$StubGenDll = Join-Path $StubGenRoot "csharp" "PythonNetStubTool" "bin" "Debug" "net11.0" "PythonNetStubGenerator.Tool.dll"

try {
    if (Test-Path $StubGenDll) {
        Write-Host "Stub generator already compiled, skipping build..." -ForegroundColor Gray
    } else {
        Write-Host "Building stub generator..." -ForegroundColor Cyan
        dotnet build (Join-Path $StubGenRoot "csharp" "PythonNetStubTool" "PythonNetStubGenerator.Tool.csproj")

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed!" -ForegroundColor Red
            exit 1
        }
    }

Write-Host "`nCleaning old JAStudio stubs..." -ForegroundColor Cyan
$jastudioPath = Join-Path $TypingsPath "JAStudio"
if (Test-Path $jastudioPath) {
    Remove-Item -Recurse -Force $jastudioPath
    Write-Host "Removed old JAStudio stubs" -ForegroundColor Yellow
}

$globalPath = Join-Path $TypingsPath "global_"
if (Test-Path $globalPath) {
    Remove-Item -Recurse -Force $globalPath
    Write-Host "Removed broken global_ stubs" -ForegroundColor Yellow
}

Write-Host "`nGenerating stubs..." -ForegroundColor Cyan

$UiRid = if ($IsWindows -or (-not (Test-Path variable:IsWindows))) { "win-x64" } elseif ($IsMacOS) { "osx-x64" } else { "linux-x64" }

$args = @(
    $StubGenDll,
    "--dest-path", $TypingsPath,
    "--target-dlls", "JAStudio.Core${Sep}bin${Sep}${Configuration}${Sep}net11.0${Sep}JAStudio.Core.dll,JAStudio.PythonInterop${Sep}bin${Sep}${Configuration}${Sep}net11.0${Sep}JAStudio.PythonInterop.dll,JAStudio.Anki${Sep}bin${Sep}${Configuration}${Sep}net11.0${Sep}JAStudio.Anki.dll,JAStudio.UI${Sep}bin${Sep}${Configuration}${Sep}net11.0${Sep}${UiRid}${Sep}JAStudio.UI.dll,JAStudio.Anki.PythonInterop${Sep}bin${Sep}${Configuration}${Sep}net11.0${Sep}JAStudio.Anki.PythonInterop.dll"
)

if ($OnlyTargetTypes) {
    $args += "--only-target-types"
}

dotnet @args

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nStubs regenerated successfully!" -ForegroundColor Green
} else {
    Write-Host "`nStub generation failed!" -ForegroundColor Red
    exit 1
}
}
finally {
    Pop-Location
}