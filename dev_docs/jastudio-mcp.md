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
  - **Its configuration is the same at every start, in every checkout** (`HeadlessConfigDictSource`): JAStudio's
    defaults, except that the batches run on one thread per logical core. A setting changed while it runs, such as a
    sentence breakdown's setting toggled with its hotkey, is kept in memory only. It never reads Magnus's configuration,
    the add-on's `meta.json`, which Anki keeps: he toggles the sentence breakdown's settings from second to second as he
    studies, so read at the start it made results depend on when the headless JAStudio was started (Magnus,
    2026-11-12). What he sees right now is the live JAStudio's to show.
  - **The user files are a copy of its own** (`HeadlessUserFiles`), of the add-on's, made as it starts: the add-on's are
    the live JAStudio's, where the window places and the workspace's state are saved.
  - **Its UI runs on Avalonia's headless platform** (`HeadlessUI`, in `JAStudio.Mcp`), on a thread of its own, as the UI
    inside Anki runs on one: windows open and are drawn as on a screen, but on none, for `render_window`. It has a
    workspace window, created as inside Anki, whose tabs show their pages as images (`NoteTabImageView`), since a native
    web view needs a real window. A page takes the keys pressed on it as the web view's does: the note view shortcuts,
    such as those that toggle the sentence breakdown's settings, work with `press_keys`.
  - **Its screen** (`HeadlessScreen`) is what a person would see of that UI: the windows open and their popups, where
    they are, one over another. The headless platform draws each window and popup on its own and keeps no track of which
    is in front, so the screen does:
    - **Popups are top levels of their own,** as on a real screen, not drawn in their window's overlay layer as the
      headless platform draws them by default: JAStudio's menus open from a window of one pixel (`PopupMenuWindow`),
      which would clip them. And they're shown without being activated, as a desktop shows them: the headless platform
      activates every window it shows, which took the keyboard from the control that opened a popup, and so closed an
      `AutoCompleteBox`'s suggestions as they opened (fixed 2026-11-09, found trying the vocab pickers).
    - **It activates windows as a desktop's window manager does** (`HeadlessWindowManager`), which the headless platform
      has none of: one window is active at a time, a click in an inactive window activates it, and when the active
      window closes or hides, the one in front is activated. JAStudio depends on it: its menus close as their window is
      deactivated. Avalonia offers no public way to deactivate a window, so it's done through reflection on the platform
      window's `Deactivated`, which fails loudly should Avalonia rename it.
  - **A failure in work the UI does on its own** (`FailuresNoCallWaitedFor`), such as an event handler for an activation
    the platform posted, is kept and fails the next call to the UI. Unhandled, it would end the UI thread's loop, and
    every call after it would wait forever.
  - **Web pages are rendered by a headless browser** (`PlaywrightWebPageImages`): the installed Edge on Windows, Chrome
    elsewhere, driven by Playwright, muted, since card pages play their note's audio. Only the headless JAStudio
    references Playwright, so its driver stays out of what Anki loads.
  - **Its card server is its own** (`CardServer.StartOnAPortTheSystemPicks`), on a port the system picks, leaving alone
    the port the card server inside Anki remembers.
  - **What needs Anki itself refuses with an exception:** creating a new note's Anki note, changing a note's cards, and
    importing. The note types for raw notes, which the import profiles window lists, it reads from the collection, as
    JAStudio inside Anki does.
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
  - **The default corpus checkout** depends on the source checkout (2026-11-12):
    - **In a linked worktree of the source repository,** the worktree's own corpus checkout, `src/jas_database`, at the
      commit its branch records. No JAStudio inside Anki works on it, and git keeps it in step with the branch.
    - **In the main checkout,** whose `src/jas_database` JAStudio inside Anki works on, the headless JAStudio's own
      worktree of the corpus repository, next to the source checkout's worktrees:
      `C:\Dev\JAStudio.worktrees\headless-corpus` for `C:\Dev\JAStudio`. It's created the first time it's needed.

    Before, every source checkout got such a worktree, made from its own corpus clone: a linked worktree's clone isn't
    Magnus's, so its commits never reached his checkout without a fetch. The Anki profile is by default the one whose
    collection file was written last.
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
  - `get_the_tag_tree` gives the tag tree, or one branch of it such as `source`, to as many levels as asked for: each tag
    path with the vocab, kanji and sentences in its branch, studied or not. Built (2026-11-10) to survey what the
    `source::` tags hold for the provider-and-work question, without a script reading the corpus files; the priority
    tags of slice 5 are the next such survey.
