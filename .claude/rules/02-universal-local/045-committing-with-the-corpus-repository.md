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
commits those himself, and the source repository's pointer to them is his "db updates" commit.

- **In the corpus repository, commit by path only the files the work changed.** Check `git status` there first. If a
  file the work changed also holds changes of Magnus's, ask before committing it.
- **Before recording the pointer, look at what it covers:** `git log <recorded commit>..HEAD` in the corpus repository,
  where `git ls-tree HEAD src/jas_database` in the source repository gives the recorded commit. If it covers commits
  of Magnus's besides the work's, say so when reporting the commit: they'll be recorded with it.
- **Without such work, leave `src/jas_database` out of source commits.** A changed pointer that the work didn't cause
  is Magnus's to commit.

Committing at all still needs Magnus's word, as always.
