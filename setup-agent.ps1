#!/usr/bin/env pwsh
# Sets up a fresh worktree for an agent that is to build and test in it: git long paths, the submodules, the Python
# venv and a .NET build. It does only what is missing, so in a checkout that is already set up it changes nothing.

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $ScriptDir

try {
    Write-Host "`n=== Background Agent Environment Setup ===" -ForegroundColor Cyan
    Write-Host "Worktree: $(Split-Path -Leaf (Get-Location))" -ForegroundColor Gray

    # Check what needs to be done
    $needsGitConfig = (git config --get core.longpaths) -ne "true"
    $needsSubmodules = -not (Test-Path "submodules\pythonnet-stub-generator\.git") -or -not (Test-Path "src\jas_database\.git")
    $needsVenv = -not (Test-Path "venv\Scripts\python.exe")
    $needsBuild = -not (Test-Path "src\src_dotnet\JAStudio.Core\bin\Debug\net11.0\JAStudio.Core.dll")

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
        dotnet build JAStudio.slnx -c Debug --verbosity quiet
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