- **The UI, built, in the headless JAStudio only** (`JAStudioMcpServer.StartWithTheHeadlessUI`): in the live JAStudio a
  window opened would open on Magnus's screen, and it has no browser to render pages with. Each tool gives an image.
  - `render_window` renders one of the main menu's windows, the options, the readings mappings or the import profiles,
    opened as the menu opens it, and closes it again. To drive one, open it from the workspace's main menu.
  - `render_the_workspace` renders the workspace window as it is, after showing a note in a tab or in the editor, if
    given one, and on the editor's tab asked for, such as a vocab's Matching tab. It keeps its tabs from one call to the
    next, and starts as Magnus last left his workspace.
  - `render_card` renders a side of a note's card as Anki's reviewer shows it, the whole card however tall.
- **Driving the UI, built (2026-11-09), in the headless JAStudio only:** the screen-tools (`ScreenTools`) show the
  headless JAStudio's screen and act on it as a person does, so Claude can use JAStudio's UI on the real corpus:
  reproduce a bug, check a fix, or try a window being built.
  - `render_the_screen` renders every window and popup open, each where it is, and lists them with their refs and the
    control that has the keyboard. `render_the_workspace` leaves the workspace open on the screen, to drive.
  - `describe_the_screen` describes the screen as text: each window and popup, with the controls a person reads or acts
    on, from Avalonia's automation tree, the tree screen readers read. Each control has a ref, such as `c12`, then its
    kind, its text, its state, such as checked, selected, expanded, out of sight or has the keyboard, and where it is.
    A control keeps its ref (`ControlRef`) as long as it exists (`ControlRefs`). A control's line is its
    `ControlDescription`. Given a control's ref, it describes only that part of the screen, such as an editor's tab:
    the whole workspace's description is about 5,000 tokens (added 2026-11-09).
  - `click_a_control`, `type_text`, `press_keys`, `scroll_a_control` and `close_a_window` act on it through the headless
    platform's own mouse and keyboard, so the input goes where a person's would and does what a person's does, and each
    gives the screen afterwards, as an image unless told not to. Each waits until the UI has handled what it started,
    and a quarter of a second more for work in the background, such as a search.
  - **A control is named by its ref, not by where it is in an image:** a ref names the control exactly however the image
    is scaled to be shown, and the description gives its text and state exactly too. A click at a control's middle that
    would reach something else, such as a window in front of it, or the edge of the list it's scrolled out of, fails and
    says what.
  - **Input as a real desktop gives it:** a key without Ctrl, Alt or Meta held types its text too, which JAStudio's menus
    pick their items by; a key, and each press and release of a mouse button, goes to whatever has the keyboard, or is
    in front at the click's point, at that moment, since the one before may have closed a window.
  - **Found with it at once (2026-11-09):** picking an item from the search results' menu puts the keyboard back on the
    results, after a dialog the item opened gave it to its input. `PopupMenuWindow` learned of a pick from the click's
    routed event, which Avalonia's `MenuItem` marks handled, and only after the item's action had run. Fixed the same
    day: the pick is recorded as the item is picked, before its action (`SpecMenuRenderer.BuildMenuItems`), and the
    screen-tools showed the dialog's filter box keeping the keyboard.
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
- **Parsing speed, built (2026-11-12):** `time_parsing_every_sentence` times tokenizing every sentence of the corpus, and
  then analyzing every one, on a given number of threads, by default one per logical core, and saves nothing. It's for
  telling whether a change made parsing faster, and how parsing scales with threads: compare runs at the same thread
  count, and discard the first after a start, which includes warming up. Each pass also reports the processor time it
  used and the machine's other processes used meanwhile, which shows whether other work on the machine disturbed it (read
  on Windows only: .NET has no way that works everywhere); how long the garbage collector paused the threads; how much
  memory it allocated, the same in every run; and the most memory the process used, sampled every 100 ms. The run reports
  the process's garbage collector mode. By default it first collects all garbage, so each run's peak is its own. Runs
  at the same time would compete for the cores, so a call made while it or `count_the_candidate_words` runs waits for
  that one to finish (2026-11-12): several calls in one message may run at once, and did, making the times nonsense.
