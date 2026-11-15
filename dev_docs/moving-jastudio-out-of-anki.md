# Moving JAStudio out of Anki

> **Status: direction agreed with Magnus on 2026-11-14 and 2026-11-15; increment 1, the names, is built, and increment
> 2, the Sync in a Studying view, is built as Claude proposed it, but for its last part, which waits for Magnus to
> have run a sync inside Anki. Of increment 4, the HTTP path, the Anki add-on's HTTP server is built, and nothing
> uses it yet.** The order of work is preliminary. Details are settled as each increment needs them. The names are
> Claude's, and wait for Magnus's pass: see [Increment 1 as built](#increment-1-as-built-the-names-2026-11-15).

## The goal

**JAStudio becomes a program of its own, and its Anki add-on becomes pure Python.** No .NET runs inside Anki, and the
two talk HTTP on the same machine.

Magnus, 2026-11-14: "Where we are now with moving code into .NET I think it is time to consider whether any .NET code
should run through Anki at all."

It continues what the standalone JAStudio (`standalone-jastudio.md`) and the headless JAStudio (`jastudio-mcp.md`)
began. It's also the answer to item 2 of `speeding-up-development.md`: a new build reaching the live JAStudio without
Anki restarting.

## Terms

Decided by Magnus on 2026-11-15, on Claude's suggestion. There are three things to name:

- **spaced repetition system** — Anki as the domain sees it: the system where the student studies, which schedules the
  reviews. Anki is the only one today. Until then `studying.md` said *studying-add-on*.
  - In names it's **SRS**, an approved exception to the rule against abbreviations (Magnus, 2026-11-15: "SRS Is a well
    known acronym, and anyone dealing with this domain should know it already"): see
    `code_standard_exceptions/srs-for-the-spaced-repetition-system.md`. In prose it stays spelled out.
- **the Anki integration** — JAStudio's code that works with Anki: `JAStudio.Anki`.
- **the Anki add-on** — the Python code that Anki loads, in `src/jastudio_src`. Add-on is Anki's own word for such
  code.

**Nothing in JAStudio is called an add-on.** On 2026-11-14 Magnus held that a studying-add-on and an Anki add-on don't
collide: "Anki addon and JASTudio addon make perfect sense to me." On 2026-11-15 he found that they do: "The problem
here is that even qualified with Anki addon is ambiguous".

- "The Anki add-on" read as the add-on in JAStudio that provides studying information from Anki, and as the add-on
  loaded in Anki that talks to JAStudio. "JAStudio Anki add-on" read both ways too.
- Each was an add-on to one of the two programs for the other, so every name built from the three words fitted both.
- A shared word is harmless while what qualifies the two names differs, as with a build server and a card server. Here
  the qualified names coincided.

## What crosses between Anki and JAStudio today

From reading the code on 2026-11-14. Today pythonnet loads JAStudio's .NET side into Anki's process
(`load_dotnet_runtime.py`), from `src/runtime_binaries`, 190 MB.

- **The Python layer is about 2,700 lines.**
- **From .NET into Anki: 23 functions** (`AnkiFacade`, backed by `anki_facade_backend.py`). They take and return ids,
  strings and lists of them. The one exception takes a note, to write its study-note from.
  - Reading Anki's database is one of them: each read is a SELECT that returns one JSON text (`AnkisOwnConnection`).
- **From Anki into .NET:**
  - *Notifications of what happened in Anki:* a profile opened or is closing, Anki's own sync is starting or has
    finished, an operation changed notes, a note was added or a file imported, notes are being deleted, and cards were
    suspended or unsuspended.
  - *What the reviewer needs for each card:* the card's page, which is already an iframe on JAStudio's `CardServer`;
    how long to show the question before auto-advancing; and the settings that the scheduling hooks read.
  - *What Magnus starts himself:* the note view shortcuts (`NoteViewShortcuts`), Ctrl+O and Ctrl+Shift+O, which show the
    workspace, and the Qt menus that C# describes and Python builds.
- **The seams between Core and Anki already exist,** since the standalone and the headless JAStudio run without Anki.
  By their names since increment 1, and as they are since increment 2 made one interface of three: `SRSAvailability`,
  `ISRSWindows`, `ISRSStudyNotes`, `ISRSTagOperations`, `IAnkiRawNotes` and `AnkiCollectionDatabase`.

## Decided

### JAStudio is a program of its own, and the Anki add-on is pure Python (Magnus, 2026-11-14)

- **The standalone JAStudio becomes the JAStudio Magnus works in,** connected to Anki. JAStudio inside Anki goes, with
  everything that exists to load .NET into Anki.
- **Not a thin .NET layer in Anki that talks Compze.Tessaging with JAStudio,** which Magnus put as the alternative. It
  would keep what causes today's trouble: pythonnet, with a native binary for each operating system and Python version
  (today only `_cffi_backend.cp313-win_amd64.pyd`), and a .NET runtime installed on the student's machine or bundled
  for each platform. And it would add a boundary instead of moving one: Python to .NET through pythonnet, then .NET to
  JAStudio.
- **No Tessaging from Python.** Magnus, 2026-11-14: "Forget talking to tessaging from python I think", since it's "a
  custom type routing based messaging platform with handshakes, discovery, event, command and query routing, connection
  protocols, etc etc".

### HTTP with JSON, both ways (2026-11-14)

Magnus: "Let's pick a well established way of talking between python and .NET. One that is easy to work with for us and
that fits our coding standards. Then we hide that behind an API that we like within jastudio."

- **From JAStudio to Anki:** the Anki add-on runs a minimal HTTP server from Python's standard library (`http.server`),
  on a thread, on the loopback address. Its handlers are today's `anki_facade_backend.py` functions, nearly as they
  are, with the same threading: what drives Anki's windows runs on Qt's main thread, and the rest calls Anki's
  collection on the calling thread. AnkiConnect is the precedent for an add-on that serves HTTP inside Anki.
- **From Anki to JAStudio:** requests to the Kestrel server JAStudio already runs for the `CardServer`.
- **Both run on the same machine:** file paths cross between them, such as that of a media file to add to Anki.
- **Turned down:**
  - *A connection of our own, carrying JSON-RPC both ways,* dialled from the add-on, which Claude first had in mind. It
    would spare Anki a server, and tell each side at once that the other is gone. It costs hand-written framing, reply
    matching and reconnecting in Python, and loses curl and ASP.NET's test host.
  - *gRPC:* its Python package is native, one for each platform and Python version, which is what's being removed.
  - *SignalR and WebSockets:* Python's standard library has no client for them.

### No notifications from Anki: a Sync button in a Studying view (Magnus, 2026-11-14)

Magnus: "we don't need instant notifications on changes from anki. A sync button in a Studying view would be perfectly
sufficient I think."

- **Anki never tells JAStudio that something changed.** The notifications listed above go, and with them what orders
  and holds them: `CatchingUpWithAnki`, and `StudyNoteWrites`' holding of the writes during Anki's own sync.
- **A sync** reads the study-notes from Anki, writes the study-notes that differ from what JAStudio would write now
  (`StudyNoteComparison`), and, for as long as importing from Anki exists, runs the import profiles that run
  automatically.
- **Study-notes are written at a sync only,** and JAStudio reads the study-notes once as it starts.
- **What that costs, none of which Magnus sees as a problem** ("None of that sounds like any real problem to me"):
  - Anki's changes show in JAStudio only after a sync: the studying statuses, Anki's tags, and study-notes deleted in
    Anki.
  - JAStudio's edits reach the study-note only at a sync. A card's page is always current, since it comes from the
    `CardServer`. But until the sync, Anki plays a note's old audio and shows its old question in its own browser, and
    so would a phone, if Anki syncs with AnkiWeb first.
  - A raw note added in Anki is imported at the next sync, not as it arrives.
- **What still goes from Anki to JAStudio** is what Magnus starts himself, and at most one query for each card: the note
  view shortcuts, showing the workspace, and how long to show the question. Each is a plain request, with nothing to
  order or to hold.

### An Anki note of one of JAStudio's own note types never becomes a corpus note (Magnus, 2026-11-15)

A **raw note** is an Anki note of another note type, which an import profile takes (`import-profiles.md`). An Anki note
of one of JAStudio's own note types, the study-notes' types, that holds no corpus note's id is something else. Magnus,
2026-11-15: "This is some sort of data corruption, from the few fields in anki a sane jastudio note cannot be created I
don't think." No sync makes a corpus note of one.

**A sync counts and lists them** (Magnus, 2026-11-15, on Claude's suggestion), so that the corruption shows. Today such
a note is read with no note's id (`StudyNoteInTheSRS.NoteId`), and the collection passes it by without a word
(`JPCollection.GiveEachNoteItsStudyNoteFrom`). It's the check `corpus-validity.md` has wanted since
2026-11-08. Where they're listed is open. Magnus: "Maybe in an errors tab or window?" JAStudio has neither today.

### A call names the collection it expects, and one click takes the collection Anki has open (Magnus, 2026-11-15)

The notifications told JAStudio which Anki profile is open. Without them, a call made with one profile's note ids while
another is open would reach the wrong notes: Magnus has two, `Magnus` and `Anki-Development`.

- **Each call from JAStudio names the collection it expects, and the Anki add-on refuses a mismatch.** It refuses the
  same way while Anki's own sync runs or no profile is open, which it knows from its own hooks.
- **JAStudio then says so, and offers the collection Anki has open instead:** "you should be able to accept using this
  one instead with a single click in jastudio to reload the anki state".

### Most of the Qt menus go (Magnus, 2026-11-14)

"Most of them likely go away. With jastudio a keyboard shortcut away, what good are they?"

- **The reviewer's note menu goes once a shortcut in Anki's reviewer shows the reviewed note in the workspace**
  (Magnus, 2026-11-15, on Claude's suggestion). The menu acts on the note being reviewed, and no shortcut shows that
  note in the workspace today: Ctrl+Shift+O shows the workspace as it was, and a click on a note link opens the linked
  note. Claude proposes the key when the work reaches it.
- **"Prioritize selected cards" and "Spread selected cards" stay for now,** as plain Python menu items in the Anki
  add-on (Magnus, 2026-11-15: "Keep them for now"). They act on the cards selected in Anki's browser, are Python
  already, and need nothing from JAStudio. Whether they stay for good is to be checked with Magnus when the menus go:
  a `todo:` comment in `AnkiBrowserMenuBuilder.BuildBrowserMenuSpec` says so.
- **The rest of the browser's menu goes,** with the other Qt menus.

### Core speaks about studying, and Anki's names stay in `JAStudio.Anki` and on the wire (2026-11-14)

Magnus: "we want to eventually be able to work with outher studying integrations in the end, not bend over backwards
about that until we have one more, but not do anything to get in the way of it, nor call anything that feels generic
Anki rather than something generic about studying." It's the principle `studying.md` set on 2026-11-07.

- **The line between studying's names and Anki's is inside JAStudio,** between Core and `JAStudio.Anki`. The Anki
  add-on and its HTTP contract are frankly Anki's: SELECTs on Anki's collection, Anki's searches. A second
  spaced repetition system brings an integration and a wire of its own.
- **Until increment 1 the seam named it two wrong ways.** `IAnkiStudyNotes`, `IAnkiTagOperations`, `IAnkiWindows` and
  `AnkiAvailability` put Anki's name on what is generic about studying. `IBackendDataLoader`, `BackendData`,
  `ExternalNoteIdMap` and `ICardOperations` said neither Anki nor studying. What they're called now is under
  [Increment 1 as built](#increment-1-as-built-the-names-2026-11-15).

### Importing from Anki is on its way out (Magnus, 2026-11-14)

Anki is two things to JAStudio today: where the student studies, and where raw notes arrive to be imported
(`IAnkiRawNotes`). The second goes. Magnus: "We can already read apkg ourselves. I think this will probably go away."

- **JAStudio reads .apkg files itself** for the reservoir (`AnkiPackageDatabase`). An import profile that takes a deck
  file as it takes Anki's open collection is an idea in `new_ideas_to_discuss.md`.
- **The Immersion Kit import stays for now.** Magnus: "For now I might want to keep the immersion kit import working,
  but with a multi million sentence reservoir coming soon I'm doubtful immersion kit will be seeing any more use from me
  after that."
- **So importing from Anki gets no generic name and no design.** It stays Anki's, in `JAStudio.Anki`, apart from
  studying, and is deleted whole once Immersion Kit is no longer used. With the Sync button it costs a handful of calls
  from JAStudio during a sync: find the raw notes, read them, tag them as imported, delete them.

### Tessaging inside JAStudio is likely, and open (Magnus, 2026-11-14)

The HTTP is hidden "behind an API that we like within jastudio. Likely tessaging." Which spaced repetition system a tommand
is for would be data on the tommand, and its handler talks to the right one. Magnus: "Assuming we do it in tessaging, it
has distinct advantages, and dogfooding it is good practice and and testing and real world API form drivers, but is not
set in stone in any way."

JAStudio doesn't reference Compze.Tessaging today. Claude's view: the move out of Anki doesn't depend on the choice,
since the renamed seam works as interfaces first.

### Several JAStudios connected to one Anki: wanted, and it can wait (Magnus, 2026-11-15)

"Multiple JAStudio instances should be able to connect to anki at the same time. This would greatly help with
development in various scenarios. Not required if it turns out to be a huge pain, but it would be nice."

- **Anki has one primary JAStudio,** the one it sends what goes from Anki to JAStudio: the cards' pages and the rest.
  It's the one that started first of those still running: "Just using the first started one of the one's still
  awailable should be fine."
- **"If this is hard it can wait."**
- **Any connected JAStudio may change what Anki holds** (Magnus, 2026-11-15: "Not sure we need to restrict it"). Each
  JAStudio works on a corpus checkout of its own (`CorpusCheckoutLock`), so two of them can hold different versions of
  the same notes, but: "Usually they will have pretty much the same set of notes, and the fields stored in anki hardly
  ever change in a note. Should any minimal "corruption" ever occur it will be something like an extra note in anki
  that one jastudio does not know about."
  - So with several connected, a study-note whose corpus note one of them doesn't have is normal, and no corruption.
- **What it means for the design (Claude, 2026-11-15, nothing tried):**
  - Calls from JAStudio to Anki need nothing more: the add-on's HTTP server answers whoever calls.
  - For what goes to the primary, the add-on has to know the running JAStudios and when each started. A JAStudio could
    tell it so as it connects, which is still a call from JAStudio, not a notification from Anki.

## What it buys

- **Distribution:** JAStudio can be published self-contained, so a student needs no .NET installed, and nothing of
  .NET is loaded into Anki. The add-on needs no native binary.
- **Multiplatform:** three of the four things `multiplatform.md` lists as not known yet go: whether pythonnet loads
  .NET inside Anki on macOS and Linux, Avalonia's UI thread not being the main thread on macOS, and two GUI toolkits in
  one process on Linux.
- **Development:** the live JAStudio runs from Rider, with its debugger, and restarts on a new build without Anki
  restarting. The JAStudio inside Anki and the standalone JAStudio become one.
- **Deleted:** `JAStudio.PythonInterop`, `JAStudio.Anki.PythonInterop`, `src/runtime_binaries` and what copies it,
  `typings/`, the stub generator, and the stubs' part of `full-build.ps1`.

## What it costs

- **Handing the keyboard to the other program's window:** Ctrl+O in Anki showing the workspace, and JAStudio opening
  Anki's browser. Operating systems restrict it between processes.
- **Two lifecycles:** something has to start JAStudio, and Anki has to do something sensible without it.
- **No type-checked contract across the boundary:** the type stubs go, so something else has to keep the Python and the
  C# in step.
- **The connection can read and delete in Anki's collection,** so it has to be open to the same user only.

Each is taken up under [Deferred until the work reaches them](#deferred-until-the-work-reaches-them) or
[Claude's design calls for the HTTP increment](#claudes-design-calls-for-the-http-increment).

## The order of work

**The preliminary order of work** (Magnus, 2026-11-15, on Claude's proposal of 2026-11-14). Each increment builds clean
and leaves JAStudio usable inside Anki as today, until the fifth.

1. **Names** (built 2026-11-15, see [Increment 1 as built](#increment-1-as-built-the-names-2026-11-15))**:** the seam
   in Core renamed in studying's words, while JAStudio still runs inside Anki on pythonnet. Names and homes first,
   wiring second (the code standard on re-homing concepts before restructuring). Every name Claude chooses gets a
   `//review-naming:studying:` comment for Magnus's pass.
   - *Left alone:* the rest of `studying.md`'s "To design", meaning the domain's words for reading, listening and
     suspending, and the states.
2. **Sync in a Studying view,** replacing the notifications, while still inside Anki. Magnus finds out early whether
   stale-until-sync really doesn't bother him. How it's to be built is proposed under
   [Increment 2](#increment-2-the-sync-in-a-studying-view-proposed-and-partly-built).
3. **Menus:** a shortcut that shows the reviewed note in the workspace, then the Qt menus go.
4. **The HTTP path, built on main in pieces that Anki doesn't use until it's complete:** the add-on's HTTP server,
   `JAStudio.Anki`'s HTTP side, and the standalone JAStudio's wiring. Anki keeps working on pythonnet meanwhile, and
   Magnus sees each piece in Rider. The first piece is built: see
   [Increment 4, its first piece as built](#increment-4-its-first-piece-as-built-the-anki-add-ons-http-server).
5. **One commit changes over:** the Anki add-on stops loading .NET. Magnus tries it on the `Anki-Development` profile
   first.
   - *No switch between the two paths.* Reverting that commit is the way back, which costs a revert, a rebuild and a
     restart, where a switch would cost a setting and a restart. A switch could never run both at once anyway: JAStudio
     inside Anki and the standalone JAStudio can't both hold the main corpus checkout (`CorpusCheckoutLock`).
6. **Once Magnus has studied with it for a while,** pythonnet, `runtime_binaries`, `typings/` and the stub generator
   are deleted.
7. **Starting JAStudio, installing it, publishing:** see [Distribution](#distribution-the-anki-add-on-on-ankiweb-is-the-way-in).

## Increment 1 as built: the names (2026-11-15)

Core's seam to the spaced repetition system is in the new namespace `JAStudio.Core.Studying`, in studying's words.
Every name is Claude's choice, marked `//review-naming:studying:` for Magnus's pass.

**The rule the names follow:** where the seam said Anki, or the backend, it says SRS, for the spaced repetition
system, and an "external note" is the study-note it always was. The names first spelled the term out, as
`ISpacedRepetitionSystemWindows`. Magnus had them take the acronym the same day.

| Was | Is |
|---|---|
| `AnkiAvailability` | `SRSAvailability` |
| `IAnkiWindows`, `NoAnkiWindows` | `ISRSWindows`, `NoSRSWindows` |
| `IBackendDataLoader`, `BackendData` | `ISRSDataLoader`, `SRSData` |
| `ICardOperations` | `ISRSCardOperations` |
| `IAnkiTagOperations` | `ISRSTagOperations` |
| `IAnkiStudyNotes`, `StudyNoteInAnki` | `ISRSStudyNotes`, `StudyNoteInTheSRS` |
| `ExternalNoteIdMap` | `StudyNoteIds` |
| `IExternalNoteUpdateHandler` | `IStudyNoteUpdateHandler`, which goes in increment 2 |

Of these, `ISRSDataLoader`, `SRSData` and `ISRSCardOperations` lasted a day: increment 2 made them part of
`ISRSStudyNotes` and `StudyNoteInTheSRS` (see
[Increment 2](#increment-2-the-sync-in-a-studying-view-proposed-and-partly-built)).

- **A study-note id** is the id the spaced repetition system gives a study-note, in Anki the id of its Anki note. It
  replaced "external id" and "external note id" everywhere: `StudyNoteIds.StudyNoteIdOf`,
  `JPCollection.NoteOfTheStudyNote`, `CardStudyingStatus.StudyNoteId`.
- **The study-notes and their comparison** are in `JAStudio.Core.Studying.StudyNotes`, which was
  `JAStudio.Core.Anki.StudyNotes`.
- **The Anki integration's implementations followed:** `AnkiDataLoader`, which was `AnkiBackendDataLoader`, and
  `AnkiCardOperations`, which was `AnkiCardOperationsImpl`. Both are part of `AnkiStudyNotes` since increment 2.
- **The Anki add-on's Python uses four of the renamed members and two of the types,** and was changed with them.
- **Core's own members that said Anki note for study-note follow.** `JPCollection.ReadTheStudyNotesFromTheSRSAgain` was
  `ReadTheStudyNotesFromAnkiAgain`, `JPNote.HasAStudyNote` was `HasAnAnkiNote`, `JPNote.ForgetItsDeletedStudyNote` was
  `ForgetItsDeletedAnkiNote`, and the enum `ItsStudyNote` was `ItsAnkiNote`. They aren't the seam, but "its Anki note"
  and "its study-note" were two words for one thing.

**What the seam still says of Anki,** each for the increment that changes the wiring:

- *No longer Anki's search syntax (2026-11-15):* `ISRSWindows` opened Anki's browser and previewer on a search in
  Anki's search syntax, which the UI wrote with the Anki integration's `QueryBuilder`. It now takes the ids of the
  study-notes to open on, and the Anki integration writes the search (`AnkisOwnWindows`). `QueryBuilder` is gone.
- **Anki's fields, markup and text:** a study-note's fields are named as Anki names them (`AnkiFieldNames`), its audio
  is in Anki's markup (`AnkiMediaMarkup`), and `StudyNoteComparison` compares text as Anki stores it. The two types
  are all that is left in `JAStudio.Core.Anki`. This goes with the design call that the write of a study-note crosses
  as the fields to write.
- **Cards and suspending:** `ISRSStudyNotes` speaks of both, which `studying.md` is to design
  words for.
- **A study-note id is a `long`,** as Anki's is.

**Not renamed, since they aren't the seam:**

- **The tagging domain's Anki tags:** `AnkiTagChanges`, `AnkiTags` and `Tags.Anki`. They name the branch `anki::` that
  the student sees.
- **The stand-ins for Anki, or for its absence:** `PretendAnki` in the specifications,
  and the hosts' `NoAnki…` and `HeadlessAnki…`.
- **`JPCollection.ReloadFromBackend` and its relatives,** where "the backend" is the corpus checkout and Anki together.

## Increment 2: the Sync in a Studying view, proposed and partly built

**Claude's proposal of 2026-11-15, from reading the code. Magnus hasn't seen it: he gave Claude the go-ahead to
continue by its own judgment, and asked for what it could build on its own overnight.** What is decided about the
Sync is under [Decided](#decided). This says how it gets built while JAStudio still runs inside Anki, in parts that
each leave JAStudio usable. Parts 1 to 3 are built. Part 4 waits for Magnus to have run a sync inside Anki.

**What tells JAStudio of Anki's changes today,** all of it to go:

| The Anki add-on's Python | Calls | Which does |
|---|---|---|
| `follow_the_study_notes_in_anki.py`, on `operation_did_execute` | `AnkiChangedNotes` | reads the study-notes again (`CatchingUpWithAnki`) |
| `clear_studying_cache_on_card_suspend_unsuspend.py` | `JPCollection.UpdateCardStudyingStatus` | sets one card's studying status |
| `anki_collection_sync_runner.py`, on `notes_will_be_deleted` | `IStudyNoteUpdateHandler.StudyNoteRemoved` | forgets a deleted study-note |
| the same, on `note_will_be_added` | `ANoteWasAddedInAnki` | runs the automatic import profiles |
| `announce_finished_imports.py`, on `operation_did_execute` | `AnkiImportedAFile` | runs the automatic import profiles |
| `anki_single_collection_syncer.py` | `StudyNoteWrites.Register` | writes a note's study-note each time the note is updated |

**What stays until JAStudio is a program of its own:** `app.py`'s four hooks, which tell JAStudio that a profile opened
or is closing, and that Anki's own sync starts or has finished (`HandleAnkiLifecycleEvent`). Inside Anki, JAStudio
loads when a profile has opened, and may not use Anki's collection during Anki's sync. A Sync asked for then is refused,
saying why.

**The parts, in order:**

1. **One read of the study-notes, and one interface** (built 2026-11-15, see
   [Part 1 as built](#part-1-as-built-one-interface-for-the-study-notes-with-one-read-of-them)). `ISRSDataLoader`,
   `ISRSStudyNotes` and `ISRSCardOperations` become one interface for the study-notes in the SRS, whose read gives
   each study-note with its note's id, its cards' studying statuses, its tags and its fields. The load and the
   comparison use the same read. No change to what JAStudio does.
2. **The sync, in Core** (built 2026-11-15, see [Part 2 as built](#part-2-as-built-the-sync-in-core))**,** with
   specifications against the pretended Anki. It reads the study-notes, gives each note its study-note, writes the
   study-notes that differ (`StudyNoteComparison`), runs the import profiles that run automatically, and answers with
   what it did: how many study-notes it read and wrote, the notes whose study-note is gone, and the Anki notes of
   JAStudio's note types that hold no corpus note's id. JAStudio's MCP server runs the same sync, with a tool of its
   own, `sync_with_anki`.
3. **The Studying view** (built 2026-11-15, see [Part 3 as built](#part-3-as-built-the-studying-window))**:** a
   window, opened from the workspace's main menu and by a shortcut, with the Sync button, what
   the last sync did, and the list of the Anki notes that hold no corpus note's id. Magnus suggested "an errors tab or
   window" for that list: here it is part of the Studying window.
4. **The notifications go:** the six rows of the table above, with `CatchingUpWithAnki.AnkiChangedNotes`,
   `StudyNoteWrites`, `IStudyNoteUpdateHandler` and `JPCollection.UpdateCardStudyingStatus`. From then on Anki's
   changes show in JAStudio after a Sync, and JAStudio's edits reach the study-notes at a Sync. This is the part where
   Magnus finds out whether stale-until-sync bothers him, so it comes last and in a commit of its own, which a revert
   takes back.

**What JAStudio's own changes do, unchanged:** suspending and unsuspending from JAStudio set the note's studying
statuses themselves (`JPNote.SetTheStatusOfEveryKnownCard`), and a change to Anki tags reads the study-notes again
itself (`AnkiTagChanges`). Neither waits for a Sync.

**For Magnus to decide, or to leave to Claude.** Both are built as proposed, for him to change:

- **Where the Studying view is.** Proposed: a window of its own. The alternative is a pane or a tab in the workspace.
- **Whether the workspace shows that a Sync is due,** such as how many notes were updated since the last one. Proposed:
  not in this increment.

### Part 1 as built: one interface for the study-notes, with one read of them

- **`ISRSStudyNotes`** is the study-notes in the spaced repetition system: `ReadAll`, `Write`,
  `SuspendAllCardsForNote`, `UnsuspendAllCardsForNote` and `DeleteTheStudyNoteOf`. It took in `ISRSDataLoader`, whose
  load read the same study-notes without their fields, and `ISRSCardOperations`, which changed them.
- **`StudyNoteInTheSRS`** is one study-note as it's read: its id, the id of the note it belongs to (`NoteId`, null
  when the study-note holds no note's id), its fields, its tags (`Tags`) and the studying status of each of its
  cards (`CardStudyingStatuses`). `SRSData`, which held the three lists the load read, is gone.
- **The collection reads them itself, and shows the progress of the read** (`JPCollection.LoadFromBackend`), where
  the loader was handed the task runner.
- **Each host has one class for them:** `AnkiStudyNotes` inside Anki, `HeadlessAnkiStudyNotes`, which reads and
  refuses changes, and the standalone JAStudio's `NoAnkiStudyNotes`. In the Anki integration,
  `StudyNotesInAnkisDatabase` makes the one read, with a SELECT each of the note types, the study-notes and the
  cards.
- **The pretended Anki of the specifications holds whole study-notes** (`PretendAnki`), and took in
  `InMemoryAnkiStudyNotes`: giving a note a study-note writes its fields, as creating it in Anki does, and JAStudio
  deleting a note's study-note deletes it there.
- **What the one read costs, measured on Magnus's collection file on 2026-11-15,** 79,719 study-notes and 159,463
  cards: the SELECT of the study-notes with every field takes about as long as the load's SELECT of one field did
  (61 ms and 94 ms in Python's SQLite, where the function that picks one field is slow), and gives 17.5 million
  characters of JSON where the load's gave 11.0 million. The study-notes' fields are small: 8.6 million characters
  in all. Not measured: reading the larger JSON into rows, and its way through pythonnet inside Anki.
- **Until part 4, the larger read also runs after every Anki operation that changed notes**
  (`JPCollection.ReadTheStudyNotesFromTheSRSAgain`), in the background.

### Part 2 as built: the sync, in Core

- **`SRSSync.Run`** is the sync, in `JAStudio.Core.Studying`. One sync runs at a time. It answers with an
  `SRSSyncReport`: how many study-notes it read, the notes whose study-note had changed in the spaced repetition
  system since JAStudio last read it, the notes whose study-note is gone, the notes whose study-note it wrote, and the
  study-notes of no note.
- **The study-notes of no note are of two kinds** (`StudyNoteComparison`), which were one list until now:
  - *Those that hold no note's id* (`StudyNotesThatHoldNoNotesId`): the corruption Magnus wanted counted and listed.
  - *Those that hold the id of a note the collection hasn't* (`StudyNotesWhoseNoteTheCollectionHasNot`), such as a
    note deleted with its study-note kept. With several JAStudios connected to one Anki, they're normal.
- **The collection reads and compares in one step for a sync**
  (`JPCollection.ReadTheStudyNotesFromTheSRSAgainAndCompareThem`), so that a load can't empty the collection between
  the two. A study-note is compared with the note whose id it holds.
- **A sync is refused while it can't run** (`SRSSync.WhyItCannotRunNow`), in words for a person:
  - *while what the spaced repetition system holds can't be used* (`SRSAvailability.WhyItCannotBeUsedNow`): inside
    Anki, while no Anki profile is open or Anki's own sync is under way. `AnkisAvailability`, in the Anki
    integration, takes in Anki's four lifecycle events, which `CatchingUpWithAnki` kept to itself until now. It's what
    is left to do with them once the notifications go;
  - *before the collection has loaded.*
- **The import profiles that run automatically are started, not waited for** (`AutomaticImportRuns.AfterASyncWithTheSRS`).
  They import in the background, and report as they do today, so what they import isn't in the sync's report.
- **The sync shows its progress** as the load does, through the task runner.
- **JAStudio's MCP server has a tool for it, `sync_with_anki`,** instead of `write_the_study_notes_that_differ`
  running it, as first proposed: that tool writes the study-notes it's asked to, and would have started imports
  besides. With `sync_with_anki`, Claude can run a sync in the live JAStudio without the Studying window.
- **Nothing starts a sync yet but that tool.** The notifications still do what a sync does, piece by piece, so a sync
  finds little to do until part 4.

### Part 3 as built: the Studying window

- **The Studying window** (`StudyingWindow`, `StudyingWindowViewModel`) has the Sync button, a line for each thing the
  last sync did, and the list of the study-notes in Anki that belong to no note, each with why: it holds no corpus
  note's id, or JAStudio has no note with the id it holds. A button opens Anki's browser on them.
- **It opens from the workspace's main menu,** Studying > Studying window, and with Ctrl+Shift+Y. A JAStudio has one
  at most: opening it again brings it to the front. The menu's place and the key are Claude's choice, marked
  `//review:` in `WorkspaceMainMenu`. It's offered only while there is a spaced repetition system.
- **It shows the last sync whoever started it,** such as JAStudio's MCP server, and when it opens: the sync keeps its
  last report (`SRSSync.LastReport`) and notifies each sync (`SRSSync.Completed`). A report says when the sync
  finished.
- **A sync that can't run says why in the window,** such as "The sync can't run now: Anki is syncing."
- **The sync runs in the background,** with the task runner's progress, as the load shows it.
- **Not in the window:** which notes had changed, were written or lost their study-note. It gives the counts.
  JAStudio's MCP server's `sync_with_anki` lists them.
- **Specified by opening it,** on Avalonia's headless platform, pressing Sync, and rendering it before and after.
- **Not tried inside Anki.** Claude can't restart Anki. Until part 4 the notifications still keep JAStudio and Anki
  in step, so the first syncs there should find little to do: they're the check that a sync does no harm.

## Increment 4, its first piece as built: the Anki add-on's HTTP server

**Built 2026-11-15 on Claude's design calls below, which Magnus hasn't commented on. Nothing starts the server yet:
Anki works on pythonnet as before.** It's the Python package `jastudio.anki_http`, in the Anki add-on.

- **`HttpServerForJAStudio`** serves on the loopback address, on a port the system picks, with Python's standard
  library alone (`http.server`), on a thread of its own and a thread for each request.
  - `GET /contract` gives the contract's version, 1.
  - `GET /collection` gives the path of the collection Anki has open.
  - `POST /call/<function>?collection=<path>` calls a function, with a JSON object of its arguments by name as the
    body.
- **Answers are JSON objects:** `{"result": …}` with status 200, `{"refused": why}` with 409, in words for a person,
  and `{"error": …}` otherwise: 401 without the token, 404 for a function there is none of, 400 for arguments that
  don't fit, and 500, with the traceback, for a function that failed.
- **The functions are those of `anki_facade_backend.py` that work on Anki's collection,** by the same names
  (`FunctionsOnTheCollection`): `notes_find`, `notes_add_tag`, `notes_remove`, the five `tags_…`, `study_note_write`,
  `note_suspend_all_cards`, `note_unsuspend_all_cards`, `note_remove`, `col_read_json`, `get_note_id_from_card_id`,
  `media_add_file` and `anki_media_dir`.
  - *`study_note_write` takes the fields to write,* by name, where pythonnet passed the note.
  - *`col_read_json` answers with the JSON the SELECT built, as JSON,* not as a string holding it: the read of the
    study-notes is 17.5 million characters.
- **Every request has to carry the server's token** (`Authorization: Bearer …`), a new one each time the server is
  made. How JAStudio gets the port and the token isn't built: a file only the same user can read is the plan.
- **A call names the collection JAStudio expects, by its path, and is refused when Anki has another open,** with the
  path of the one it has. It's in the URL's query, since a path can hold characters a header can't.
- **A call is refused, saying why, while JAStudio may not use Anki's collection** (`AnkiForJAStudio`): no profile
  open, or Anki's own sync under way. The contract is served all the same.
- **It needs neither .NET nor Anki's window.** `AnkiForJAStudio` is all it knows of Anki: the open collection,
  whether JAStudio may use it, and telling Anki's windows of an operation. Inside Anki that will be Anki's main window.
- **Specified without Anki's window** (`src/tests/jastudio_tests/anki_http`): each function, and each refusal, called
  over HTTP on a copy of the test collection opened with Anki's own library. The repository had no such test.

**Not built:**

- *The functions that drive Anki's windows:* opening its browser and previewer, its tooltip, and refreshing its
  windows. They need Anki's main thread and window.
- *`AnkiForJAStudio` for the real Anki,* and starting the server as a profile opens.
- *How JAStudio finds the server:* its port and its token.
- *JAStudio's side:* a seam for `AnkiFacade`, which is a static class, its implementation over HTTP, and the
  specification that compares the Python functions with the C# contract.

## Distribution: the Anki add-on on AnkiWeb is the way in

**The direction for the increment on starting JAStudio, installing it and publishing** (Magnus, 2026-11-15, on Claude's
suggestion).

**The Anki add-on has to be in AnkiWeb's list of add-ons.** Magnus: "I figure that no matter how silly it might seem,
not going through the standard distribution channel would tank usage. Once one has installed the addon that looks so
cool, the odds that one clicks the "Download and Install" button are very good. If one just hears about some program
somewhere you should install that will integrate with anki.... Odds plummet I would say".

- **The Anki add-on is the way in.** A student installs it from AnkiWeb, as any add-on. When it finds no JAStudio, it
  offers to download and install the release it goes with, and it starts it.
- **A contract version keeps the two in step.** The HTTP contract has a version, which changes when the contract does,
  and which the two compare when they first talk. Several JAStudios built from different checkouts then all work with
  one add-on, as long as their contract version is the add-on's.

**Turned down:** that JAStudio installs its Anki add-on into Anki, which Claude first suggested. It would keep JAStudio
out of AnkiWeb's list. And with several JAStudios connected to one Anki, each would install its own add-on over the
others'.

**Not known:** what AnkiWeb lets an add-on download and run, and whether it limits an add-on's size. Anki's page on
sharing add-ons says nothing of either (read 2026-11-15).

**If handing over the keyboard takes uiAccess,** an install started by the add-on needs elevation, since JAStudio then
has to be in a protected folder (Magnus, 2026-11-15; see
[Deferred until the work reaches them](#deferred-until-the-work-reaches-them)).

## Claude's design calls for the HTTP increment

For Magnus to see, and to change if he doesn't share them. Nothing is tried.

- **A version check when the two first talk:** an add-on and a JAStudio whose contracts differ refuse each other,
  saying so. What the version is of is under [Distribution](#distribution-the-anki-add-on-on-ankiweb-is-the-way-in).
  *The add-on's side serves its version (2026-11-15).*
- **A specification that compares the add-on's Python functions with the C# contract,** in place of the type stubs.
- **The connection is open to the same user only,** such as by a token that only the user's files hold. *The server
  demands a token (2026-11-15). Where JAStudio reads it isn't built.*
- **The write of a study-note crosses as the fields to write,** not as a note. *So it does (2026-11-15).*
- **The add-on's handlers get specifications that run without Anki's window,** against a collection file opened with
  Anki's own library. *They have them (2026-11-15), for the functions that work on the collection.*

## Deferred until the work reaches them

- **Handing the keyboard between the two programs.**
  - On Windows, the program that has the keyboard can let another take it (`AllowSetForegroundWindow`). Untried.
  - Magnus, 2026-11-15: "If we find nothing better, we can sign jastudio and run it with uiAccess, then it can forground
    itself and anki." Windows grants uiAccess only to a signed program installed in a protected folder, such as Program
    Files, so installing JAStudio would then take elevation: "If not installed in program files uiAccess would not
    work."
  - macOS and Linux: not looked into.
- **What Anki's reviewer shows when JAStudio isn't running.** Most likely Anki's own card templates, which a phone
  needs anyway (`web-rendering-architecture.md`, "Card Templates Installed in Anki").
- **How long to show a question, and the settings the scheduling hooks read:** asked of JAStudio for each card, or sent
  to the add-on when they change.
- **How a student relates to an Anki profile** (`studying.md`, "To design", 5).
- **Tessaging or interfaces inside JAStudio.**
- **With several JAStudios connected:**
  - How the add-on learns that one stopped without saying so.
  - Which JAStudio's sync imports, for as long as importing from Anki exists. An import deletes its raw notes from
    Anki, so they'd end up in the corpus checkout of whichever JAStudio syncs first. Claude's suggestion: only the
    primary's sync imports.
- **Who starts JAStudio,** and what the add-on does until it runs: see
  [Distribution](#distribution-the-anki-add-on-on-ankiweb-is-the-way-in).

## Also turned down

- **Loading JAStudio's assemblies into an `AssemblyLoadContext` that can be unloaded,** the smaller way that
  `speeding-up-development.md` asks about. It would spare the restart of Anki, and nothing else: the native binaries,
  the .NET runtime on the student's machine and the unknowns on macOS and Linux would all stay.
- **A documented Compze.Tessaging wire protocol that a peer not written in .NET can speak,** which Claude recommended
  and withdrew. Magnus, 2026-11-14: "At the stage tessaging is at I'm not sure we want to promise anything about any
  sort of stable formats."
