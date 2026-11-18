<#
.SYNOPSIS
Runs the solution's specifications, and keeps every failure's name, message and stack trace in a report, with what the rest of
the machine was doing meanwhile.

.DESCRIPTION
Runs dotnet test on JAStudio.slnx, with a TRX results file for each specification project, in a run folder of its own under
.spec-runs in the checkout. From the results files it writes report.md there: how many specifications passed and failed in each
project, and for each failure its full name, its message and its stack trace. A failure that happens once under the full,
parallel run, such as a race, can then be looked into afterwards; dotnet test's console output gives only its name.

With -Times, it runs the suite that many times in a row, building only before the first, and the report says how many of the
runs each failing specification failed in: the way to hunt a flaky one, and to confirm a fix to it under load.

Each run's report also says how much processor time the rest of the machine used meanwhile, and the processes that used the
most. Other sessions' builds and test runs, NCrunch and Rider share the machine, and a failure that comes only under load
shows here when it came.

.PARAMETER Times
How many times to run the suite in a row. 1 by default.

.PARAMETER Filter
A filter for dotnet test, such as "FullyQualifiedName~When_following_Ankis_lifecycle", to run only those specifications.

.PARAMETER NoBuild
Runs the specifications as last built, without building first.

.EXAMPLE
.\dev_scripts\run-the-specifications.ps1

.EXAMPLE
.\dev_scripts\run-the-specifications.ps1 -Times 5 -NoBuild
#>
param(
   [int] $Times = 1,
   [string] $Filter,
   [switch] $NoBuild
)

$ErrorActionPreference = 'Stop'
$Checkout = Split-Path -Parent $PSScriptRoot
$RunFolder = Join-Path $Checkout ".spec-runs\$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss')"
$TrxNamespace = @{ trx = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }

function Get-ProcessorSecondsByProcess {
   $seconds = @{}
   foreach($process in Get-Process) {
      try { $seconds[[int]$process.Id] = [pscustomobject]@{ Name = $process.ProcessName; Seconds = $process.TotalProcessorTime.TotalSeconds } } catch { }
   }
   $seconds
}

<# The processes that aren't this run's: neither this script nor a process whose command line names this checkout, as the specifications'
   test hosts and the builds do. A process of this run that exited before the end isn't seen, nor counted. #>