- **Candidate words, built (2026-11-12):** `count_the_candidate_words` analyzes every sentence of the corpus and counts
  the candidate words the analysis built, in all and by how many tokens each spans: how many were built, how many have a
  form a vocab or a dictionary entry has, the only ones that can get a match, and how many got a valid match. It showed
  that 80% of the candidate words the analysis built were compounds that couldn't match anything, which it no longer
  builds (`dev_docs/parsing-speed.md`).
- **The reparse after a vocab edit, built (2026-11-12):** `time_reparsing_the_sentences_of_a_vocab` runs, for one
  vocab, the reparse the vocab editor runs after each change (`LocalNoteUpdater.ReparseSentencesForVocab`), and reports
  how many sentences it reparsed, how long finding them took, how long the whole reparse took, and the processor time
  used, the other processes' included. Run right after a start and again, it tells the warm-up from the work. It found
  that finding a small vocab's sentences took longer than reparsing them (`dev_docs/parsing-speed.md`).
- **What parsing allocates, built (2026-11-12):** `sample_what_analyzing_every_sentence_allocates` analyzes every
  sentence of the corpus while an `EventListener` in the process sums the runtime's allocation events,
  `GCAllocationTick`, by type: one about every 100 KB each heap allocates, with the type it was allocating. It reports
  how much the pass allocated, counted exactly, how much of it the samples cover, and the types that took the most. No
  profile here shows allocations, dotTrace's timeline included, so this is how to find what parsing could allocate less
  of: it found LINQ delegates, iterators and closures, a sixth of it, that the analysis no longer allocates. It waits for any other parse of every
  sentence, as the timing tool does.
- **Shared-conditions, built (2026-11-12):** `list_the_shared_conditions` lists the corpus's shared-conditions, each with its
  definition as code and the vocab and shared-conditions that use it. `define_a_shared_condition` defines one, from its
  subject, its name and its definition in the JSON a vocab's match-condition is stored in, and saves it in its own file in
  the corpus checkout. Built to define the first five, made from the matching rules that are small trees, and for defining
  more while working on how sentences parse (`dev_docs/matching-configuration.md`, increment 6).
  `use_a_shared_condition_wherever_its_definition_is` uses one by name in every vocab's match-condition and the other
  shared-conditions' definitions wherever its definition is, and reports the vocab and shared-conditions it made use it:
  built to put the five to use in the corpus. `redefine_a_shared_condition` gives one another definition and reparses the
  sentences of every vocab whose match-condition depends on it, as its window in JAStudio does: built for changing them
  while working on how sentences parse (step 3 of increment 6). `rename_a_shared_condition` renames one, with its file, and
  saves every vocab and shared-condition using it by the new name: built for the naming pass (step 3 of increment 6).
  `delete_a_shared_condition` deletes one, with its definition written out wherever it's used, the reverse of making one.
  `render_window` renders the window listing them, "Shared-conditions".
  `find_the_conditions_vocab_share` lists the conditions, and the combinations
  of a group's conditions, that several vocab's match-conditions hold, as candidates for shared-conditions, each with its
  definition ready for `define_a_shared_condition`: built at Magnus's suggestion, to find what's worth naming (step 2 of
  increment 6). It proposes; a mind names and decides.
