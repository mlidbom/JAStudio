# Moving JAStudio out of Anki

> **Status: direction agreed with Magnus on 2026-11-14 and 2026-11-15; increment 1, the names, is built, and so is
> increment 2, the Sync: since 2026-11-15 Anki no longer tells JAStudio of what changes, and the workspace has a Sync
> button. Of increment 4, the HTTP path, the Anki add-on's HTTP server is built, and so is JAStudio's implementation
> of its calls over HTTP, which only its specifications use yet, and the access file, by which a JAStudio finds the
> add-on's server. On JAStudio's side, the UI's direct calls to
> `AnkiFacade` are behind Core's seams, but for the menu commands and the menu of Anki's browser, and the Anki
> integration calls Anki through interfaces, which pythonnet implements inside Anki.** The order of work is
> preliminary. Details are settled as each increment needs them. The names are Claude's, and wait for
> Magnus's pass: see [Increment 1 as built](#increment-1-as-built-the-names-2026-11-15).

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

From reading the code on 2026-11-14, before increment 2: of the notifications listed here, only a profile opening or
closing and Anki's own sync starting or finishing are left. Today pythonnet loads JAStudio's .NET side into Anki's process
(`load_dotnet_runtime.py`), from `src/runtime_binaries`, 190 MB.

- **The Python layer is about 2,700 lines.**
- **From .NET into Anki: 23 functions** (`AnkiThroughPythonnet` and what is left of `AnkiFacade`, backed by
  `anki_facade_backend.py`). They take and return ids, strings and lists of them. The one exception took a note, to
  write its study-note from, until 2026-11-15: it now takes the fields to write.
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
  `ISRSWindows`, `ISRSStudyNotes`, `ISRSTagOperations`, `IAnkiRawNotes` and `AnkiCollectionDatabase`, and since
  2026-11-15 `ISRSMedia`.

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

