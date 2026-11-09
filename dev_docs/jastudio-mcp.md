# JAStudio's MCP server — Design

> **Status: agreed direction (Magnus, 2026-11-09), nothing built yet.** Details are settled as each increment needs them.
> The corpus data-model work (`corpus-data-model.md`, "Order of work") waits for this: its remaining slices are mostly
> migrate, rebuild, analyze, tweak and rebuild again, which this makes Claude's own loop instead of a round trip through
> Magnus.

## Why

Developing JAStudio's parsing and data model is mostly tweaking something and running it again on the real corpus.
Until now only Magnus could run JAStudio on the real corpus, inside Anki: every full rebuild, every look at how a
sentence parses, and every migration (which needs Anki closed) was a round trip through him. In slice 7 alone that was
four full rebuilds, and the analysis of a parse meant reading stored JSON and writing throwaway programs that ran the
tokenizer.

An MCP server in JAStudio gives Claude tools to work with JAStudio directly: look at notes, see how a sentence parses and
why, change notes, run a full rebuild and see what it changed. Magnus expects it to become a constant way of working,
not an occasional one, so it's built as a permanent part of JAStudio, not as throwaway tooling.

## Principles

- **One MCP server, two ways to start JAStudio.** The tools only use JAStudio's own services (the collection, the
  dictionary, the analysis, `LocalNoteUpdater`), so the same server runs in both:
  - **live, inside Anki**, as JAStudio runs now, so Claude can help Magnus with real, live data while JAStudio is open;
  - **headless, without Anki or a UI**, a process Claude builds, starts and restarts itself. Anki takes in a new build
    only when it restarts, so after a code change only the headless JAStudio runs the new code without Magnus.
    The test environment's registrar (`TestEnvironmentSpecificDependenciesRegistrar`) shows the seams this needs: the
    note repository, the Anki backends, and the configuration sources Anki provides.
- **Reading and writing, live too** (Magnus, 2026-11-09). The corpus is a git repository, so every change shows as a
  diff and can be undone: disasters are all but impossible.
- **Writes go through the domain model,** as the UI's do, so each change is saved, and updates generated data, the way
  any edit does. No tool edits the corpus's files directly.
- **Every operation is a plain tool call.** A full rebuild takes about 8 seconds, so nothing needs to run in the
  background.

## The first tools (a proposal)

- **Reading:** search notes with the note search syntax (`note-search.md`), get a note as the corpus stores it, look a
  word up in the dictionary (its entries, senses and parts of speech, raw and mapped).
- **Analysis:** analyze a sentence, given as text or by its note: the tokens, every match with whether it's displayed,
  and why a candidate was rejected. This replaces the throwaway tokenizer programs, and answers questions such as why
  a word doesn't match a vocab.
- **Writing:** the edits the workspace's editors make, through the same domain operations.
- **Running:** a full rebuild, reparsing sentences, regenerating a note's data. What a run changed is in the corpus
  repository's diff, which Claude reads with git.

## Open questions

1. **Which corpus the headless JAStudio works on.** Two JAStudio processes writing the same checkout would overwrite
   each other's changes. A git worktree of the corpus repository would make each experiment a branch of its own, but how
   its results then reach Magnus's checkout, while the live JAStudio holds the corpus in memory, needs working out.
2. **Migrations and the live JAStudio.** A migration rewrites the corpus's files, which the live JAStudio would
   overwrite from memory, so today Anki is closed first. A tool that reloads the collection from disk might be enough.
3. **The endpoint.** The card server listens on a port that may change (`CardServer`), but Claude Code's configuration
   needs a fixed address for each JAStudio, the live one and the headless one, which may run at the same time.
4. **Rebuilding while the headless JAStudio runs.** Windows locks the assemblies a process has loaded, so a build would
   fail while it runs. It might run from a copy of the build output, as Anki's JAStudio runs from `runtime_binaries`.
5. **For Claude to verify:** whether Claude Code reconnects to an MCP server over HTTP after the server restarts, and
   whether tools from a server that wasn't running when the session started can be connected later.

## Order of work (tentative)

1. The headless JAStudio on the real corpus, with the MCP server and a few reading tools: enough to confirm Claude Code
   connects, and that it reads the corpus as the live JAStudio does.
2. The full rebuild as a tool. A headless full rebuild of a clean corpus checkout must change nothing, which shows it
   does what the rebuild inside Anki does. Running it twice and checking that the second run changes nothing is also the
   test for a full rebuild that reaches a stable result in one run (`corpus-data-model.md`, open question 14).
3. Analyzing a sentence.
4. The same server in the live JAStudio, inside Anki.
5. The writing tools.