- **Memory, built (2026-11-12):** `describe_memory` reports the garbage collector's mode and settings, the managed
  heap, what the garbage collector has committed, and the process's working set and private memory; optionally also
  after the most thorough collection there is, which shows what the process keeps when it must give back all it can.
- **What the heap holds, built (2026-11-12):** `describe_what_the_heap_holds` collects all garbage, then counts every
  object on the managed heap from a snapshot of the process, with ClrMD (`HeapCensus`), as `dotnet-dump`'s
  `dumpheap -stat` and `-strings` count a dump's: the types that take the most, and the string values whose copies take
  the most. It's how to find what the notes take in memory, and to measure what a change saves: before it, a heap dump of
  1.4 GB and minutes of `dotnet-dump`. The snapshot can catch memory being made into objects, which reads as objects that
  aren't, such as strings of length -1 at the end of a region of the heap: only objects that end within their region, and
  strings of a length that isn't negative, are counted. See `dev_docs/memory-use.md`.
- **What holds a type's objects, built (2026-11-12):** `describe_what_holds_the_objects_of_a_type` counts, for one type,
  the types of the objects that reference its objects (`HolderCensus`): how many of them do, how many of the type's
  objects they reference, and those objects' megabytes; and how many only a root holds. Built to tell what holds the
  heap's 117 MB of strings and 106 MB of `byte[]`s, which a count by type can't. It counts direct references only: a
  list's strings are held by its array, and asking about the array's type tells what holds the arrays. It and
  `describe_what_the_heap_holds` read the same `HeapSnapshot`, one at a time. In the specifications they run in a
  collection of their own, after every other specification: a census of the specifications' process took over 100
  seconds while every other specification ran, a second alone.
- **The reservoir, built on the branch `corpus-reservoir` (2026-11-11):** `what_a_vocab_does_in_the_reservoir` tells what
  a vocab, as JAStudio has it now, does in a reservoir's millions of sentences (`WhatAVocabDoesInTheReservoir`): where it
  matches, which sentences reject it and why, and where its matching changed since the reservoir's bulk analysis, which
  is what an edit to the vocab did. It takes the reservoir's file as an argument; the reservoir's texts load at its
  first use, in up to a minute (`OpenReservoirs`), and every later use takes seconds. It's the near-instant feedback of
  `todo/expanding-the-sentence-corpus.md`, over the reservoir rather than the working set.
