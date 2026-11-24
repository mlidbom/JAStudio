#!/usr/bin/env pwsh
<#
.SYNOPSIS
Tests prepare-a-new-worktree.sessionstart-hook.sh, and with -AndTheCorpus dev_scripts/check-out-the-corpus.ps1, on a new linked worktree of this
checkout's repository, at HEAD, in the temporary folder, with the working copies of both. Removes the worktree afterwards.

.DESCRIPTION
The worktree's venv link is removed before the worktree, by itself: removing a folder that holds it must never reach the main checkout's venv.
-AndTheCorpus adds about two minutes: the corpus's checkout, and its removal with the worktree.
#>
param([switch]$AndTheCorpus)
$ErrorActionPreference = 'Stop'
$checkout = git -C $PSScriptRoot rev-parse --show-toplevel
$mainCheckout = Split-Path (git -C $checkout rev-parse --path-format=absolute --git-common-dir)
$hook = Join-Path $PSScriptRoot 'prepare-a-new-worktree.sessionstart-hook.sh'
$corpusScript = Join-Path $checkout 'dev_scripts/check-out-the-corpus.ps1'
$worktree = Join-Path ([IO.Path]::GetTempPath()) "jastudio-new-worktree-$(Get-Random)"
$failed = [Collections.Generic.List[string]]::new()

# Git's bash, as Claude Code runs hooks with on Windows: there, bash on the PATH may be WSL's.
$bash = if($IsWindows) { Join-Path (Split-Path (Split-Path (Get-Command git).Source)) 'bin/bash.exe' } else { 'bash' }

function Confirm-That([bool]$holds, [string]$what) {
   if($holds) { "  ok: $what" }
   else { "  FAILED: $what"; $failed.Add($what) }
}

function Invoke-TheHookIn([string]$folder) {
   $env:CLAUDE_PROJECT_DIR = $folder
   try { (& $bash $hook) -join "`n" }
   finally { Remove-Item Env:CLAUDE_PROJECT_DIR }
}

function Test-IsTheSameFolder([string]$one, [string]$other) {
   [IO.Path]::GetFullPath($one).TrimEnd('\', '/') -eq [IO.Path]::GetFullPath($other).TrimEnd('\', '/')
}

git -C $checkout worktree add --quiet --detach $worktree HEAD
try {
   "A session starting in a new worktree:"
   $said = Invoke-TheHookIn $worktree
   $venv = Get-Item -LiteralPath (Join-Path $worktree 'venv') -ErrorAction SilentlyContinue
   Confirm-That ($null -ne $venv -and $venv.LinkType -in 'Junction', 'SymbolicLink') 'venv is a link'
   Confirm-That ($null -ne $venv -and (Test-IsTheSameFolder $venv.LinkTarget (Join-Path $mainCheckout 'venv'))) "it links to the main checkout's venv"
   Confirm-That (Test-Path (Join-Path $worktree 'venv/pyvenv.cfg')) 'a venv is reached through it'
   Confirm-That (Test-Path (Join-Path $worktree 'submodules/pythonnet-stub-generator/.git')) 'the stub-generator is checked out'
   Confirm-That ($said -match 'Linked venv' -and $said -match 'Checked out the submodule') "it says what it did: $said"
   Confirm-That ($said -match 'check-out-the-corpus\.ps1') 'it says how to check out the corpus'

   "Another session starting there:"
   Confirm-That ([string]::IsNullOrEmpty((Invoke-TheHookIn $worktree))) 'it says nothing'

   "A session starting in the main checkout:"
   Confirm-That ([string]::IsNullOrEmpty((Invoke-TheHookIn $mainCheckout))) 'it says nothing'

   if($AndTheCorpus) {
      $corpus = Join-Path $worktree 'src/jas_database'
      $recorded = (git -C $worktree ls-tree HEAD src/jas_database).Split()[2]
      $worktreesCorpusScript = Join-Path $worktree 'dev_scripts/check-out-the-corpus.ps1'
      Copy-Item $corpusScript $worktreesCorpusScript -Force

      "Checking out the corpus in the new worktree:"
      $said = (& pwsh -NoProfile -File $worktreesCorpusScript) -join "`n"
      $said
      Confirm-That ((git -C $corpus rev-parse HEAD) -eq $recorded) 'it is at the commit the branch records'
      Confirm-That ((git -C $corpus remote get-url origin) -eq (git -C $worktree config --file .gitmodules submodule.src/jas_database.url)) "its origin is GitHub's corpus repository"
      Confirm-That (-not (Test-Path (Join-Path $corpus 'media/commercial-01/.git'))) 'the media are not checked out'

      "Checking it out again:"
      Confirm-That (((& pwsh -NoProfile -File $worktreesCorpusScript) -join "`n") -match 'already checked out, at the commit the branch records') 'it says it is already checked out'
   }
}
finally {
   $venv = Join-Path $worktree 'venv'
   if((Get-Item -LiteralPath $venv -ErrorAction SilentlyContinue).LinkType) { [IO.Directory]::Delete($venv) }
   git -C $checkout worktree remove --force -- $worktree
}

if($failed.Count) { throw "$($failed.Count) of the expectations failed." }
"All held."
