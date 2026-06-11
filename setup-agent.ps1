#!/usr/bin/env pwsh
# Background Agent Environment Setup
# This script is for GitHub Copilot background agents running in VS Code worktrees.
# DO NOT run manually - the human developer has already set up their environment.

param(
    [switch]$Force  # Override check and run anyway
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $ScriptDir

try {
    # Detect if running in a background agent context
    $IsBackgroundAgent = $env:COPILOT_AGENT_MODE -eq "background" -or $env:GITHUB_ACTIONS -eq "true"
    
    if (-not $IsBackgroundAgent -and -not $Force) {
        Write-Host "⚠️  This script is for background agents only." -ForegroundColor Yellow
        Write-Host "The human developer should use .\setup-dev.ps1 or set up manually." -ForegroundColor Yellow
        Write-Host "Use -Force to override this check." -ForegroundColor Yellow
        exit 0
    }

    Write-Host "`n=== Background Agent Environment Setup ===" -ForegroundColor Cyan
    Write-Host "Worktree: $(Split-Path -Leaf (Get-Location))" -ForegroundColor Gray

    # Check what needs to be done
    $needsGitConfig = -not (git config --global core.longpaths)
    $needsSubmodules = -not (Test-Path "submodules\pythonnet-stub-generator\.git") -or -not (Test-Path "src\jas_database\.git")
    $needsVenv = -not (Test-Path "venv\Scripts\python.exe")
    $needsBuild = -not (Test-Path "src\src_dotnet\JAStudio.Core\bin\Debug\net10.0\JAStudio.Core.dll")

    if (-not ($needsGitConfig -or $needsSubmodules -or $needsVenv -or $needsBuild)) {
        Write-Host "`n✓ Environment already set up - nothing to do" -ForegroundColor Green
        exit 0
    }

    # 1. Enable Git long paths if needed
    if ($needsGitConfig) {
        Write-Host "`nEnabling Git long paths..." -ForegroundColor Yellow
        git config --global core.longpaths true
        Write-Host "✓ Git long paths enabled" -ForegroundColor Green
    }

    # 2. Initialize git submodules if needed
    if ($needsSubmodules) {
        Write-Host "`nInitializing git submodules..." -ForegroundColor Yellow
        git submodule update --init --recursive
        if ($LASTEXITCODE -ne 0) { throw "Git submodule initialization failed" }
        Write-Host "✓ Submodules initialized" -ForegroundColor Green
    }

    # 3. Create Python venv if needed
    if ($needsVenv) {
        Write-Host "`nCreating Python virtual environment..." -ForegroundColor Yellow
        python -m venv venv
        if ($LASTEXITCODE -ne 0) { throw "Python venv creation failed" }
        
        Write-Host "Installing Python dependencies..." -ForegroundColor Yellow
        & venv\Scripts\pip.exe install -r requirements.txt --quiet
        if ($LASTEXITCODE -ne 0) { throw "Python dependency installation failed" }
        Write-Host "✓ Python environment ready" -ForegroundColor Green
    }

    # 4. Build .NET if needed
    if ($needsBuild) {
        Write-Host "`nBuilding .NET solution..." -ForegroundColor Yellow
        dotnet build src\src_dotnet\JAStudio.slnx -c Debug --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw ".NET build failed" }
        Write-Host "✓ .NET build successful" -ForegroundColor Green
    }

    Write-Host "`n=== Agent Environment Ready ===" -ForegroundColor Green
    Write-Host "You can now run builds and tests normally." -ForegroundColor Cyan

} catch {
    Write-Host "`n❌ Setup failed: $_" -ForegroundColor Red
    Write-Host "This may require manual intervention by the human developer." -ForegroundColor Yellow
    exit 1
} finally {
    Pop-Location
}