- **Writing: parity with the workspace's editors (Magnus, 2026-11-09).** Whatever Magnus can edit in JAStudio, Claude can
  edit through the MCP server, rather than one edit at a time as work needs it. Parity holds by construction: each note
  type's editable fields are defined once, in Core (`IEditableField<TNote>`, such as `VocabEditableFields`), each with its
  name, its label, the kind of value it takes and how it's read and written through the note's own operations. The
  note's editors in the workspace are built from them, and the MCP server edits all of them. Not yet built from them: the
  editors of a vocab's forms, parts of speech, whether it's usually written in kana and tags, which write through the
  same operations of the vocab, and show labels of their own.
  - **Built:** `get_the_editable_fields_of_a_note` lists a note's editable fields, with their values, and `edit_a_note`
    writes any of them, then does what the editor does after a change: reparses the sentences that may contain a vocab
    when a field written changes how sentences parse, then updates the note's generated data. A field is named by its
    path in the note as the corpus stores it, as `get_note` and the note diff show it: `corpusAnswer`,
    `matching.requiredOrForbidden.surface`, or, for a flag in a list of flags, `matching.flags.isPoisonWord`. They
    replaced the single-edit tools for a vocab's readings, parts of speech and matching flags.
  - **The plan, in increments:**
    1. The vocab's fields, and its editor's tabs built from them. **Done.**
    2. The kanji's and the sentence's fields the same way, replacing the kanji's readings tools. **Done.**
    3. What the notes' menus do:
       - **Done:** the fields the menus edit, as editable fields like any other, after those of the editor: a vocab's
         related vocab (`relatedVocab.synonyms` and the rest) and a sentence's configuration
         (`configuration.highlightedWords`, `configuration.incorrectMatches`, `configuration.hiddenMatches`). A set of
         them (`EditableSetField`) is written as the menus change it, one item at a time through the note's own
         operations, so what those do besides happens too: a synonym added links back, a perfect synonym gets the
         vocab's answer, an incorrect match reparses the sentence. Since a write can change other fields of the note,
         such as a vocab's synonyms when its perfect synonyms are written, `edit_a_note` reports every field whose value
         changed, not only those written. The editors don't show these fields: changing Magnus's editors isn't part of
         this.
       - **Done:** the menus' commands, defined once in Core as the fields are (`INoteCommand<TNote>`, such as
         `VocabCommands`), each with its name, the label its menu shows, the text it takes, if any, and whether it
         creates a vocab or changes the note. The menus are built from them, keeping their own structure and shortcuts,
         and `get_the_commands_of_a_note` lists them and `run_a_command_on_a_note` runs them, reporting each field of the
         note the command changed and every vocab it added to the collection, which includes those that
         autogenerating a vocab's compound parts creates for parts that have none. Accepting a meaning moved from the menus into the notes
         (`VocabNote.AcceptMeaning`, `KanjiNote.AcceptMeaning`), which refuse it while the note has an answer of its own,
         as the menus only offer it then.
       - **Left out:** the menu items that open notes, copy text to the clipboard or open a dialog, and reparsing a
         vocab's sentences, which `edit_a_note` does itself when a field written changes how sentences parse.
       - **Done:** `create_a_note` creates a vocab, a sentence or a kanji from a text, as the context menu's "Create"
         does: a vocab from the dictionary, then the sentences that may contain it reparsed; a kanji with the menu's
         answer, "TODO".
       - **Done (Magnus, 2026-11-09):** the context menu's universal note actions, `suspend_all_cards_of_a_note`,
         `unsuspend_all_cards_of_a_note` and `delete_a_note`. Suspending works only in the live JAStudio, which has an
         Anki to suspend cards in.
       - **Done (2026-11-10):** `add_a_tag_to_the_notes_found` and `remove_a_tag_from_the_notes_found` add a tag to,
         or remove it from, every note a search finds, as adding or removing it in a note's tags in the editor does, and
         refuse a tag JAStudio defines, as the editor does. Built for fixes to the corpus's tags, such as an episode tag
         put on another episode's sentences. A tag branch is renamed on some notes by adding the new tag and removing
         the old.
       - **Done (2026-11-10):** `delete_a_tag_branch` deletes a tag and every tag below it, as the tag tree's Delete
         does: a branch of the notes' own tags from every note with a tag in it, a branch below `anki::` in Anki, from
         every Anki note with a tag in it, as one Anki operation. Built for the cleanup drive of the tags JAStudio had
         written into Anki (`corpus-data-model.md`), about two dozen branches, some with over a hundred paths below.
       - **Deleting a note, as decided with Magnus:** JAStudio is the master of the notes, and Anki only of a student's
         studying, so Anki never prevents a deletion, and JAStudio deletes notes outside Anki too. What happens to a
         note's Anki note, which holds its cards and their review history, is a choice (`ItsAnkiNote`): kept as it is,
         or deleted with the history, which can't be undone. The menu's delete dialog asks for the choice when the note
         has an Anki note, keeping it by default; `delete_a_note` takes it as `ankiNote`, and fails without it for such a
         note, as the dialog's question would stop a person. A kept Anki note is left alone, not suspended, which
         changes only whether its cards come up for review: dealing with it in Anki is Magnus's to do. Its cards show
         that the note no longer exists in JAStudio. The headless JAStudio can only keep an Anki note.
  - **Left out on purpose:** generating text-to-speech audio, which calls Azure or Google and costs money. A note's
    media lists aren't editable fields: `get_note` shows them as the corpus stores them, and the workspace's editors
    remove files from them.
  - A kanji's readings of each kind are one field, such as `readings.on`, a list of `{"kana", "isPrimary"}` in order,
    which replaced the three tools that added, removed and made primary one reading at a time.
