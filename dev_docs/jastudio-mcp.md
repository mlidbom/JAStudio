# JAStudio's MCP server — Design

> **Status: agreed direction (Magnus, 2026-11-09); steps 1 to 4 are built: the headless JAStudio behind the MCP gateway, the full rebuild and analyzing a sentence as tools, and the same server in the live JAStudio, with reloading the collection and fast-forwarding the corpus checkout.** Details
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
  valuable in itself). *Built: `reload_the_collection`, and the workspace's Local Actions > "Reload the collection from
  disk".*
- **A change to the notes made and checked in the headless JAStudio reaches the live one without a restart** (Magnus,
  2026-11-09), when it needs no new code: Claude commits it in the headless JAStudio's worktree, shows Magnus what it
  changed, and on his word fast-forwards his corpus checkout to it through the live JAStudio
  (`fast_forward_the_corpus_checkout`), which reloads. If his checkout's `HEAD` has moved meanwhile, as it does all
  day, Claude redoes the change on top of it instead of rebasing rebuilt data. A change that needs new code goes the
  other way: the fast-forward is made while Anki is closed, and Anki's restart on the new build loads it.

## How it's built

Three projects, plus the live JAStudio's part in `JAStudio.Anki.PythonInterop`, and the specifications of the tools
(`JAStudio.Mcp.Specifications`):

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
- **The live JAStudio, inside Anki,** runs the same server (`LiveJAStudioMcpServer`, started from
  `JAStudioAnkiAppRoot`):
  - **It starts once the collection has loaded for the first time,** on a port the system picks, so its tools never see
    a collection still loading at startup, and stops as Anki closes.
  - **It announces itself** in `%LOCALAPPDATA%\JAStudio\live-jastudio.json` (`LiveJAStudioAnnouncement`): the server's
    address, Anki's process id and the corpus checkout. The file is deleted when the server stops.
  - **Anki takes in the server's assemblies** because `JAStudio.Anki.PythonInterop` copies its NuGet assemblies to its
    output (`CopyLocalLockFileAssemblies`), which `copy_runtime_binaries.py` copies Anki's runtime from. That added
    exactly the MCP SDK's assemblies and `JAStudio.Mcp.dll`, and changed none Anki already had (checked 2026-11-09).
- **Reloading** (`JPCollection.ReloadFromBackend`, and `ReloadFromBackendAfter`, which makes a change to the corpus
  checkout, such as the fast-forward, while the collection holds no notes):
  - **Each note whose file changed on disk is passed on as updated,** so what follows note updates catches up, such as
    its Anki note: a reload compares each note's data with the same note's as held before. Saving it again on the way
    leaves the checkout as git has it: after a fast-forward on the real corpus, `list_changed_notes` showed nothing.
  - **Notes added or deleted on disk** are loaded or dropped, but not passed on: an added note gets no Anki note, and a
    deleted note's Anki note stays. A change that adds or deletes notes is better made through JAStudio.
- **`JAStudio.Mcp.Gateway`, the MCP gateway:** the stdio MCP server Claude Code starts, once for each JAStudio, chosen by
  its `--jastudio` argument:
  - **`jastudio-headless`** in `.mcp.json` (`Headless/`): its own tools start and stop the headless JAStudio, as below.
  - **`jastudio-live`** (`Live/`): its own tool, `connect_to_live_jastudio`, reads the live JAStudio's announcement,
    connects to its MCP server and offers its tools. They're withdrawn when Anki's process exits; connecting again
    after Anki restarts offers them again. Claude Code tells the two gateways' tools apart by the server's name:
    `mcp__jastudio-live__search_notes` and `mcp__jastudio-headless__search_notes`.
  - **Claude Code starts it through `start-the-gateway.ps1`,** which builds it into a folder of its own for that start,
    `%LOCALAPPDATA%\JAStudio\mcp-gateway\build-for-process-<process id>`, and runs it from there. Run from its build
    output instead, it would lock that output for the whole session, and every build of the solution that recompiled
    it, in Rider or for the Definition of Done, would fail on the locked file. That happened (2026-11-09) even though
    the gateway hadn't changed: Magnus's rebuild in Rider recompiled it to identical bytes, and the next build failed
    to copy them over the locked file, since MSBuild copies by timestamp, not content. Claude Code starts both gateways
    at once, so the script builds one at a time (a named mutex): two builds of one project at once fail on each other's
    files in its `obj` folder.
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

**The gateway is the one part that can't change without Magnus.** It runs for the whole session, so a changed gateway
is taken in only when Claude Code reconnects to it, with `/mcp` or a new session, when the start script builds it
again. So it references no JAStudio assembly and knows nothing of JAStudio's tools. Everything that changes belongs in
the headless JAStudio.

## The tools

- **Reading, built:**
  - `search_notes` takes a search in the JSON form the workspace keeps searches in (`NoteConditionJson`, see
    `note-search.md`). It counts the notes found, and lists up to 50 of them, or as many as asked for.
  - `get_note` gets a note as the corpus stores it.
  - `describe_collection` gives the corpus folder and the notes of each type, studied or not.
