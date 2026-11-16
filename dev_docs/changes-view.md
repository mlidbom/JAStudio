# The changes view — Design Direction

> **Status: wanted and agreed in outline with Magnus (2026-11-09, 2026-11-10); not designed in detail, not started.**
> It comes after the matching configuration's redesign (see [Where it stands](#where-it-stands-in-the-order-of-work)).
> Collected here on 2026-11-15 from `jastudio-mcp.md`, `jastudio-workspace.md`, `media-storage-and-import.md` and the
> conversations it was discussed in. Magnus has called it the changes/commit view, workflow and functionality.

## Why

Magnus, 2026-11-09: "I'd absolutely love to have an intelligent way to do commits inside jastudio that show me what
notes changed and how. Or have the ability to check the history of a note and/or roll it back to a previous state,
including reparsing with those changes and seeing what effect that has. Seeing what sentences changed and how when I
change something about one or more vocabs would also be invaluable."

And on 2026-11-10, of the commit step: it "would guarantee that a full rebuild has run after the latest vocab change,
and would give me a quick overview over changes, and would handle media sub-repositories, and probably help with
creating commit messages naming the actually edited notes. This will turn committing/saving changes I make from a
cumbersome manual process involving jastudio and multiple git repositories managed in git-extensions into ... probably
just two or three clicks in the ui". It "will be a huge boon to my constant workflow."

- **Today a commit is a manual round:** JAStudio, then each repository in Git Extensions, the media submodules among
  them.
- **A forgotten full rebuild goes unnoticed.** An edit to a vocab changes how sentences parse, and the commit has to
  hold those changes too, as the process for changes to the corpus requires (the skill
  `.claude/skills/changing-what-the-corpus-stores`). Magnus forgetting the rebuild when working in the UI "should be
  pretty much eliminated" by the commit step.
- **JAStudio has no undo.** Every change is written to the note at once, with no Save button (`jastudio-workspace.md`,
  "Live editing"). The corpus being JSON files in git makes that safe, but restoring a note with git by hand is far more
  work than a confirmation (`tags.md`). A note's history, and rolling it back, cover mistakes.

## What it does

### Committing: the changes since the last commit, as notes

- **The changes are shown as notes, not as files:** each changed note with what changed in it, as
  `diff_the_changed_notes` shows them (below). For a sentence, the words it now parses to, against the words before.
- **A full rebuild runs before committing,** so a commit always holds everything its edits changed.
- **The notes edited by hand are told apart from the notes a rebuild only regenerated,** and the commit message names
  the edited ones, whatever their type. JAStudio knows which notes an editor or a command wrote and which a rebuild
  did. The diff alone can't tell them apart: a rebuild changes generated data, such as a vocab's `sentenceCount`, on
  notes nobody edited.
- **The media submodules are committed too,** innermost first: `media/commercial-01` and `media/commercial-02`, then the
  corpus repository with their new pointers.
- **Small coherent commits** (Magnus, 2026-11-10: "Absolutely"): each edit that stands on its own gets its own commit,
  with the parsing changes it caused.

### A note's history, and rolling it back

- **A note's history is its versions as notes,** each compared with the one before. Its file is named by its id, so the
  history is `git log` of that one file.
- **Rolling back writes the old version through the collection,** not by checking the file out with git. It goes
  through the normal update path, so the cache, the files, the snapshot and the notifications stay consistent, it
  reaches Anki, and the sentences it affects are reparsed. What the rollback did then shows in the changes, as any edit
  does.
- **Reverting a note to its last committed version** is the simplest case: its file as of `HEAD`. It was the
  workspace's increment 7, parked on 2026-11-06 because it freed nothing from Anki (`jastudio-workspace.md`).
- **Later: seeing what a rollback would do before keeping it.** That takes more than git: a trial run on a copy of the
  corpus, as the headless JAStudio does on its own worktree today (Claude, 2026-11-09; a design for later).

### What a change to vocab did to the sentences

The same comparison against the last commit, limited to sentences and shown as changes in their parsing: words added
and removed, now displayed and no longer displayed, each with its vocab. Over every sentence, the same words counted
show how a change played out across the corpus at a glance.

## What's built to build on

In JAStudio's MCP server (`jastudio-mcp.md`, "The tools"), for Claude's analyses of rebuilds:

