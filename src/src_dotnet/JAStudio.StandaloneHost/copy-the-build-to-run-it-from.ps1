<#
.SYNOPSIS
Copies the standalone JAStudio's newest build to the folder it runs from, bin/copy-it-runs-from, replacing what was there.

.DESCRIPTION
Run from its build output, the standalone JAStudio would lock the files it has loaded for as long as it runs, and every
build of the solution that recompiled them, in Rider or for the Definition of Done, would fail on them. Run from a copy,
builds don't touch what runs, and each start runs the newest build. The headless JAStudio and the MCP gateway run from
copies for the same reason.

The run configuration "Standalone JAStudio" runs this after building, through the run configuration "Copy the build of
the standalone JAStudio", then starts the copy.
#>
$ErrorActionPreference = 'Stop'

$projectFolder = $PSScriptRoot
$folderItRunsFrom = [IO.Path]::GetFullPath((Join-Path $projectFolder 'bin/copy-it-runs-from'))

$newestBuild = Get-ChildItem (Join-Path $projectFolder 'bin/Debug') -Recurse -Filter 'JAStudio.StandaloneHost.exe' |
   Sort-Object LastWriteTimeUtc -Descending |
   Select-Object -First 1
if (-not $newestBuild) { throw "There is no build of the standalone JAStudio under $projectFolder\bin\Debug: build it first." }

$runningFromTheCopy = Get-Process 'JAStudio.StandaloneHost' -ErrorAction SilentlyContinue |
   Where-Object { $_.Path -and $_.Path.StartsWith($folderItRunsFrom, [StringComparison]::OrdinalIgnoreCase) }
if ($runningFromTheCopy) {
   throw "The standalone JAStudio of this checkout still runs from $folderItRunsFrom (process $($runningFromTheCopy.Id -join ', ')): close it, then start it again."
}

$copying = [Diagnostics.Stopwatch]::StartNew()
if (Test-Path $folderItRunsFrom) { Remove-Item $folderItRunsFrom -Recurse -Force }
Copy-Item $newestBuild.DirectoryName $folderItRunsFrom -Recurse
Write-Host "Copied the build in $($newestBuild.DirectoryName) to $folderItRunsFrom in $([math]::Round($copying.Elapsed.TotalSeconds, 1))s."
