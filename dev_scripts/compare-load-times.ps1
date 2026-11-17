<#
.SYNOPSIS
Compares how long the headless JAStudio of several source checkouts takes to load the same corpus checkout, such as a branch
against main.

.DESCRIPTION
Each round starts each checkout's headless JAStudio in turn, from its build output, waits until its MCP server listens, and
stops it. For each run it reports the load's figures from the log (the whole population of JAStudio's data, the notes, the
dictionary), the time until the MCP server listens, the processor time the headless JAStudio used, and the processor time the
rest of the machine used meanwhile, with the processes that used the most. A run during which other processes used more than a
little processor time was disturbed: repeat it.

A warm run loads the notes from the corpus checkout's snapshot caches (snapshot.bin). A cold run deletes them first, so it
reads every note's file, and writes the caches again. The checkouts' runs share the caches, so the checkouts compared must
write the same caches: compare the files after each one's cold run if their stored data may differ.

Only one JAStudio runs on a corpus checkout at a time: stop the headless JAStudio first (stop_headless_jastudio).

.PARAMETER SourceCheckouts
The source checkouts to compare, by a name for each, such as @{ main = 'C:\Dev\JAStudio.worktrees\measuring-main';
branch = 'C:\Dev\JAStudio.wt2' }. Each is built first, as the MCP gateway builds the headless JAStudio. A worktree of main is
made with: git worktree add --detach <folder> main

.PARAMETER CorpusFolder
The corpus checkout every run loads. By default that of the source checkout this script is in, src\jas_database.

.PARAMETER AddonFolder
The add-on folder, with the user files, every run is started on. By default that of the source checkout this script is in, src.

.PARAMETER AnkiCollectionFile
The collection the notes' studying statuses are read from. By default that of the Anki profile whose collection was written last.

.EXAMPLE
dev_scripts\compare-load-times.ps1 -SourceCheckouts ([ordered]@{ main = 'C:\Dev\JAStudio.worktrees\measuring-main'; branch = '.' }) -ColdRounds 1 -WarmRounds 5
#>
param(
   [Parameter(Mandatory)] [System.Collections.IDictionary] $SourceCheckouts,
   [string] $CorpusFolder,
   [string] $AddonFolder,
   [string] $AnkiCollectionFile,
   [int] $ColdRounds = 1,
   [int] $WarmRounds = 5,
   [string] $LogFolder = (Join-Path ([IO.Path]::GetTempPath()) "jastudio-load-times-$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss')"),
   [int] $TimeoutSeconds = 240
)
$ErrorActionPreference = 'Stop'

