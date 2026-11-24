#!/usr/bin/env pwsh
<#
.SYNOPSIS
Checks out the corpus, the submodule src/jas_database, in a linked worktree of JAStudio's source repository, at the commit its branch records.

.DESCRIPTION
For work that reads the corpus in a worktree: the parsing cases' full mode (In_the_full_mode), and the headless JAStudio, which works on the
worktree's own corpus checkout. Nothing in the Definition of Done reads it, so a new worktree doesn't get it when a session starts there.

The corpus repository is cloned from the main checkout's clone of it, on local disk, rather than from GitHub: git hardlinks the history, so
nothing is downloaded, the history takes no new space, and the checkout of its 110,000 files takes about a minute. The clone's origin is then
the corpus repository on GitHub, as git's own checkout of the submodule would have it. A commit the main checkout's clone doesn't have is
fetched from there.

Its media submodules, media/commercial-01 and -02, aren't checked out: nothing run in a worktree reads them.

Does nothing in the main checkout, whose corpus checkout JAStudio inside Anki works on, nor to a corpus already checked out, which it leaves
at the commit it's at: a session's analysis may be in it.
#>
$ErrorActionPreference = 'Stop'
$corpus = 'src/jas_database'
$checkout = git -C $PSScriptRoot rev-parse --show-toplevel
$gitFolder = git -C $checkout rev-parse --absolute-git-dir
$commonGitFolder = git -C $checkout rev-parse --path-format=absolute --git-common-dir
if($gitFolder -eq $commonGitFolder) {
   "This is the main checkout, whose corpus checkout JAStudio inside Anki works on: left as it is."
   return
}

$recorded = (git -C $checkout ls-tree HEAD $corpus).Split()[2]
$corpusCheckout = Join-Path $checkout $corpus
if(Test-Path (Join-Path $corpusCheckout '.git')) {
   $at = git -C $corpusCheckout rev-parse HEAD
   if($at -eq $recorded) { "The corpus is already checked out, at the commit the branch records, $recorded." }
   else { "The corpus is already checked out, at $at, and the branch records ${recorded}: left where it is." }
   return
}

$theMainCheckoutsClone = Join-Path $commonGitFolder "modules/$corpus"
$gitHub = git -C $checkout config --file .gitmodules "submodule.$corpus.url"
"Checking out the corpus at $recorded, cloned from the main checkout's clone of it, $theMainCheckoutsClone. It takes about a minute."
$took = [Diagnostics.Stopwatch]::StartNew()
git -C $checkout -c protocol.file.allow=always -c "submodule.$corpus.url=$theMainCheckoutsClone" submodule update --init --quiet -- $corpus
$checkedOut = $LASTEXITCODE -eq 0
if(Test-Path (Join-Path $corpusCheckout '.git')) { git -C $corpusCheckout remote set-url origin $gitHub }
if(-not $checkedOut) {
   "The main checkout's clone hasn't the commit the branch records: fetching it from GitHub, $gitHub."
   git -C $checkout submodule update --init --quiet -- $corpus
   if($LASTEXITCODE -ne 0) { throw "Checking out the corpus at $recorded failed: see git's output above." }
}
"Checked out the corpus at $recorded, in {0:N0} s." -f $took.Elapsed.TotalSeconds
