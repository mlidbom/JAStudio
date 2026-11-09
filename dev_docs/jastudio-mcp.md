# JAStudio's MCP server — Design

> **Status: agreed direction (Magnus, 2026-11-09); step 1, the headless JAStudio behind the MCP gateway, is built.** Details
> are settled as each increment needs them. [How it's built](#how-its-built) describes what exists.
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
- **Claude owns it** (Magnus, 2026-11-09). It's Claude's UI into JAStudio: Claude adds and changes its tools as the
  work needs them, without asking first, tells Magnus what it added, and commits it separately. See the project rule
  `090-claude-owns-the-jastudio-mcp-server.md`.

## Decided

- **Which corpus checkout the headless JAStudio works on** is chosen case by case, by default a git worktree of the
  corpus repository of its own (Magnus, 2026-11-09). Two JAStudio processes writing the same checkout would overwrite
  each other's changes.
- **Reloading the collection from disk** is how a migration, or resetting the corpus with git, reaches a running
  JAStudio: an MCP tool, and also a command in JAStudio's UI (Magnus, 2026-11-09: resetting with git and reloading is
  valuable in itself).

## How it's built

Three projects, plus the specifications of the tools (`JAStudio.Mcp.Specifications`):

- **`JAStudio.Mcp`, the MCP server:** the tools, served over HTTP on the loopback address only, stateless, by
  `JAStudioMcpServer`. Its tools use JAStudio's services through the same child container the card server uses. An
  exception a tool throws reaches Claude in full, type, message and stack trace, as the tool's error result
  (`ToolExceptionReporting`).
- **`JAStudio.HeadlessHost`, the headless JAStudio:** JAStudio wired by `HeadlessEnvironmentDependenciesRegistrar`
  instead of Anki's registrar.
  - **The notes** come from the corpus checkout it's started on, saved there as inside Anki.
  - **The studying statuses and Anki note ids** come from an Anki profile's `collection.anki2`, read as JAStudio inside
    Anki reads it. While Anki runs, its latest changes may still be in the database's write-ahead log, which reading
    the file doesn't see.
  - **The configuration** is read from the add-on's `meta.json`, never written: Anki owns it.
  - **What needs Anki itself refuses with an exception:** creating a new note's Anki note, changing a note's cards, and
    importing.
  - It loads the collection and starts the MCP server, on a port the system picks, then writes the server's address to
    standard output, and exits when the process it was told to live with, the gateway, exits.
- **`JAStudio.Mcp.Gateway`, the MCP gateway:** the stdio MCP server Claude Code starts, `jastudio-headless` in
  `.mcp.json`.
  - **Its own tools** are `start_headless_jastudio` and `stop_headless_jastudio`. Starting builds the headless JAStudio
    from the source checkout the gateway runs in, so it always runs the checkout's code as it is. It copies the build
    output to `%LOCALAPPDATA%\JAStudio\headless\runtime-of-gateway-<process id>` and runs it from there, so building
    JAStudio while it runs never fails on a locked file. Then it connects to the headless JAStudio's MCP server.
  - **While the headless JAStudio runs,** the gateway offers its tools as its own and forwards their calls. Changing the
    gateway's tool list sends Claude Code `notifications/tools/list_changed`, and Claude Code fetches the new list.
  - **Each run's output** is logged in `%LOCALAPPDATA%\JAStudio\headless\logs`. A run that fails to start, or a build
    that fails, fails the tool call with the end of the log or the build's output.
  - **The default corpus checkout** is the headless JAStudio's own worktree of the corpus repository, next to the source
    checkout's worktrees: `C:\Dev\JAStudio.worktrees\headless-corpus` for `C:\Dev\JAStudio`. It's created the
    first time it's needed. The Anki profile is by default the one whose collection file was written last.
  - **Restarting takes about 7 seconds** when the build is up to date. The first load on a corpus checkout takes about
    20, since it builds the checkout's `snapshot.bin` caches.

### Why a gateway

Claude Code can't reconnect an MCP server by itself (its documentation, `code.claude.com/docs/en/mcp`, checked
2026-11-09):
- a server that isn't reachable when the session starts stays failed until Magnus reconnects it with `/mcp`;
- a stdio server that exits does too.

A server Claude restarts to take in a new build would need Magnus every time, so the server Claude Code connects to
never restarts: the gateway runs for the whole session, and the headless JAStudio behind it restarts at will. Claude
Code does follow a server's tool list as it changes, which is how the headless JAStudio's tools come and go.

**The gateway is the one part that can't change without Magnus.** Claude Code runs it from its build output for the
whole session, so a changed gateway needs `/mcp` to reconnect it, and on Windows building it while it runs fails on its
locked files. So it references no JAStudio assembly and knows nothing of JAStudio's tools. Everything that changes
belongs in the headless JAStudio.

## The tools

- **Reading, built:**
  - `search_notes` takes a search in the JSON form the workspace keeps searches in (`NoteConditionJson`, see
    `note-search.md`). It counts the notes found, and lists up to 50 of them, or as many as asked for.
  - `get_note` gets a note as the corpus stores it.
  - `describe_collection` gives the corpus folder and the notes of each type, studied or not.
- **Reading, proposed:** look a word up in the dictionary (its entries, senses and parts of speech, raw and mapped).
- **Analysis:** analyze a sentence, given as text or by its note: the tokens, every match with whether it's displayed,
  and why a candidate was rejected. This replaces the throwaway tokenizer programs, and answers questions such as why
  a word doesn't match a vocab.
- **Writing:** the edits the workspace's editors make, through the same domain operations.
- **Running:** a full rebuild, reparsing sentences, regenerating a note's data. What a run changed is in the corpus
  repository's diff, which Claude reads with git.

## Open questions

1. **How a headless experiment's results reach Magnus's checkout,** from the worktree it ran on: committed there and
   merged, then reloaded in the live JAStudio, most likely.
2. **How the gateway reaches the live JAStudio** (step 4). The headless JAStudio's address needs no configuring, since
   the gateway starts it and reads it. The live one starts with Anki, so the gateway must find it: a fixed port, or an
   address the live JAStudio writes to a file. Most likely a second gateway, `jastudio-live`, offering its tools once it
   finds it.

Answered:
- **Rebuilding while the headless JAStudio runs:** it runs from a copy of its build output (see
  [How it's built](#how-its-built)).
- **Whether Claude Code reconnects to a restarted server:** it doesn't, which is why there's a gateway (see
  [Why a gateway](#why-a-gateway)).

## Order of work (tentative)

1. **Done:** the headless JAStudio on the real corpus, behind the MCP gateway, with three reading tools. It reads the
   corpus as the live JAStudio does: `get_note` gives a note exactly as its file has it, and the counts of notes and of
   notes studied match the corpus and Anki's profile. Claude Code follows the gateway's tool list as it changes:
   after `start_headless_jastudio`, the headless JAStudio's tools are there to call in the same session (confirmed
   2026-11-09).
2. The full rebuild as a tool. A headless full rebuild of a clean corpus checkout must change nothing, which shows it
   does what the rebuild inside Anki does. Running it twice and checking that the second run changes nothing is also the
   test for a full rebuild that reaches a stable result in one run (`corpus-data-model.md`, open question 14).
3. Analyzing a sentence.
4. The same server in the live JAStudio, inside Anki, and reloading the collection from disk, as a tool and in the UI.
5. The writing tools.