$checkouts = [ordered]@{}
foreach($name in $SourceCheckouts.Keys) { $checkouts[$name] = (Resolve-Path $SourceCheckouts[$name]).Path }
$thisCheckout = Split-Path $PSScriptRoot
if(-not $CorpusFolder) { $CorpusFolder = Join-Path $thisCheckout 'src\jas_database' }
if(-not $AddonFolder) { $AddonFolder = Join-Path $thisCheckout 'src' }
if(-not (Test-Path (Join-Path $CorpusFolder 'vocab'))) { throw "$CorpusFolder holds no notes: is it an initialized corpus checkout?" }
if(-not $AnkiCollectionFile) { $AnkiCollectionFile = (Get-ChildItem (Join-Path $env:APPDATA 'Anki2\*\collection.anki2') | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName }
New-Item -ItemType Directory -Force $LogFolder | Out-Null

function Get-HeadlessHostDll([string] $checkout) {
   $projectFolder = Join-Path $checkout 'src\src_dotnet\JAStudio.HeadlessHost'
   $dll = Get-ChildItem (Join-Path $projectFolder 'bin\Debug') -Recurse -Filter 'JAStudio.HeadlessHost.dll' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
   if(-not $dll) { throw "No build of the headless JAStudio in $projectFolder" }
   $dll.FullName
}

function Get-TheHeadlessHosts { Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" | Where-Object { $_.CommandLine -like '*JAStudio.HeadlessHost.dll*' } }

function Stop-TheHeadlessHostsStartedHere([int[]] $processIds) {
   foreach($id in $processIds) { Stop-Process -Id $id -Force -ErrorAction SilentlyContinue }
   Start-Sleep -Seconds 2
}

function Get-ProcessorSecondsByProcess {
   $seconds = @{}
   foreach($process in Get-Process) {
      try { $seconds[[int]$process.Id] = [pscustomobject]@{ Name = $process.ProcessName; Seconds = $process.TotalProcessorTime.TotalSeconds } } catch { }
   }
   $seconds
}

function Read-SharedFile([string] $path) {
   if(-not (Test-Path $path)) { return '' }
   $stream = [IO.File]::Open($path, 'Open', 'Read', 'ReadWrite')
   try { (New-Object IO.StreamReader($stream)).ReadToEnd() } finally { $stream.Dispose() }
}

function Get-SecondsOfTheSummaryLine([string] $log, [string] $step) {
   $match = [regex]::Match($log, "(?m)^\s*(?:=== )?$step(?: ===)?\s+\[(?<value>[\d,.]+)(?<unit>ms|s)\]")
   if(-not $match.Success) { return $null }
   $value = [double]$match.Groups['value'].Value.Replace(',', '.')
   if($match.Groups['unit'].Value -eq 'ms') { $value / 1000 } else { $value }
}

function Remove-TheSnapshotCaches { Get-ChildItem (Join-Path $CorpusFolder '*\snapshot.bin') -ErrorAction SilentlyContinue | Remove-Item }

function Measure-ALoad([string] $name, [string] $dll, [string] $kind, [int] $round) {
   if($kind -eq 'cold') { Remove-TheSnapshotCaches }
   $logFile = Join-Path $LogFolder "$name-$kind-$round.log"
   $arguments = @("`"$dll`"", '--addon-folder', "`"$AddonFolder`"", '--corpus-folder', "`"$CorpusFolder`"", '--anki-collection-file', "`"$AnkiCollectionFile`"", '--mcp-server-port', '0', '--exit-with-process', $PID)

   $before = Get-ProcessorSecondsByProcess
   $stopwatch = [Diagnostics.Stopwatch]::StartNew()
   $process = Start-Process dotnet -ArgumentList $arguments -WorkingDirectory (Split-Path $dll) -RedirectStandardOutput $logFile -RedirectStandardError "$logFile.errors" -WindowStyle Hidden -PassThru
   try {
      while(-not (Read-SharedFile $logFile).Contains("JAStudio's MCP server listens at")) {
         if($process.HasExited) { throw "$name's headless JAStudio exited before its MCP server listened: see $logFile" }
         if($stopwatch.Elapsed.TotalSeconds -gt $TimeoutSeconds) { throw "$name's headless JAStudio had no MCP server after $TimeoutSeconds seconds: see $logFile" }
         Start-Sleep -Milliseconds 100
      }
      $untilTheMcpServer = $stopwatch.Elapsed.TotalSeconds
      $after = Get-ProcessorSecondsByProcess
   }
   finally { Stop-TheHeadlessHostsStartedHere @($process.Id) }

   $others = foreach($id in $after.Keys) {
      if($id -eq $process.Id -or $id -eq $PID) { continue }
      $start = if($before.ContainsKey($id)) { $before[$id].Seconds } else { 0 }
      [pscustomobject]@{ Name = $after[$id].Name; Seconds = $after[$id].Seconds - $start }
   }
   $log = Read-SharedFile $logFile
   [pscustomobject]@{
      Round                  = $round
      Checkout               = $name
      Kind                   = $kind
      PopulatingSeconds      = Get-SecondsOfTheSummaryLine $log 'Populating JAStudio data from file system'
      NotesSeconds           = Get-SecondsOfTheSummaryLine $log '(?:Loading the notes|Loading notes from repository)'
      DictionarySeconds      = Get-SecondsOfTheSummaryLine $log 'Loading the dictionary'
      UntilTheMcpServer      = [math]::Round($untilTheMcpServer, 2)
      OwnProcessorSeconds    = [math]::Round($after[$process.Id].Seconds, 2)
      OtherProcessorSeconds  = [math]::Round(($others | Measure-Object Seconds -Sum).Sum, 2)
      OthersUsingTheMost     = ($others | Sort-Object Seconds -Descending | Select-Object -First 3 | ForEach-Object { "$($_.Name) $([math]::Round($_.Seconds, 1))s" }) -join ', '
      ReadTheNotesFiles      = $log.Contains('falling back to full rebuild') -or $kind -eq 'cold'
   }
}

if(Get-TheHeadlessHosts | Where-Object { $_.CommandLine -like "*$CorpusFolder*" }) { throw "A headless JAStudio runs on $CorpusFolder already: stop it first." }

$dlls = [ordered]@{}
foreach($name in $checkouts.Keys) {
   Write-Host "Building $name's headless JAStudio ($($checkouts[$name]))"
   dotnet build (Join-Path $checkouts[$name] 'src\src_dotnet\JAStudio.HeadlessHost\JAStudio.HeadlessHost.csproj') -c Debug -v quiet -nologo --disable-build-servers -nodeReuse:false | Out-Null
   if($LASTEXITCODE -ne 0) { throw "Building $name's headless JAStudio failed" }
   $dlls[$name] = Get-HeadlessHostDll $checkouts[$name]
}

function Measure-Rounds([string] $kind, [int] $rounds) {
   if($rounds -le 0) { return }
   foreach($round in 1..$rounds) { foreach($name in $dlls.Keys) { Measure-ALoad $name $dlls[$name] $kind $round } }
}
$runs = @(Measure-Rounds 'cold' $ColdRounds; Measure-Rounds 'warm' $WarmRounds)

$runs | Format-Table Round, Checkout, Kind, PopulatingSeconds, NotesSeconds, DictionarySeconds, UntilTheMcpServer, OwnProcessorSeconds, OtherProcessorSeconds, OthersUsingTheMost -AutoSize | Out-String -Width 250 | Write-Host
foreach($kind in 'cold', 'warm') {
   foreach($name in $dlls.Keys) {
      $populating = @($runs | Where-Object { $_.Checkout -eq $name -and $_.Kind -eq $kind } | ForEach-Object PopulatingSeconds | Sort-Object)
      if($populating.Count -eq 0) { continue }
      Write-Host "$kind, $name`: populating median $($populating[[math]::Floor($populating.Count / 2)]) s, from $($populating[0]) to $($populating[-1]) s over $($populating.Count) runs"
   }
}
Write-Host "The runs' logs: $LogFolder"
