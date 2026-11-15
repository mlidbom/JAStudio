# Moving JAStudio out of Anki

> **Status: direction agreed with Magnus on 2026-11-14 and 2026-11-15; nothing is built.** The order of work is
> preliminary. Details are settled as each increment needs them.

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
- **The seams between Core and Anki already exist,** since the standalone and the headless JAStudio run without Anki:
  `AnkiAvailability`, `IAnkiWindows`, `IAnkiStudyNotes`, `IAnkiTagOperations`, `ICardOperations`, `IAnkiRawNotes`,
  `IBackendDataLoader` and `AnkiCollectionDatabase`.

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
a note is left out without a word when the study-notes are read
(`AnkiBackendDataLoader.StudyNoteRow.HoldsACorpusNoteId`). It's the check `corpus-validity.md` has wanted since
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
- **Today's seam names it two wrong ways.** `IAnkiStudyNotes`, `IAnkiTagOperations`, `IAnkiWindows` and
  `AnkiAvailability` put Anki's name on what is generic about studying. `IBackendDataLoader`, `BackendData`,
  `ExternalNoteIdMap` and `ICardOperations` say neither Anki nor studying.

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

1. **Names:** the seam in Core renamed in studying's words, while JAStudio still runs inside Anki on pythonnet. Names
   and homes first, wiring second (the code standard on re-homing concepts before restructuring). Every name Claude
   chooses gets a `//review-naming:studying:` comment for Magnus's pass.
   - *Left alone:* the rest of `studying.md`'s "To design", meaning the domain's words for reading, listening and
     suspending, and the states.
2. **Sync in a Studying view,** replacing the notifications, while still inside Anki. Magnus finds out early whether
   stale-until-sync really doesn't bother him.
3. **Menus:** a shortcut that shows the reviewed note in the workspace, then the Qt menus go.
4. **The HTTP path, built on main in pieces that Anki doesn't use until it's complete:** the add-on's HTTP server,
   `JAStudio.Anki`'s HTTP side, and the standalone JAStudio's wiring. Anki keeps working on pythonnet meanwhile, and
   Magnus sees each piece in Rider.
5. **One commit changes over:** the Anki add-on stops loading .NET. Magnus tries it on the `Anki-Development` profile
   first.
   - *No switch between the two paths.* Reverting that commit is the way back, which costs a revert, a rebuild and a
     restart, where a switch would cost a setting and a restart. A switch could never run both at once anyway: JAStudio
     inside Anki and the standalone JAStudio can't both hold the main corpus checkout (`CorpusCheckoutLock`).
6. **Once Magnus has studied with it for a while,** pythonnet, `runtime_binaries`, `typings/` and the stub generator
   are deleted.
7. **Starting JAStudio, installing it, publishing:** see [Distribution](#distribution-under-discussion).

## Distribution: under discussion

**Magnus's view (2026-11-15): the Anki add-on has to be in AnkiWeb's list of add-ons.** "I figure that no matter how
silly it might seem, not going through the standard distribution channel would tank usage. Once one has installed the
addon that looks so cool, the odds that one clicks the "Download and Install" button are very good. If one just hears
about some program somewhere you should install that will integrate with anki.... Odds plummet I would say".

**Withdrawn:** Claude's suggestion that JAStudio installs its Anki add-on into Anki. It would keep JAStudio out of
AnkiWeb's list. And with several JAStudios connected to one Anki, each would install its own add-on over the others'.

**Claude's suggestion, not yet answered:**

- **The Anki add-on is the way in.** A student installs it from AnkiWeb, as any add-on. When it finds no JAStudio, it
  offers to download and install the release it goes with, and it starts it.
- **A contract version keeps the two in step,** where the withdrawn suggestion had them be the same build. The HTTP
  contract has a version, which changes when the contract does, and which the two compare when they first talk.
  Several JAStudios built from different checkouts then all work with one add-on, as long as their contract version is
  the add-on's.

**Not known:** what AnkiWeb lets an add-on download and run, and whether it limits an add-on's size. Anki's page on
sharing add-ons says nothing of either (read 2026-11-15).

**If handing over the keyboard takes uiAccess,** an install started by the add-on needs elevation, since JAStudio then
has to be in a protected folder (Magnus, 2026-11-15; see
[Deferred until the work reaches them](#deferred-until-the-work-reaches-them)).

## Claude's design calls for the HTTP increment

For Magnus to see, and to change if he doesn't share them. Nothing is tried.

- **A version check when the two first talk:** an add-on and a JAStudio whose contracts differ refuse each other,
  saying so. What the version is of is under [Distribution](#distribution-under-discussion).
- **A specification that compares the add-on's Python functions with the C# contract,** in place of the type stubs.
- **The connection is open to the same user only,** such as by a token that only the user's files hold.
- **The write of a study-note crosses as the fields to write,** not as a note.
- **The add-on's handlers get specifications that run without Anki's window,** against a collection file opened with
  Anki's own library. The repository has no such test today.

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
  [Distribution](#distribution-under-discussion).

## Also turned down

- **Loading JAStudio's assemblies into an `AssemblyLoadContext` that can be unloaded,** the smaller way that
  `speeding-up-development.md` asks about. It would spare the restart of Anki, and nothing else: the native binaries,
  the .NET runtime on the student's machine and the unknowns on macOS and Linux would all stay.
- **A documented Compze.Tessaging wire protocol that a peer not written in .NET can speak,** which Claude recommended
  and withdrew. Magnus, 2026-11-14: "At the stage tessaging is at I'm not sure we want to promise anything about any
  sort of stable formats."