function Get-TheRestOfTheMachine($before, $after) {
   $ours = @{ [int]$PID = $true }
   foreach($process in Get-CimInstance Win32_Process) {
      if($process.CommandLine -and $process.CommandLine.IndexOf($Checkout, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $ours[[int]$process.ProcessId] = $true }
   }
   foreach($id in $after.Keys) {
      if($ours.ContainsKey($id)) { continue }
      $start = if($before.ContainsKey($id)) { $before[$id].Seconds } else { 0 }
      [pscustomobject]@{ Name = $after[$id].Name; Seconds = $after[$id].Seconds - $start }
   }
}

<# Every specification's result in the TRX files in $folder, each with the project it's in. #>
function Read-TheResults([string] $folder) {
   foreach($file in Get-ChildItem $folder -Filter *.trx) {
      [xml] $trx = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8
      $storageOf = @{}
      foreach($test in (Select-Xml -Xml $trx -XPath '//trx:UnitTest' -Namespace $TrxNamespace).Node) { $storageOf[$test.id] = $test.TestMethod.codeBase }
      foreach($result in (Select-Xml -Xml $trx -XPath '//trx:UnitTestResult' -Namespace $TrxNamespace).Node) {
         $errorInfo = $result.Output.ErrorInfo
         [pscustomobject]@{
            Project    = [IO.Path]::GetFileNameWithoutExtension($storageOf[$result.testId])
            Name       = $result.testName
            Outcome    = $result.outcome
            Message    = if($errorInfo) { "$($errorInfo.Message)".Trim() } else { '' }
            StackTrace = if($errorInfo) { "$($errorInfo.StackTrace)".Trim() } else { '' }
         }
      }
   }
}

function Invoke-ARun([int] $number, [bool] $build) {
   $folder = Join-Path $RunFolder "run-$number"
   $arguments = @('test', (Join-Path $Checkout 'JAStudio.slnx'), '--verbosity', 'quiet', '--logger', 'trx', '--results-directory', $folder)
   if(-not $build) { $arguments += '--no-build' }
   if($Filter) { $arguments += @('--filter', $Filter) }

   Write-Host "Run $number of $Times..." -ForegroundColor Cyan
   $before = Get-ProcessorSecondsByProcess
   $stopwatch = [Diagnostics.Stopwatch]::StartNew()
   & dotnet @arguments | Where-Object { $_ -match '^\s*(Passed!|Failed!)' -or $_ -match 'error' } | ForEach-Object { Write-Host "  $_" }
   $seconds = $stopwatch.Elapsed.TotalSeconds
   $others = @(Get-TheRestOfTheMachine $before (Get-ProcessorSecondsByProcess))

   $results = @(Read-TheResults $folder)
   if($results.Count -eq 0) { throw "Run $number left no results in ${folder}: dotnet test failed before running any specification (a build error?)." }
   [pscustomobject]@{
      Number                = $number
      Seconds               = [math]::Round($seconds, 1)
      Results               = $results
      Failures              = @($results | Where-Object Outcome -ne 'Passed' | Where-Object Outcome -ne 'NotExecuted')
      OtherProcessorSeconds = [math]::Round(($others | Measure-Object Seconds -Sum).Sum, 1)
      OthersUsingTheMost    = ($others | Sort-Object Seconds -Descending | Select-Object -First 5 | ForEach-Object { "$($_.Name) $([math]::Round($_.Seconds, 1)) s" }) -join ', '
   }
}

function Write-TheReport($runs) {
   $lines = [Collections.Generic.List[string]]::new()
   $failing = @($runs | ForEach-Object { $_.Failures } | Group-Object Name | Sort-Object Name)
   $lines.Add("# The specifications, $(Split-Path -Leaf $RunFolder)")
   $lines.Add('')
   $runsDone = if($runs.Count -eq 1) { '1 run of the suite' } else { "$($runs.Count) runs of the suite" }
   $filtered = if($Filter) { ', filtered by ' + [char]96 + $Filter + [char]96 } else { '' }
   $outcome = if($failing.Count -eq 0) { 'Every specification passed.' } elseif($failing.Count -eq 1) { '1 specification failed.' } else { "$($failing.Count) specifications failed." }
   $lines.Add("$runsDone$filtered, in $Checkout. $outcome")
   if($failing.Count -gt 0) {
      $lines.Add('')
      foreach($group in $failing) { $lines.Add("- **$($group.Name)**: failed in $($group.Count) of $($runs.Count)") }
   }

   foreach($run in $runs) {
      $lines.Add('')
      $lines.Add("## Run $($run.Number): $($run.Failures.Count) failed, in $($run.Seconds) s")
      $lines.Add('')
      $lines.Add("The rest of the machine used $($run.OtherProcessorSeconds) processor-seconds meanwhile; the most: $($run.OthersUsingTheMost).")
      $lines.Add('')
      $lines.Add('| Project | Passed | Failed |')
      $lines.Add('|---|---|---|')
      foreach($project in $run.Results | Group-Object Project | Sort-Object Name) {
         $lines.Add("| $($project.Name) | $(@($project.Group | Where-Object Outcome -eq 'Passed').Count) | $(@($project.Group | Where-Object { $_.Outcome -ne 'Passed' -and $_.Outcome -ne 'NotExecuted' }).Count) |")
      }
      foreach($failure in $run.Failures) {
         $lines.Add('')
         $lines.Add("### $($failure.Name)")
         $lines.Add('')
         $lines.Add("$($failure.Project), $($failure.Outcome)")
         $lines.Add('')
         $lines.Add('```')
         $lines.Add($failure.Message)
         $lines.Add('')
         $lines.Add($failure.StackTrace)
         $lines.Add('```')
      }
   }

   $report = Join-Path $RunFolder 'report.md'
   Set-Content -LiteralPath $report -Value $lines -Encoding utf8
   Copy-Item $report (Join-Path $Checkout '.spec-runs\latest-report.md')
   $report
}

New-Item -ItemType Directory -Force $RunFolder | Out-Null
$runs = @(for($number = 1; $number -le $Times; $number++) { Invoke-ARun $number (-not $NoBuild -and $number -eq 1) })
$report = Write-TheReport $runs
$failed = @($runs | ForEach-Object { $_.Failures })
$failedOnes = @($failed | Group-Object Name).Count
Write-Host ''
if($failed.Count -eq 0) { Write-Host "Every specification passed in $Times run$(if($Times -ne 1) { 's' })." -ForegroundColor Green }
elseif($failedOnes -eq 1) { Write-Host '1 specification failed: see the report.' -ForegroundColor Red }
else { Write-Host "$failedOnes specifications failed: see the report." -ForegroundColor Red }
Write-Host "Report: $report"
exit $(if($failed.Count -eq 0) { 0 } else { 1 })