- **The dictionary, built:** `look_up_in_the_dictionary` looks up a word, or a vocab as JAStudio looks it up, and shows each
  entry's forms and senses, with the dictionary's own parts of speech and the ones JAStudio maps them to.
- **Analysis, built:** `analyze_sentence` analyzes a sentence, given as text or by its note, with the note's
  configuration. It reports:
  - the tokens, after JAStudio's preprocessing, each with the token it was split from;
  - every candidate word that got a match, at its place in the sentence;
  - each match, to a vocab, a dictionary entry, or missing, with whether it's valid and displayed, and the reasons it
    isn't (`Match.FailureReasons`, `Match.HidingReasons`).

  It replaces the throwaway tokenizer programs, and answers questions such as why a word doesn't match a vocab: it
  showed at once why まかれる parses as まかる + える + う (MeCab reads まかれる as one ichidan verb, and the preprocessing
  splits it as a godan potential).
- **Writing:** the edits the workspace's editors make, through the same domain operations, added as the work needs them.
  - **Built:** `set_the_parts_of_speech_of_a_vocab` replaces a vocab's parts of speech, as adding and removing them in its
    editor does, and reports them before and after. It doesn't reparse sentences: `full_rebuild` shows what the change does.
  - **Built:** `add_a_reading_to_a_kanji`, `remove_a_reading_from_a_kanji` and `set_whether_a_kanji_reading_is_primary`,
    the three edits the kanji's readings editor makes, each reporting the kanji's readings before and after.
- **Running, built:**
  - `full_rebuild` runs `LocalNoteUpdater.FullRebuild`, as the workspace's menu does, and reports how long it took and
    what it changed.
  - `list_changed_notes` gives the notes whose files in the corpus checkout differ from its git `HEAD`, by note type, from
    `git status`. How each changed is in the checkout's git diff.
  - `reload_the_collection` loads the collection again from the corpus checkout, and reports the notes it found changed
    on disk.
  - `fast_forward_the_corpus_checkout` fast-forwards the corpus checkout to a commit and reloads, and reports the commits
    before and after and the notes whose files it changed. Git refuses what isn't a fast-forward, or would overwrite
    changes not committed.
- **Running, proposed:** reparsing some sentences, regenerating a note's data.
- **Proposed, from git (Magnus, 2026-11-09):** a diff of notes rather than of files. Both versions of a note's file,
  from git, read into notes and compared as notes: forms added, matching configuration changed, a sentence's words now
  matching other vocab. A tool first, to analyze rebuilds with, then the same comparison under JAStudio's own features
  Magnus would love: committing inside JAStudio while seeing which notes changed and how, a note's history with rolling
  back to a version, and which sentences a change to vocab changed. The git command line suits it: on the corpus
  repository (207 commits, 79,718 note files) `git status` took 0.17 seconds and a note's history 0.035.

## Open questions

None at the moment.

Answered:
- **How a headless experiment's results reach Magnus's checkout:** fast-forwarded through the live JAStudio, which
  reloads (see [Decided](#decided)).
- **How the gateway reaches the live JAStudio:** by the file the live JAStudio announces its server in, from a gateway
  of its own, `jastudio-live` (see [How it's built](#how-its-built)).
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
2. **Done:** the full rebuild as a tool.
   - **On a clean corpus checkout it changed nothing,** in 7.9 seconds, so the headless rebuild does what the rebuild
     inside Anki did.
   - **A second run changed nothing either,** in 5.9 seconds. That checkout had already settled, after Magnus's two
     runs, so it doesn't yet show that a full rebuild reaches a stable result in one run (`corpus-data-model.md`, open
     question 14). The test for that is the same two runs, after a change whose effect on vocab feeds back into parsing.
3. **Done:** analyzing a sentence.
4. **Done:** the same server in the live JAStudio, inside Anki, reloading the collection from disk as a tool and in the
   UI, and fast-forwarding the corpus checkout. Checked with the headless JAStudio on the real corpus: a fast-forward to
   a test commit changing one vocab reported that vocab and left the checkout clean, and a git reset back and a reload
   found it changed back. The `jastudio-live` gateway was checked against the headless JAStudio standing in for the live
   one: it connected, forwarded calls, and withdrew the tools when the announced process exited. Inside Anki
   (2026-11-09): `connect_to_live_jastudio` connected, the counts of notes matched the corpus checkout's files, studying
   statuses came from Anki, and a reload took 3.1 seconds and found nothing changed on Magnus's clean checkout. The
   first real live fast-forward (2026-11-09, the suru-verb fix) moved Magnus's checkout and reloaded the six vocab, but
   its report failed: the last git command, listing the changed notes, was ended with `STATUS_CONTROL_C_EXIT`, the exit
   code of a process whose console closed. Anki has no console, so each git command got one of its own; git now runs
   with no console window. That waits for Anki's next restart to be checked.
5. **Under way:** the writing tools, built for the data fixes they're needed for. The first, for the suru verbs stored as
   godan verbs (`corpus-data-issues.md`): looking the six up in the dictionary, setting their parts of speech in the headless
   JAStudio, and a full rebuild, which changed only those six vocab. A second run changed nothing more. The second, for
   the kanji readings that looked wrong: the three reading edits, and a full rebuild that changed only the two kanji.