- **`diff_the_changed_notes`** compares the notes whose files differ between two versions of the corpus: by default the
  checkout's `HEAD` and its files as they are now, or any two commits. Both versions of each file are read into notes
  and compared property by property; a sentence's parsed words are compared as words with their vocab. It can't compare
  across a change to how the corpus stores notes. The code is `NotesDiff`, in `JAStudio.Mcp/Tools/Corpus/NoteDiffs`.
- **`list_changed_notes`** lists the notes whose files differ from `HEAD`, from `git status`.
- **`reload_the_collection`** loads the collection again from the corpus checkout, and so does the workspace's Local
  Actions > "Reload the collection from disk" (Magnus, 2026-11-09: resetting with git and reloading is valuable in
  itself).
- **`fast_forward_the_corpus_checkout`** moves the checkout forward to a commit and reloads.
- **The git layer:** `CorpusCheckout`, the corpus checkout as git sees it, with `GitCommandLine` behind it, in
  `JAStudio.Mcp/Corpus`.

All of it is in `JAStudio.Mcp`, which the UI doesn't reference. The plan on 2026-11-09 was a class for the corpus
repository, with the git command line behind it, in a project that both the live JAStudio and the MCP server use; the
note diff would have to move with it.

## Git: the command line, behind a layer of its own (2026-11-09)

JAStudio runs git as a child process, and keeps git out of the rest of the code. It's fast enough to use interactively.
Measured on the corpus repository, with 207 commits and 79,718 note files:

| What | Time |
|---|---|
| `git status` of the whole checkout | 0.17 s |
| One note's history, `git log -- <its file>` | 0.035 s |
| A note's file as it was at a commit | 0.03 s |
| The files changed across the last 20 commits | 0.18 s |

Bulk reads, such as the old versions of thousands of sentences, go through one long-running `git cat-file --batch`,
which `diff_the_changed_notes` already does.

**Why not LibGit2Sharp,** which runs libgit2 in-process with a typed API:

- **The command line is the same git Magnus, Rider and Claude use on the repository.** It honours the config, the
  worktrees, the media submodules and git's own speed-ups. libgit2 is a separate implementation that lags behind git,
  and a second implementation writing to the same repository is a risk for little gain.
- **LibGit2Sharp's real advantage is not starting a process per call,** and the batch process covers the one place
  where that matters.
- **Parsing git's output stays inside the layer,** and the formats meant for programs (`--porcelain`, `-z`, `--format`)
  are stable.
- **The cost is that JAStudio needs git installed,** which is normal on all three platforms.

## Where it stands in the order of work

- **2026-11-10, morning:** Magnus put it between the domain model's cleanup and the matching configuration's redesign:
  "Building that may just slide in before the changes to vocab matching config".
- **2026-11-10, evening,** asked whether the matching configuration's redesign now comes first: "Well, this design comes
  before and I'm all exited about it so I'm guessing yes." Its increments 1 to 5 were built from 2026-11-10 to
  2026-11-12 (`matching-configuration.md`).
- **2026-11-15:** Magnus left a standalone JAStudio's sync able to reach his Anki from any checkout: "It is rarely a
  problem and if it is we have git and the upcoming changes/commit view."

## What it makes easier later

- **All media in submodules, and the corpus repository's history rewritten** to remove media that isn't ours
  (`media-storage-and-import.md`, "Not done"). Magnus, 2026-11-10: "Once we have commit support in jastudio,
  making working with separate media submodules easier, we might consider" it. Far off.

## Open questions

1. **Reverting a note created since the last commit,** which has no committed version (open since 2026-11-06).
2. **Small coherent commits from the UI** (Claude, 2026-11-15). Magnus edits several notes between commits, and once
   they're rebuilt together, which parsing changes came from which edit can't be told apart. Does the view commit all
   of them as one, or have a commit made after each edit?
3. **The source repository's pointer** (Claude, 2026-11-15). A corpus commit of Magnus's is recorded in the source
   repository by a `db updates` commit, which the main checkout's Claude session makes today
   (`.claude/rules/02-universal-local/045-committing-with-the-corpus-repository.md`). Does the view make it, and does it
   push?
4. **A rebuild in a JAStudio that runs older code than the source checkout** (Claude, 2026-11-15) would commit the old
   code's parsing. Each running JAStudio reporting the commit its build was made from, agreed and not built
   (`jastudio-mcp.md`, "Order of work", 8), would let the view say so before committing.