- **Running, built:**
  - `full_rebuild` runs `LocalNoteUpdater.FullRebuild`, as the workspace's menu does, and reports how long it took and
    what it changed.
  - `save_every_note` writes every note as JAStudio stores it now, changed or not, which JAStudio otherwise never does:
    on a clean checkout, the notes whose files then change are those JAStudio would write differently from their files.
    Built 2026-11-10 for slice 9, to see what a change to how notes are stored does to the files, and to check a
    migration: after it, saving every note must change nothing. Meant for the headless JAStudio's own worktree.
  - `list_changed_notes` gives the notes whose files in the corpus checkout differ from its git `HEAD`, by note type, from
    `git status`. How each changed is what `diff_the_changed_notes` shows.
  - `reload_the_collection` loads the collection again from the corpus checkout, and reports the notes it found changed
    on disk.
  - `fast_forward_the_corpus_checkout` fast-forwards the corpus checkout to a commit and reloads, and reports the commits
    before and after and the notes whose files it changed. Git refuses what isn't a fast-forward, or would overwrite
    changes not committed.
- **Running, proposed:** reparsing some sentences, regenerating a note's data.
- **The study-notes in Anki, built (2026-11-10):** a note's study-note is the Anki note holding its cards, with the few
  fields Anki needs (the question, the audio, the note's id). Its tags are the student's, which JAStudio doesn't write
  (`corpus-data-model.md`, "The corpus's tags and Anki's tags are separate"). JAStudio writes the fields whenever it
  updates the note, but a reload passes on only the notes whose files changed while it held them, and none at startup, so a change
  to the corpus made while Anki was closed, or a change to what JAStudio writes, leaves study-notes behind. Built for
  slice 9 of `corpus-data-model.md`, whose change of format is fast-forwarded with Anki closed.
  - `list_the_study_notes_that_differ` compares every study-note in Anki (`IAnkiStudyNotes`, read straight from Anki's
    database) with the one JAStudio would write now (`StudyNoteComparison`), as Anki keeps what's written into it: text in
    Unicode normalization form C. It counts the differences by field, and lists the notes, each with the fields that
    differ. It also counts and marks the notes where only the markup of the audio differs: the same audio files in the
    same order, with only the markup around them, or which audio field holds which file, different
    (`NoteWhoseStudyNoteDiffers.OnlyTheMarkupOfItsAudioDiffers`), and leaves them out of the lists when asked. Added
    2026-11-10 for slice 9, when moving the audio to lists made 475 study-notes differ so, and none otherwise.
  - `write_the_study_notes_that_differ` writes them, or the study-notes of the notes given, through the same Python code
    that writes a study-note on every update (`JPNoteDataShim.write_study_note`), then compares again. Only in the live
    JAStudio: the headless one compares, reading the collection file, but has no Anki to write in.
  - **The first comparison (2026-11-10, headless, on the corpus at `8a1a6404c9`):** of 79,719 study-notes, 16 differ:
    sentences whose questions have spaces or a tab at an end that the study-note hasn't. None differs in its audio. It
    compared tags then too: 21,236 study-notes had tags in Anki that the corpus had dropped, those slice 6 dropped
    (`has-no-studying-sentences` on 17,073 vocab, the kanji's study-state tags), and `_uk`, `_disable_uk` and the vocab's
    matching and register tags, which earlier slices turned into data. No template, filtered deck, saved search or
    add-on code uses them. That showed JAStudio replacing every study-note's whole tag list, which ended the same day.
- **A diff of notes rather than of files, built (Magnus, 2026-11-09):** `diff_the_changed_notes` compares the notes whose
  files differ between two versions of the corpus: by default the checkout's `HEAD` and its files as they are now, which
  is what a full rebuild changed, or any two commits.
  - **Both versions of each note's file are read into notes,** as JAStudio reads notes, and written again as it stores
    them, so the comparison is of notes: a file that differs only in how it's written counts as changed only in its file.
    The versions in commits are read with a single `git cat-file --batch`, however many there are.
  - **Notes are compared property by property:** each value changed, before and after, and the items added to and removed
    from each list, such as a vocab's forms or a kanji's kun readings. It counts how many notes each property changed in.
  - **A sentence's parsed words are compared as words,** each with the vocab it's matched to, by question: words added,
    removed, now displayed and no longer displayed. Those are also counted over all the sentences, word by word, which is
    how a change to parsing played out across the corpus at a glance.
  - **It can't compare across a change to how the corpus stores notes:** a file from before the change can't be read into
    a note, and the tool says which file in which version.

  Still proposed: the same comparison under JAStudio's own features Magnus would love: committing inside JAStudio while
  seeing which notes changed and how, a note's history with rolling back to a version, and which sentences a change to
  vocab changed. The git command line suits it: on the corpus repository (207 commits, 79,718 note files) `git status`
  took 0.17 seconds and a note's history 0.035.

  **Committing inside JAStudio may come before the changes to the vocab's matching configuration** (Magnus,
  2026-11-10): it would turn his commits, now a manual round of JAStudio and several repositories in Git Extensions,
  into two or three clicks. What it would do:
  - run a full rebuild before committing, so a commit always holds everything its edits changed, as the process for
    changes to the corpus requires (`.claude/rules/02-universal-local/045-committing-with-the-corpus-repository.md`);
  - show the changes as notes, as `diff_the_changed_notes` does;
  - commit the media submodules too, innermost first;
  - tell the notes edited by hand from those a rebuild only regenerated, since JAStudio knows which notes an editor
    or a command wrote and which a rebuild did, and name the edited ones in the commit message. The diff alone can't:
    a rebuild changes generated data such as a vocab's `sentenceCount` on notes nobody edited.

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
     runs, so it didn't show that a full rebuild reaches a stable result in one run. A specification checks that now
     (`corpus-data-model.md`, open question 14).
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
   with no console window. The next live fast-forward, after Anki restarted on that build (the kanji readings fix), ran
   through and reported the two kanji it changed, in 4.3 seconds.
5. **Under way:** the writing tools, built for the data fixes they're needed for. The first, for the suru verbs stored as
   godan verbs (`corpus-data-issues.md`): looking the six up in the dictionary, setting their parts of speech in the headless
   JAStudio, and a full rebuild, which changed only those six vocab. A second run changed nothing more. The second, for
   the kanji readings that looked wrong: the three reading edits, and a full rebuild that changed only the two kanji.
6. **Done:** the diff of notes, `diff_the_changed_notes`, to analyze rebuilds with instead of throwaway scripts over the
   git diff. Checked on the real corpus (2026-11-09):
   - **Over the two data fixes** it reported exactly them: six vocab with `godan verb` removed from their parts of speech,
     and the kanji readings removed and added.
   - **Over Magnus's commit adding 点が入る** it showed the three sentences that now parse it, each hiding 点, が and 入る.
   - **Over all 49,162 sentences,** with a line break added to each file, it found every one changed only in its file, in
     about 20 seconds.
   - **It found 23 sentences that store the same parsed word twice:** えば, at the same place and matched to the same vocab,
     displayed once and hidden once, as in 触れれば. The comparison counts words as many times as a sentence has them.
     Whether the twice-parsed word is right is a question for the parsing.