**For the move out of Anki it's interfaces** (Magnus, 2026-11-15, on Claude's recommendation: "Interfaces should work
fine for this."). Tommands can replace them later, one group of functions at a time.

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
2. **Sync in a Studying view** (built 2026-11-15, see [Increment 2](#increment-2-the-sync-as-built))**,** replacing
   the notifications, while still inside Anki. Magnus finds out early whether stale-until-sync really doesn't bother
   him.
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
[Increment 2](#increment-2-the-sync-as-built)).

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

## Increment 2: the Sync, as built

**Built on 2026-11-15, in four parts that each left JAStudio usable inside Anki, as Claude proposed that day from
reading the code.** Magnus had given Claude the go-ahead to continue by its own judgment, and asked for what it could
build on its own overnight: parts 1 to 3. He then ran a sync inside Anki, which "runs fine": it read 79,719
study-notes, wrote none, and found none that belongs to no note. On that he asked for the Sync button in the
workspace, and said yes to part 4. What is decided about the Sync is under [Decided](#decided).

**What told JAStudio of Anki's changes until part 4,** all of it gone:

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
4. **The notifications go** (built 2026-11-15, see
   [Part 4 as built](#part-4-as-built-ankis-notifications-are-gone))**:** the six rows of the table above, with
   `CatchingUpWithAnki.AnkiChangedNotes`, `StudyNoteWrites`, `IStudyNoteUpdateHandler` and
   `JPCollection.UpdateCardStudyingStatus`. From then on Anki's changes show in JAStudio after a Sync, and JAStudio's
   edits reach the study-notes at a Sync. This is the part where Magnus finds out whether stale-until-sync bothers
   him, so it came last and in a commit of its own, which a revert takes back.

**What JAStudio's own changes do, unchanged:** suspending and unsuspending from JAStudio set the note's studying
statuses themselves (`JPNote.SetTheStatusOfEveryKnownCard`), and a change to Anki tags reads the study-notes again
itself (`AnkiTagChanges`). Neither waits for a Sync.

**Where the Sync is, decided by Magnus on 2026-11-15,** when he had seen the Studying window: "I don't want sync several
clicks and a dialog away. Just a sync button. Click it and it spins and done, green marked sync button. Maybe right
click or something if you want the complex UI, or a separate UI item..." So the workspace has a Sync button of its
own: see [The Sync button as built](#the-sync-button-as-built). Claude had proposed a window of its own for the
Studying view, and nothing in the workspace to show that a Sync is due.

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
- **Until part 4, the larger read also ran after every Anki operation that changed notes**
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
  Magnus confirmed that a Sync runs them (2026-11-15), after first saying "No auto creation or auto import": "Yes. I
  overstated that." What stays manual is creating study-notes: JAStudio never creates one.
- **The sync opens no progress window** (since the Sync button, 2026-11-15): whoever started the sync shows that it's
  under way, as the Sync button does, and no window opens over what the person does. Claude first had it open one for
  a write of more than 200 study-notes. Magnus, the same day: "I probably still don't want it popping up
  automatically. Just spinning is fine. If I lose patience I can right click it. And this will hardly ever happen.
  Only sound and question is written and they hardly ever change.." Until the Sync button it showed its progress as
  the load does, through the task runner.
- **The sync says whether one is under way** (`SRSSync.IsRunning`, and `IsRunningChanged` to follow it), whoever
  started it.
- **JAStudio's MCP server has a tool for it, `sync_with_anki`,** instead of `write_the_study_notes_that_differ`
  running it, as first proposed: that tool writes the study-notes it's asked to, and would have started imports
  besides. With `sync_with_anki`, Claude can run a sync in the live JAStudio without the Studying window.
- **Until the Studying window, nothing started a sync but that tool,** and until part 4 the notifications still did
  what a sync does, piece by piece, so a sync found little to do.

### Part 3 as built: the Studying window

- **The Studying window** (`StudyingWindow`, `StudyingWindowViewModel`) has a Sync button, a line for each thing the
  last sync did, and the list of the study-notes in Anki that belong to no note, each with why: it holds no corpus
  note's id, or JAStudio has no note with the id it holds. A button opens Anki's browser on them. It's the "complex
  UI" of Magnus's words: the Sync itself is one click in the workspace.
- **It opens from the workspace's main menu,** Studying > Studying window, and with Ctrl+Shift+Y. A JAStudio has one
  at most: opening it again brings it to the front. The menu's place and the key are Claude's choice, marked
  `//review:` in `WorkspaceMainMenu`. It's offered only while there is a spaced repetition system.
- **It shows the last sync whoever started it,** such as JAStudio's MCP server, and when it opens: the sync keeps its
  last report (`SRSSync.LastReport`) and notifies each sync (`SRSSync.Completed`). A report says when the sync
  finished.
- **A sync that can't run says why beside the Sync button,** such as "The sync can't run now: Anki is syncing."
- **The sync runs in the background.**
- **Not in the window:** which notes had changed, were written or lost their study-note. It gives the counts.
  JAStudio's MCP server's `sync_with_anki` lists them.
- **Specified by opening it,** on Avalonia's headless platform, pressing Sync, and rendering it before and after.
- **Tried inside Anki by Magnus on 2026-11-15:** "The sync runs fine". It read 79,719 study-notes, wrote none, and
  found no note whose study-note had changed or was gone, and no study-note that belongs to no note. The
  notifications still kept JAStudio and Anki in step then, so a sync there found little to do.
- **The names of increment 2 wait for Magnus's pass, which he deferred** (2026-11-15: "Defer this. Keep your names and
  the comments for now."): those marked `review-naming:studying:` since increment 2, among them `SRSSync`,
  `SRSSyncReport`, `StudyNotesReadFromTheSRS` and `AnkisAvailability`, and the `//review:` in `WorkspaceMainMenu` on
  the menu's place for the Studying window and its key, Ctrl+Shift+Y.

### The Sync button as built

**Built 2026-11-15, the day Magnus asked for it:** "Just a sync button. Click it and it spins and done, green marked
sync button."

- **The workspace window has a Sync button,** at the right end of the main menu's row (`SyncButton`,
  `SyncButtonViewModel`). One click runs a sync in the background. It shows only while there is a spaced repetition
  system.
- **Its icon is two arrows in a circle, which turn while a sync is under way,** whoever started it: this button, the
  Studying window's, or JAStudio's MCP server. The button starts no other sync meanwhile.
- **Once the sync has finished, the icon is a green check mark, and the button's border is green.** They stay so
  until the next sync starts. A sync that fails leaves the button unmarked.
  - *The mark says that the last sync finished, not that nothing has changed since.* JAStudio isn't told of what
    changes in Anki, and its own edits reach the study-notes at the next sync.
- **No progress window opens,** however long the sync takes.
- **A sync that can't run says why beside the button,** as in the Studying window.
- **A right-click opens the Studying window,** as the keyboard's menu key does. The button's tooltip says so, with
  what a sync does, and when the last one finished and how many study-notes it read and wrote.
- **The Studying window's Sync button is the same button.**
- **Seen only in the Studying window so far.** Claude can't open the workspace window in a specification, and the
  headless JAStudio has no spaced repetition system, so its workspace shows no Sync button.

### Part 4 as built: Anki's notifications are gone

**Built 2026-11-15, on Magnus's yes the same day, in a commit of its own, which a revert takes back. Not tried inside
Anki when this was written: Claude can't restart Anki.**

- **Anki tells JAStudio of four things only:** that a profile opened or is closing, and that Anki's own sync is
  starting or has finished (`app.py`, `JAStudioAnkiAppRoot.HandleAnkiLifecycleEvent`).
- **Gone from the Anki add-on's Python:** `follow_the_study_notes_in_anki.py`,
  `clear_studying_cache_on_card_suspend_unsuspend.py` with `studing_status_helper.py`, `announce_finished_imports.py`,
  and the package `note/collection`, whose synchronizer followed the notes added and deleted in Anki and wrote a
  note's study-note each time the note was updated. About 300 lines.
- **Gone from .NET:** `StudyNoteWrites`, `IStudyNoteUpdateHandler`, `JPCollection.UpdateCardStudyingStatus`,
  `JPNote.ForgetItsDeletedStudyNote`, `AutomaticImportRuns.AfterAnkiImportedAFile` and `AfterANoteWasAddedInAnki`,
  and what Python called on `JAStudioAnkiAppRoot`: `AnkiChangedNotes`, `AnkiImportedAFile`, `ANoteWasAddedInAnki`
  and `StudyNoteWrites`.
- **`FollowingAnkisLifecycle` is what is left of `CatchingUpWithAnki`.** It tells `AnkisAvailability` of each of the
  four, and reads the profile that opened: the first profile to open has JAStudio load its collection, and each one
  after that has it read the study-notes again, since they're that profile's.
  - *Anki syncs as a profile opens, when set to,* and JAStudio may not use Anki's collection meanwhile. The profile
    is then read once that sync has finished. Until part 4 the operation Anki announces a sync's end with did that.
  - *It's in `JAStudio.Anki`, and specified* (`When_following_Ankis_lifecycle`), since inside Anki nothing else loads
    JAStudio's collection, and Claude can't try it there. `CatchingUpWithAnki` was in `JAStudio.Anki.PythonInterop`,
    which no specification reaches, and had none.
  - *Specifying it found a deadlock in `BackgroundTaskManager`,* fixed the same day: disposing it held its lock while
    it waited for the running tasks, so a task that started another one meanwhile waited for that lock, and both
    stood still until the lock timed out after two minutes. Closing Anki within half a second of a profile opening
    would have done it.
- **What it means in use,** as decided:
  - *What changes in Anki shows in JAStudio after a Sync:* a card suspended or unsuspended, a tag, a study-note
    deleted, and whatever Anki's own sync brought from AnkiWeb.
  - *What JAStudio changes in a note reaches its study-note at a Sync.* Until then Anki plays the note's old audio
    and shows its old question in its browser. A card's page is current, since it comes from the `CardServer`.
  - *Raw notes are imported at a Sync,* by the import profiles that run automatically, not as they arrive.
- **Unchanged, and needing no Sync:** suspending and unsuspending from JAStudio, and JAStudio's changes to Anki's
  tags, after which it reads the study-notes again itself (`AnkiTagChanges`).
- **Claude expects, and hasn't tried:** that an action in JAStudio on a note whose study-note was deleted in Anki
  since the last Sync, such as suspending its cards, fails with Anki's error, since JAStudio still holds the id of an
  Anki note that is gone.
- **The specifications that used the notifications now use the sync or a read of the study-notes:**
  `When_a_vocab_is_deleted_in_Anki`, `When_import_profiles_run_automatically`, where the run on a note added in Anki
  went, and `When_keeping_the_results_of_a_search_up_to_date`.

## Increment 4, its first piece as built: the Anki add-on's HTTP server

**Built 2026-11-15 on Claude's design calls below, which Magnus found "OK for now" the same day. Nothing starts the
server yet: Anki works on pythonnet as before.** It's the Python package `jastudio.anki_http`, in the Anki add-on.

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
- **It answers one request a connection, and says so** (`Connection: close`, since 2026-11-15). Until it did, .NET's
  `HttpClient` kept each connection for its next request, found it closed, and failed about one call in ten with
  "The response ended prematurely". JAStudio's specifications over HTTP found it.

**Not built:**

- *The functions that drive Anki's windows:* opening its browser and previewer, its tooltip, and refreshing its
  windows. They need Anki's main thread and window.
- *`AnkiForJAStudio` for the real Anki,* and starting the server as a profile opens.
- *How JAStudio finds the server:* its port and its token.
- *JAStudio's side,* until 2026-11-15: a seam for `AnkiFacade`, which was a static class, and its implementation
  over HTTP. The seam is interfaces (Magnus, 2026-11-15: "Interfaces should work fine for this. Go with your
  preference."), one for each group of functions. Both are built since: see the three steps below. What JAStudio's
  side still lacks is under step 3.

**Claude's plan for JAStudio's side, from reading `AnkiFacade`'s callers on 2026-11-15. Its first step is built, but
for the menu commands, and so is its second: see [step 1](#jastudios-side-step-1-as-built-the-uis-direct-calls) and
[step 2](#jastudios-side-step-2-as-built-ankis-functions-as-interfaces) below.**

- **Most of `AnkiFacade`'s 24 functions are already called from behind Core's seams,** by the classes of
  `JAStudio.Anki` that implement them: `AnkiStudyNotes` (`ISRSStudyNotes`), `AnkiTagOperations`
  (`ISRSTagOperations`), `AnkisOwnWindows` (`ISRSWindows`), `AnkiRawNotes` (`IAnkiRawNotes`), `AnkisOwnConnection`
  (`AnkiCollectionDatabase`) and `AnkiEnvironmentPaths`. Those classes call the static `AnkiFacade`.
- **So the seam goes inside `JAStudio.Anki`:** an interface for each group of Anki's functions, which those classes
  are given, with one implementation over pythonnet, which is today's `AnkiFacade` as objects, and one over HTTP.
  The groups, as `AnkiFacade` and the add-on's `FunctionsOnTheCollection` have them: Anki's notes and their cards,
  its tags, its media, its database, and its windows.
- **Four places in `JAStudio.UI` call `AnkiFacade` directly,** past Core's seams, and go behind them first:
  - `MenuItem`, `BrowserMenus` and `AutomaticImportRunReports` refresh Anki's windows and show its passing message:
    `ISRSWindows` has both.
  - `UsingASpeech` copies a file into Anki's media folder (`AnkiFacade.Media.AddFile`): Core has no seam for that.
  - `BrowserMenus` also asks for a card's note, and prioritizes and spreads cards. It builds the menus of Anki's
    browser, which go in increment 3.
- **The order:** the UI's direct calls go behind Core's seams; then the interfaces in `JAStudio.Anki`, with the
  pythonnet implementation, which changes nothing in what JAStudio does; then the HTTP implementation, specified
  against the add-on's HTTP server running on a copy of the test collection, started from the specifications.

### JAStudio's side, step 1 as built: the UI's direct calls

**Built 2026-11-15, but for the menu commands. Nothing changes in what JAStudio does.**

- **`ISRSMedia`, a new seam in Core,** is the spaced repetition system's own copies of the files its study-notes
  reference. JAStudio gives it a copy of an audio file it makes (`Add`).
  - `AnkisMediaFolder`, in the Anki integration, is Anki's media folder, through Anki's own `col.media.add_file`.
  - `NoSRSMedia` is for a JAStudio without a spaced repetition system: the standalone and the headless JAStudio.
  - `PretendAnki` keeps the copies in the specifications, which now say that Anki was given the file.
- **`UsingASpeech` gives the TTS audio to `ISRSMedia`.** It asked whether Python runs, and called `AnkiFacade`.
- **`AutomaticImportRunReports`, and the reparse in `AnkiBrowserMenuBuilder`, use `ISRSWindows`.**
  `ShowAPassingMessage` takes how long to show the message, since the import's message shows for 5 seconds where
  the others show for 3.

**Left in `JAStudio.UI`,** which therefore still references `JAStudio.Anki`:

- *`AnkiBrowserMenuBuilder`'s calls for cards:* a card's note, and prioritizing and spreading cards. They go with the
  Qt menus, in increment 3.
- *`SpecMenuItem.UICommand`:* open, below.

#### Open: how the menu commands refresh the spaced repetition system's windows

**Put to Magnus on 2026-11-15, who turned both options below down:** "I thought we agreed on sync only... Why do we
need any of this unless we clicked the sync button? And on the sync button. if we save notes via anki's normal API's
so that undo etc works, as I think we do, then anki will refresh what it needs itself, I'm 80% sure. So sum total:
virtually all of that is just removed. If just running the sync surfaces any problems we can consider refreshing
Anki's ui at the end of sync, but the way we currently refresh Ankis UI is Not Recommended(TM) by the anki devs...."

**What reading the code found, the same day:**

- **The card in Anki's reviewer and previewer is JAStudio's own page,** in an iframe on the `CardServer`, drawn from
  the corpus note, not from the study-note. It changes when the note does, with no sync, and nothing tells it to
  draw again: the refresh after a menu command is what redraws the card under review today.
- **The refresh pokes Anki's windows through private members** (`UIUtils.refresh` in the add-on): it sets the
  reviewer's `_refresh_needed`, the previewer's `_last_state`, and runs the browser's search again.
- **Anki's windows follow an operation only when they're told of it** (`aqt.operations.on_op_finished`), as Anki's
  own windows are of theirs. `col.update_note` alone tells none. JAStudio's tag functions tell Anki; its write of a
  study-note, suspending, unsuspending and deleting don't.
- **While the collection loads, a card says that JAStudio is still loading** (`AppendingPrerenderer`), which isn't
  JAStudio's page: only Anki can replace it with the card, so the refresh after a load has a use of its own.

**Not settled:** a change that removes the refreshes, and has a card's page reload itself when its note changes,
is in Claude's working tree, uncommitted, while Magnus has doubts about it (2026-11-15).

**The two options Magnus turned down:**

57 places in the UI make a menu command with `SpecMenuItem.UICommand`, and 35 more through `NoteCommandMenuItem`,
which uses it. It runs the command, and then refreshes Anki's reviewer, previewer and browser, by calling
`AnkiFacade` when Python runs. It's a static factory, and most of its callers are static menu builders that have no
services, so it can't ask for `ISRSWindows`. Five other places in the UI call `ISRSWindows.Refresh` themselves,
after a change of their own.

1. **Claude's recommendation: Anki's windows refresh when a note or the configuration changes,** as JAStudio's own
   note pages do (`NoteTabViewModel`). The Anki integration listens to the collection, and refreshes once for a burst
   of changes. `UICommand` becomes `Command`, and the five explicit refreshes go.
   - *It changes what JAStudio does.* Anki's windows then also refresh after an edit in the workspace's editors,
     which Claude finds no refresh for today, and after changes made in the background, such as a reparse or an
     import. A card under review would be drawn again when its note changes, whoever changed it.
   - *It fits the move out of Anki:* a refresh is then one call over HTTP from one place, not one from each command.
2. **Every menu builder is given `ISRSWindows`, and passes it to `UICommand`.** About 90 call sites change, and the
   signatures of the static menu builders that lead to them, and nothing in what JAStudio does.

### JAStudio's side, step 2 as built: Anki's functions as interfaces

**Built 2026-11-15. Nothing changes in what JAStudio does.**

**Tried inside Anki on 2026-11-15,** through JAStudio's MCP server: the read of every study-note, and the write of
one, both ways, with its Japanese intact. The calls for tags, suspending, deleting and media weren't exercised
there.

- **Five interfaces in `JAStudio.Anki.AnkisFunctions`,** one for each group of the functions JAStudio calls in Anki:
  `IAnkisNotes`, `IAnkisTags`, `IAnkisMedia`, `IAnkisDatabase` and `IAnkisWindows`. Each member is one function of
  the Anki add-on, which its doc comment names, such as `notes_find`.
- **`AnkiThroughPythonnet` implements all five,** each as one of its properties: it's what `AnkiFacade` was, as
  objects. It also answers for the add-on's own folder, which isn't one of Anki's functions.
- **The Anki integration's classes are given the group they call:** `AnkiStudyNotes`, `AnkiRawNotes`,
  `AnkiTagOperations`, `AnkisOwnWindows`, `AnkisMediaFolder`, `AnkisOwnConnection` and `AnkiEnvironmentPaths`. The
  add-on's registrar makes one `AnkiThroughPythonnet` and hands its groups out.
- **A study-note is written as its fields,** as over HTTP: `IAnkisNotes.WriteTheFields` takes the fields JAStudio
  writes (`JPNote.StudyNoteFields`), and Python's `study_note_write` writes them as its HTTP twin does. Python read
  them from the .NET note before (`JPNoteDataShim`, deleted).
- **`AnkiFacade` keeps three calls,** which `JAStudio.UI` still makes itself: the refresh after a menu command, and the
  two of the menu of Anki's browser. See step 1's "Left in `JAStudio.UI`".
- **Not in the interfaces:** the add-on's `get_note_id_from_card_id`, which only the menu of Anki's browser calls.

### JAStudio's side, step 3 as built: Anki's functions over HTTP

**Built 2026-11-15. Nothing uses it yet but its specifications: JAStudio inside Anki calls Anki through pythonnet as
before.**

- **`AnkiOverHttp`,** in `JAStudio.Anki.AnkisFunctions.OverHttp`, implements four of the five groups, each as one of
  its properties: `IAnkisNotes`, `IAnkisTags`, `IAnkisMedia` and `IAnkisDatabase`.
  - *Not `IAnkisWindows`:* the add-on's server doesn't serve the functions that drive Anki's windows yet.
- **It's given how to reach the server and the collection JAStudio works with:** the port and the token
  (`AnkisHttpServerAccess`), which is read from the access file (below), and the collection's file. How JAStudio
  comes by the collection isn't built.
- **Before its first call it compares contract versions,** and refuses an add-on that speaks another
  (`AnkiSpeaksAnotherContractException`). The version is 1 on both sides.
- **A refusal and a failure are each an exception:**
  - `AnkiRefusedTheCallException` has Anki's reason in words for a person, and the collection Anki has open when
    that isn't the one JAStudio works with. Nothing shows either to a person yet.
  - `CallToAnkiFailedException` has the HTTP status and what Anki said: Python's traceback when the function failed.
- **The read of the database returns the JSON as it arrived,** with no second serialization
  (`AnkisHttpServer.CallForItsJsonText`). JAStudio reads the study-notes through it with the classes it reads
  them with inside Anki (`AnkisOwnConnection`, `StudyNotesInAnkisDatabase`).
- **A call's arguments are serialized before they're sent,** so that the request says how long its body is: Python's
  `http.server` reads that many bytes, and nothing of a body sent in chunks, which is how .NET sends JSON by default.

**Specified against the add-on's own server** (`When_JAStudio_calls_Anki_over_HTTP`, 30 specifications): every
function of the four groups, the refusals and the failures. So a function whose name, arguments or answer differ
between the two sides fails there: it's the comparison of the Python functions with the C# contract that the design
calls ask for, made by using both.

- **Each specification gets a copy of the test collection of its own,** served by the add-on's `HttpServerForJAStudio`
  (`AnkiServingACopyOfTheTestCollection`).
- **One Python process serves them all** (`serving_copies_of_the_test_collection.py`, among the add-on's tests),
  started with the venv's Python when the first specification asks. The specifications tell it to start and stop
  servers, and to have one refuse calls as while Anki's own sync is under way.
- **The 30 take about 8 seconds, in parallel.**

### The access file as built: how a JAStudio finds the add-on's server

**Built 2026-11-15. Nothing in Anki starts the serving yet: only the tests and the specifications do.**

- **`ServingJAStudio`,** in the add-on, is its serving of JAStudio for as long as it lasts: it starts the HTTP
  server and then writes the access file, and it removes the file and then stops the server.
- **The access file holds the server's port and token,** as JSON: `{"port": …, "token": …}`. Both are new each
  time the serving starts, so a JAStudio reads the file each time it looks for Anki.
  - *Written whole or not at all:* to a file beside it, which then replaces it.
  - *For the user alone:* on macOS and Linux the file's mode says so. On Windows the folder does.
- **`AnkisHttpServerAccessFile`,** in `JAStudio.Anki`, reads it: the access, or none while no Anki serves, which
  is when the file isn't there.
- **The specifications reach the add-on's server through it:** each served copy of the test collection has an
  access file of its own, which is where the 34 specifications of JAStudio's calls over HTTP get the port and the
  token.

**Claude's design calls, for Magnus to see:**

- **The file is in the user's own folder for JAStudio's data:** `anki-http-server-access.json` in
  `%LOCALAPPDATA%\JAStudio` on Windows, where JAStudio keeps other files of the user's already, such as the
  `CardServer`'s port. Not in the add-on's folder, nor in the Anki profile's.
  - *Why:* a JAStudio started from any checkout has to find the Anki that runs, and it takes the collection Anki
    has open, so it can't look by collection.
  - *Both sides name the folder themselves:* .NET as its `LocalApplicationData`, and the add-on by the same rule
    for each platform (`the_access_file_of_this_user`). A specification compares the two on the platform it runs
    on: Windows here, Linux in CI. Not tried on macOS.
- **One file for the user, so one Anki at a time serves JAStudio.** A second Anki, started on another data folder,
  would write over the first one's file.
- **An Anki that ends without stopping leaves its file behind,** naming a server that no longer answers. Telling
  that apart from an Anki that runs is JAStudio's to do when it calls, which isn't built.

**What the HTTP path still lacks:**

- *In the add-on:* the functions that drive Anki's windows, `AnkiForJAStudio` for the real Anki, and starting
  `ServingJAStudio` as a profile opens.
- *In JAStudio:* `IAnkisWindows` over HTTP, and the standalone JAStudio's wiring: the Anki integration's classes
  given `AnkiOverHttp`'s groups, and whether the spaced repetition system is available (`SRSAvailability`) told
  from the access file, from Anki's refusals and from whether the server answers at all.
- *From Anki to JAStudio:* the cards' pages, and the rest that Python asks of .NET today.
- *In the contract:* `note_remove` goes the next time the contract changes, since `notes_remove` does the same for
  several notes (Magnus, 2026-11-15: "OK").

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
  *The add-on's side serves its version, and JAStudio's side compares it with its own before its first call
  (2026-11-15).*
- **A specification that compares the add-on's Python functions with the C# contract,** in place of the type stubs.
  *The specifications of `AnkiOverHttp` call every function on the add-on's own server (2026-11-15).*
- **The connection is open to the same user only,** such as by a token that only the user's files hold. *The server
  demands a token, which JAStudio reads from the access file, written for the user alone (2026-11-15).*
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
- **Tessaging inside JAStudio,** after the move: the move itself uses interfaces.
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
