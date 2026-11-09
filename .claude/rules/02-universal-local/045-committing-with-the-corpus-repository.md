# Committing a change that reaches the corpus repository

**When work on the source changes the corpus repository (`src/jas_database`), commit both, and record the corpus
repository's new commit in the source commit.** That is much of the point of the corpus being a submodule: a source
commit pins the corpus commit its code goes with, so the relationship between a code change and the data change it
caused is trivial to track, in either direction.

Such a change is anything the work did to the corpus repository's files: deleting a file the code no longer reads
(`media/metadata/media-import-rules.json`), migrating the notes to a new format, changing `import-profiles.json`'s
shape, moving media.

## The most important and most common case: a change to how matching or analysis works

That is, any change to what parsing a sentence, or a note's generated data, comes out as:
- tokenizing and word extraction (`LanguageServices/JanomeEx`);
- matching vocab to the words of sentences (`VocabNoteMatchingRules`, `VocabNoteMatchingConfiguration`, forms,
  compound parts);
- what a note's `UpdateGeneratedData` computes.

The corpus stores what these produce, such as every sentence's parsing result. So when one of them changes:

1. **A full rebuild of the corpus MUST be run.** That's `LocalNoteUpdater.FullRebuild`, which reparses every sentence
   and updates every note: in the workspace's main menu, Local Actions > Update > "All the above: Full rebuild". It
   runs only on the real corpus, in JAStudio inside Anki (the desktop host runs on the test environment), so ask
   Magnus to run it once the change is built. Anki takes in a new build on its next restart. Before asking, check that
   the corpus repository has no uncommitted changes, and if it has, ask Magnus to commit them first: then everything
   the rebuild changes is the work's.
2. **Inspect and analyze what the rebuild changed, yourself, every time, to verify that the change made parsing
   better.** Go through the diff in the corpus repository:
   - how many sentences and notes changed;
   - what kinds of changes they are, such as words newly matched, no longer matched, or matched to another vocab;
   - whether each kind is what the change intended.

   Look hardest for regressions: sentences that now parse worse, or changes the work didn't aim at. Report what you
   found to Magnus, with examples, before committing. If he doesn't ask for this when a parsing change is made, remind
   him: the rebuild has to be run and its result analyzed before the change is committed.
3. **The changes the rebuild makes to the corpus MUST be committed together with the source change that caused
   them,** as below. A source change to matching or analysis is never committed without them.

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

- **Only when the corpus checkout is on `main`, and `main` is pushed:** `git -C src/jas_database status -sb` starts with
  `## main...origin/main` and shows no `[ahead`. On a detached `HEAD` or another branch, leave the pointer alone: how
  branching the corpus plays out isn't worked out yet (Magnus: "I'm not confident on the effects or logic if we start
  branching the db"). Unpushed commits may still change, so leave those alone too.
- **The pointer records the corpus repository's `HEAD` commit, not its working tree:** Magnus's uncommitted edits there
  don't matter, and stay his.
- **Commit only the pointer:** `git commit -m "db updates" -- src/jas_database`, so nothing else staged goes with it.

Committing anything else still needs Magnus's word, as always, apart from the MCP server (see
`090-claude-owns-the-jastudio-mcp-server.md`).
