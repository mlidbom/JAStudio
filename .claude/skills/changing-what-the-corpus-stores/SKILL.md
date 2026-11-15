---
name: changing-what-the-corpus-stores
description: >-
  The process for a change to what JAStudio's corpus stores: a change to how matching or analysis works (tokenizing,
  word extraction, matching vocab to the words of sentences, a note's generated data), and a change to the corpus's
  notes themselves, such as a fix to some notes' tags or to a vocab's match-condition. Full rebuilds before and after
  it in the headless JAStudio, an analysis of what changed, and the corpus committed with the source. Use it before
  changing any such code or any note in src/jas_database, when a sentence that parses wrong or a word that matches the
  wrong vocab is to be fixed, and after merging a branch whose work changes what the corpus stores.
---

# Changing what the corpus stores: rebuild, analyze, and commit the corpus with the source

The corpus stores what matching and analysis produce, such as every sentence's parsing result. So a change to them, and
a change to the notes themselves, is made with full rebuilds of the corpus around it, and its commit holds everything
the rebuild changed.

The always-loaded rule "Committing a change that reaches the corpus repository"
(`.claude/rules/02-universal-local/045-committing-with-the-corpus-repository.md`) says who commits to the corpus, in
which order the repositories are committed, and how Magnus's own changes are kept out of the work's commits. This is the
process around those commits.

## A change to how matching or analysis works

That is, any change to what parsing a sentence, or a note's generated data, comes out as: tokenizing and word
extraction, matching vocab to the words of sentences, and what a note's `UpdateGeneratedData` computes. When one of
them changes:

1. **A full rebuild of the corpus MUST be run.** That's `LocalNoteUpdater.FullRebuild`, which reparses every sentence
   and updates every note's generated data. Run it yourself, in the headless JAStudio, as the process below says: a
   rebuild before changing the code, and one after.
2. **Inspect and analyze what the rebuild changed, yourself, every time, to verify that the change made parsing
   better.** Go through the diff in the corpus repository:
   - how many sentences and notes changed;
   - what kinds of changes they are, such as words newly matched, no longer matched, or matched to another vocab;
   - whether each kind is what the change intended.

   Look hardest for regressions: sentences that now parse worse, or changes the work didn't aim at. Report what you
   found to Magnus, with examples. If you can't tell whether the result is acceptable, report it before committing,
   and let him judge (Magnus, 2026-11-11). If he doesn't ask for this when a parsing change is made, remind him: the
   rebuild has to be run and its result analyzed before the change is committed.
3. **The changes the rebuild makes to the corpus MUST be committed with the source change that caused them:** in the
   main checkout, together; for work on a branch, by the main checkout's session right after the merge, as below. A
   source change to matching or analysis never reaches `main` without them.

## The process for every change to the corpus's notes (Magnus, 2026-11-10)

For a change to code that changes what the corpus stores, such as matching, and for a change to the notes themselves,
such as a fix to some notes' tags. Make it in the headless JAStudio, at the commit Magnus's corpus checkout is at: in the
main checkout, on its own worktree of the corpus repository, `C:\Dev\JAStudio.worktrees\headless-corpus`. If his checkout
has uncommitted changes, ask him to commit them first, so that the work goes on top of them. The headless JAStudio
always runs the latest source: `start_headless_jastudio` builds the source checkout as it is.

1. **Before any change, a full rebuild.** If it changes anything, commit that on its own, with the message
   `full-rebuild`, and more only to say why when that's known: then no inconsistency that was already there mixes into
   the change's commit. For a code change, run it before changing the code.
   - A rebuild that changes anything means the corpus was behind: most likely a rebuild forgotten after an earlier
     edit, or a code change committed without its rebuild. Whether to tell Magnus, or only say so in the commit
     message, is a judgment call: a small change that parses well, whose cause the previous commit shows, needs only
     the message.
2. **Then a cycle for each change:** make the change, run a full rebuild, check what changed for sanity with
   `diff_the_changed_notes`, and commit the change together with everything the rebuild changed. The commit then shows
   the change's whole effect, and its message says what was done by hand.
   - **The message names the notes edited by hand,** whatever their type. The rebuild also changes generated data on
     notes nobody edited, such as a vocab's `sentenceCount`, so the diff can't tell the two apart.
   - **If the results aren't sane, don't commit:** report what looks wrong. Whether to throw the changes away at once is
     a judgment call: they're in the headless JAStudio's own checkout, so leaving them while investigating blocks
     nothing of Magnus's.
3. **Small coherent commits:** a cycle, and a commit, for each change that stands on its own. Removing a wrong episode
   tag from some sentences and removing a song's tag from the kanji are two commits, not one. Each is then easy to
   review, to match with its source commit, and to revert on its own. Each corpus commit gets its source commit with
   the pointer, as the rule says.
4. **Changes to the notes come between code changes, never in the middle of one:** a code change half made would leak
   into the notes' rebuild.
5. **Magnus's checkout takes the commits in** by a fast-forward through the live JAStudio
   (`fast_forward_the_corpus_checkout`), standard for the main checkout's session, which then pushes the corpus's
   `main`. When his checkout has moved on meanwhile, redo the change on top of it rather than rebasing rebuilt data.
   When the commits come with code the live JAStudio doesn't run yet, it needs a restart on the new build: end the
   response with the restart notice (`046-telling-magnus-to-restart-jastudio.md`). When the corpus's format changed,
   the old code can't read the new notes, so the fast-forward is made while Anki is closed.

## In a linked worktree, on a branch

The session goes through the same cycles in its worktree's `src/jas_database`, to check and analyze, and commits
nothing there without Magnus's permission.

- **What it hands over instead:** what its rebuild changed, analyzed as above, and the edits to the notes the work
  needs, listed in its report and in its commit message. A large migration is code.
- **With Magnus's permission to commit to the corpus,** the session works as the main checkout's does, with two
  differences. It starts from the commit Magnus's checkout is at, which it fetches into its worktree's clone first. And
  its commits are in its worktree's clone, so the fast-forward fetches them: `fast_forward_the_corpus_checkout` with
  `fetchingItFrom` set to the worktree's `src/jas_database`.

## After a merge to `main` of work that changes what the corpus stores

The main checkout's session makes the corpus change on the merged code. It runs the full rebuild, makes the edits the
branch listed, checks what changed against what the branch reported, and commits it, named after the merge, with the
pointer. Two branches' changes to parsing first meet in that rebuild, so look hardest there for what neither branch saw.
