# Committing a change that reaches the corpus repository

**When work on the source changes the corpus repository (`src/jas_database`), commit both, and record the corpus
repository's new commit in the source commit.** That is much of the point of the corpus being a submodule: a source
commit pins the corpus commit its code goes with, so the relationship between a code change and the data change it
caused is trivial to track, in either direction.

Such a change is anything the work did to the corpus repository's files: deleting a file the code no longer reads
(`media/metadata/media-import-rules.json`), migrating the notes to a new format, changing `import-profiles.json`'s
shape, moving media.

## Before changing matching, analysis or the corpus's notes: load the skill `changing-what-the-corpus-stores`

The corpus stores what matching and analysis produce, such as every sentence's parsing result. So a change to them
takes a full rebuild of the corpus before it and one after, an analysis of what the rebuild changed, and a corpus commit
that goes with the source commit: a source change to matching or analysis never reaches `main` without the changes its
rebuild makes to the corpus. The skill has the process. Load it before any change to:

- tokenizing and word extraction (`LanguageServices/Parsing`);
- matching vocab to the words of sentences (`Note/Vocabulary/Matching`, `Parsing/WordExtraction`, forms, compound
  parts);
- what a note's `UpdateGeneratedData` computes;
- the notes themselves, such as a fix to some notes' tags or to a vocab's match-condition.

## Who changes the corpus repository: the session in the main checkout (Magnus, 2026-11-12)

Every source checkout has its own clone of the corpus repository: git keeps one for each linked worktree's submodule.
Only the main checkout's, `C:\Dev\JAStudio\src\jas_database`, is Magnus's: the one JAStudio inside Anki works on, on
`main`. Corpus commits made in two clones, or on two branches, diverge, and merging rebuilt data is a mess: thousands of
generated note files changed two ways. So the corpus gets one line of history, made in Magnus's repository.

- **The session in the main checkout** commits to the corpus, fast-forwards Magnus's checkout to its commits, records
  the pointer on `main`, and pushes the corpus repository's `main`, as standard procedure: the work being agreed is
  enough. Magnus: "I think we are probably to conservative in agents pushing the corpus. This is git. it has the ability
  to undo stuff if we need to...." The push takes along any commits of Magnus's under the work's.
- **A session in a linked worktree,** on a branch, uses the worktree's own `src/jas_database`, which the headless
  JAStudio works on there by default, only to check and analyze: the rebuilds, the edits, the diffs. When it's done, it
  puts that checkout back at the commit its branch records, and it leaves the `src/jas_database` pointer alone.
  Committing to the corpus anyway needs Magnus's permission, asked for each time.
- **After a merge to `main` of work that changes what the corpus stores,** the main checkout's session makes the corpus
  change on the merged code, as the skill says.

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
