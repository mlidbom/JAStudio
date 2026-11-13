# Committing a change that reaches the corpus repository

**When work on the source changes the corpus repository (`src/jas_database`), commit both, and record the corpus
repository's new commit in the source commit.** That is much of the point of the corpus being a submodule: a source
commit pins the corpus commit its code goes with, so the relationship between a code change and the data change it
caused is trivial to track, in either direction.

Such a change is anything the work did to the corpus repository's files: deleting a file the code no longer reads
(`media/metadata/media-import-rules.json`), migrating the notes to a new format, changing `import-profiles.json`'s
shape, moving media.

## Who changes the corpus repository: the session in the main checkout (Magnus, 2026-11-12)

Every source checkout has its own clone of the corpus repository: git keeps one for each linked worktree's submodule.
Only the main checkout's, `C:\Dev\JAStudio\src\jas_database`, is Magnus's: the one JAStudio inside Anki works on, on
`main`. Corpus commits made in two clones, or on two branches, diverge, and merging rebuilt data is a mess: thousands of
generated note files changed two ways. So the corpus gets one line of history, made in Magnus's repository.

- **The session in the main checkout** commits to the corpus, fast-forwards Magnus's checkout to its commits, records
  the pointer on `main`, and pushes the corpus repository's `main`, as standard procedure: the work being agreed is
  enough. Magnus: "I think we are probably to conservative in agents pushing the corpus. This is git. it has the ability
  to undo stuff if we need to...." The push takes along any commits of Magnus's under the work's. Pushing the source
  repository still needs his word.
- **A session in a linked worktree,** on a branch, uses the worktree's own `src/jas_database`, which the headless
  JAStudio works on there by default, only to check and analyze: the rebuilds, the edits, the diffs. When it's done, it
  puts that checkout back at the commit its branch records, and it leaves the `src/jas_database` pointer alone.
  - **What it hands over instead:** what its rebuild changed, analyzed as below, and the edits to the notes the work
    needs, listed in its report and in its commit message. A large migration is code.
  - **Committing to the corpus anyway needs Magnus's permission,** asked for each time. With it, the session works as
    the main checkout's does, with two differences. It starts from the commit Magnus's checkout is at, which it fetches
    into its worktree's clone first. And its commits are in its worktree's clone, so the fast-forward fetches them:
    `fast_forward_the_corpus_checkout` with `fetchingItFrom` set to the worktree's `src/jas_database`.
- **After a merge to `main` of work that changes what the corpus stores,** the main checkout's session makes the corpus
  change on the merged code. It runs the full rebuild, makes the edits the branch listed, checks what changed against
  what the branch reported, and commits it, named after the merge, with the pointer. Two branches' changes to parsing
  first meet in that rebuild, so look hardest there for what neither branch saw.

## The most important and most common case: a change to how matching or analysis works

That is, any change to what parsing a sentence, or a note's generated data, comes out as:
- tokenizing and word extraction (`LanguageServices/Parsing`);
- matching vocab to the words of sentences (`VocabNoteMatchingRules`, `VocabNoteMatchingConfiguration`, forms,
  compound parts);
- what a note's `UpdateGeneratedData` computes.

The corpus stores what these produce, such as every sentence's parsing result. So when one of them changes:

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
3. **The changes the rebuild makes to the corpus MUST be committed with the source change that caused them,** as
   below: in the main checkout, together; for work on a branch, by the main checkout's session right after the merge,
   as above. A source change to matching or analysis never reaches `main` without them.

## The process for every change to the corpus's notes (Magnus, 2026-11-10)

For a change to code that changes what the corpus stores, such as matching, and for a change to the notes themselves,
such as a fix to some notes' tags. Make it in the headless JAStudio, at the commit Magnus's corpus checkout is at: in the
main checkout, on its own worktree of the corpus repository, `C:\Dev\JAStudio.worktrees\headless-corpus`. If his checkout
has uncommitted changes, ask him to commit them first, so that the work goes on top of them. The headless JAStudio
always runs the latest source: `start_headless_jastudio` builds the source checkout as it is. A session in a linked
worktree goes through the same cycles in its worktree's `src/jas_database`, to check and analyze, and commits nothing
there without permission, as above.

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
   the pointer, as below.
4. **Changes to the notes come between code changes, never in the middle of one:** a code change half made would leak
   into the notes' rebuild.
5. **Magnus's checkout takes the commits in** by a fast-forward through the live JAStudio
   (`fast_forward_the_corpus_checkout`), standard for the main checkout's session, which then pushes the corpus's
   `main`. When his checkout has moved on meanwhile, redo the change on top of it rather than rebasing rebuilt data.
   When the commits come with code the live JAStudio doesn't run yet, it needs a restart on the new build: end the
   response with the restart notice (`046-telling-magnus-to-restart-jastudio.md`). When the corpus's format changed,
   the old code can't read the new notes, so the fast-forward is made while Anki is closed.

## How, innermost repository first

1. **A media submodule** (`src/jas_database/media/commercial-01` or `-02`), if the work changed it: commit there.
2. **The corpus repository:** commit the work's changes there, together with the media submodule's new pointer if
   step 1 made one.
3. **The source repository:** commit the work, *including the `src/jas_database` pointer*, so the source commit names
   the corpus commit it goes with.

Use the same commit message in each repository (Magnus, 2026-11-08), so the commits are easy to match up.

## Only the work's own changes

Magnus's use of JAStudio changes the corpus repository all the time: every note edited is a changed file there. He
commits those himself, and the source repository's pointer to them goes in a "db updates" commit (below).

- **In the corpus repository, commit by path only the files the work changed.** Check `git status` there first. If a
  file the work changed also holds changes of Magnus's, ask before committing it.
- **Before recording the pointer, look at what it covers:** `git log <recorded commit>..HEAD` in the corpus repository,
  where `git ls-tree HEAD src/jas_database` in the source repository gives the recorded commit. If it covers commits
  of Magnus's, record his pushed ones first, in a "db updates" commit. If it still covers some of his, unpushed ones,
  say so when reporting the commit: they'll be recorded with it.
- **Other than that, leave `src/jas_database` out of source commits.**

## Magnus's own corpus commits: a "db updates" commit

Whenever the source repository's `git status` shows `src/jas_database` changed because of corpus commits of Magnus's
on `main` that are pushed, commit the pointer in a source commit of its own, with the message `db updates` and nothing
else (Magnus, 2026-11-09). That commit needs no go-ahead.

- **Only in the main checkout, when the corpus checkout is on `main`, and `main` is pushed:**
  `git -C src/jas_database status -sb` starts with `## main...origin/main` and shows no `[ahead`. Unpushed commits may
  still change, so leave those alone.
- **The pointer records the corpus repository's `HEAD` commit, not its working tree:** Magnus's uncommitted edits there
  don't matter, and stay his.
- **Commit only the pointer:** `git commit -m "db updates" -- src/jas_database`, so nothing else staged goes with it.
