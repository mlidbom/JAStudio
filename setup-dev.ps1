#!/usr/bin/env pwsh
# Windows development environment setup for JAStudio
# Run this once after cloning the repository to set up the development environment.

param(
    [switch]$SkipVenv,
    [switch]$SkipSubmodules
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $ScriptDir

try {
    Write-Host "`n=== JAStudio Windows Development Setup ===" -ForegroundColor Cyan

    # 1. Enable long paths for Git (required for Compze submodule)
    if(-not $SkipSubmodules) {
        Write-Host "`nEnabling Git long paths support..." -ForegroundColor Yellow
        git config --global core.longpaths true
        Write-Host "✓ Git long paths enabled" -ForegroundColor Green
    }

    # 2. Initialize git submodules
    if(-not $SkipSubmodules) {
        Write-Host "`nInitializing git submodules..." -ForegroundColor Yellow
        git submodule update --init --recursive
        if($LASTEXITCODE -ne 0) { throw "Git submodule initialization failed" }
        Write-Host "✓ Submodules initialized" -ForegroundColor Green
    }

    # 3. Create Python virtual environment
    if(-not $SkipVenv) {
        if(Test-Path "venv") {
            Write-Host "`nvenv directory already exists, skipping creation" -ForegroundColor Yellow
        } else {
            Write-Host "`nCreating Python virtual environment..." -ForegroundColor Yellow
            python -m venv venv
            if($LASTEXITCODE -ne 0) { throw "Python venv creation failed" }
            Write-Host "✓ Virtual environment created" -ForegroundColor Green
        }

        # 4. Install Python dependencies
        Write-Host "`nInstalling Python dependencies..." -ForegroundColor Yellow
        & venv\Scripts\pip.exe install -r requirements.txt --quiet
        if($LASTEXITCODE -ne 0) { throw "Python dependency installation failed" }
        Write-Host "✓ Python dependencies installed" -ForegroundColor Green
    }

    # 5. Build .NET solution
    Write-Host "`nBuilding .NET solution..." -ForegroundColor Yellow
    dotnet build JAStudio.slnx -c Debug
    if($LASTEXITCODE -ne 0) { throw ".NET build failed" }
    Write-Host "✓ .NET build successful" -ForegroundColor Green

    Write-Host "`n=== Setup Complete ===" -ForegroundColor Green
    Write-Host "`nYou can now:" -ForegroundColor Cyan
    Write-Host "  • Run tests: dotnet test JAStudio.slnx" -ForegroundColor White
    Write-Host "  • Run Python tests: venv\Scripts\python.exe -m pytest" -ForegroundColor White
    Write-Host "  • Full validation: .\full-build.ps1" -ForegroundColor White

} catch {
    Write-Host "`n❌ Setup failed: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}
