#!/usr/bin/env pwsh
# Starts JAStudio's MCP gateway for Claude Code, which runs this script as the stdio MCP server `jastudio-headless` in .mcp.json.
#
# It builds the gateway into a folder of its own for this start, and runs it from there. Running it from its build output instead would lock
# that output for the whole Claude Code session, and every build of JAStudio that recompiled the gateway, in Rider or for the Definition of
# Done, would fail on it. Building it at each start also means a changed gateway is taken in when Claude Code reconnects to it.
#
# Standard output is the stdio connection to Claude Code: nothing but the gateway may write to it, so the build's output goes to standard
# error. The design is in dev_docs/jastudio-mcp.md.

$ErrorActionPreference = 'Stop'

$gatewayProject = Join-Path $PSScriptRoot 'JAStudio.Mcp.Gateway.csproj'
$sourceCheckout = (Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..')).Path
$gatewayBuildsFolder = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'JAStudio' 'mcp-gateway'
$buildFolderPrefix = 'build-for-process-'

function Remove-TheBuildsOfGatewaysThatExited {
   $runningProcessIds = (Get-Process).Id
   if (-not (Test-Path $gatewayBuildsFolder)) { return }

   Get-ChildItem $gatewayBuildsFolder -Directory -Filter "$buildFolderPrefix*" |
      Where-Object { [int]$_.Name.Substring($buildFolderPrefix.Length) -notin $runningProcessIds } |
      Remove-Item -Recurse -Force
}

Remove-TheBuildsOfGatewaysThatExited

$buildFolder = Join-Path $gatewayBuildsFolder "$buildFolderPrefix$PID"
dotnet build $gatewayProject -c Debug -o $buildFolder -v quiet -nologo --disable-build-servers -nodeReuse:false 2>&1 |
   ForEach-Object { [Console]::Error.WriteLine($_) }
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# This start's build folder is left for the next start to delete: the files of a process that has just exited stay locked for a moment.
& dotnet (Join-Path $buildFolder 'JAStudio.Mcp.Gateway.dll') --source-checkout $sourceCheckout
exit $LASTEXITCODE
