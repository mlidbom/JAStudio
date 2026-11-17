# The standalone JAStudio

**JAStudio with its workspace window, exactly as inside Anki, as a program of its own** (Magnus, 2026-11-13). It
runs from Rider, on the checkout Rider has open, so two checkouts' JAStudios, such as main's and a branch's, can
run side by side and be compared: Anki allows one instance at a time.

**Since 2026-11-15 it calls the Anki that runs beside it, over HTTP,** where it had no Anki at all: see
[What is Anki's](#what-is-ankis-it-calls-the-anki-that-runs-for) below. It's the JAStudio that JAStudio inside
Anki is to become (`moving-jastudio-out-of-anki.md`).

The project is `JAStudio.StandaloneHost`, beside `JAStudio.HeadlessHost`, the headless JAStudio, which has no screen
and serves JAStudio's MCP server.

## Running it

Run the Rider run configuration **Standalone JAStudio** (`.run/Standalone JAStudio.run.xml`). It builds and starts
`JAStudio.StandaloneHost` on the add-on folder of the checkout Rider has open, `$PROJECT_DIR$/src`. Started by hand,
it takes that folder as its one argument.

## What is the same as inside Anki

It works on the add-on folder as JAStudio inside Anki works on the one Anki loads it from, so everything comes from
the same files:

| What | Where |
|---|---|
| The notes | the checkout's corpus checkout, `src/jas_database` |
| The configuration | `src/user_files/configuration.json` (`ConfigurationFile`) |
| Every window's size and place | `src/user_files/window-places.json` (`RememberedWindowPlaces`) |
| The workspace's tabs and search | `src/user_files/workspace-state.json` (`SavedWorkspaceState`) |

The workspace window is the same one, made by the same call (`UIApp.InitializeWorkspaceWindow`), with the same main
menu. It shows at once, and the collection loads in the background, as inside Anki once an Anki profile opens.

The configuration was Anki's to keep until 2026-11-13, as the add-on's config in `src/meta.json`. JAStudio keeps it
itself now; the first time it finds no `configuration.json`, it starts from what Anki kept.

## What is Anki's, it calls the Anki that runs for

Anki is a program beside it, which starts, ends and opens another profile on its own. The standalone JAStudio
finds the Anki that runs through the access file the Anki add-on writes, and calls it over HTTP (`AnkiOverHttp`),
with the classes JAStudio inside Anki reaches Anki with. How it's built is in `moving-jastudio-out-of-anki.md`,
under "The standalone JAStudio calls the Anki that runs, as built".

- **While Anki runs with a profile open,** it has what JAStudio inside Anki has of Anki: the study-notes, read as
  it loads and at each sync, with their studying statuses and Anki tags; the Sync button; importing; Anki's
  browser and previewer; suspending and unsuspending cards.
- **While Anki can't be used,** it loads all the same, with no study-notes: every note shows as not studied, none
  has Anki tags (`anki::`, the priority tags), a kanji has no default primary vocab, and searches on studying
  find nothing. Nothing saved to the corpus depends on these. The Sync button, asked to sync, says why Anki can't
  be used. Once it can, a sync reads the study-notes.
- **It works with the collection Anki had open at its first call.** When Anki opens another profile, the Sync
  button says so, and offers to use the collection Anki has open instead.
- **Whichever checkout it runs on, it calls the one Anki that runs.** A sync from a branch's standalone JAStudio
  writes the study-notes that differ from that checkout's corpus into Anki, and runs the import profiles that run
  automatically, which take their raw notes out of Anki and into that checkout's corpus.
- **What Anki shows comes from JAStudio inside Anki still:** the cards' pages, the note view shortcuts and the
  menus. The standalone JAStudio serves Anki nothing yet.

The seams: `SRSAvailability` (`AnkisAvailabilityOverHttp`), `ISRSStudyNotes`, `ISRSTagOperations`, `ISRSWindows`,
`ISRSMedia` and `IAnkiRawNotes`, each implemented by the Anki integration, `JAStudio.Anki`.

## One JAStudio per corpus checkout

Two JAStudios saving notes to one corpus checkout would each overwrite the other's edits. So every JAStudio, inside
Anki, standalone or headless, holds the corpus checkout it works on for as long as it runs (`CorpusCheckoutLock`, in
`CoreApp`), and one started on a checkout another holds refuses to start, saying so. On the main checkout, that means
the standalone JAStudio runs only while Anki is closed, and Anki's JAStudio won't start while the standalone one runs.

The hold is a file beside the corpus checkout, `src/jas_database.lock`, held open and shared with no one. The system
deletes it when the process ends, however it ends, so a crash leaves no lock behind.

## Its MCP server

As inside Anki, it runs JAStudio's MCP server from its start, before the collection has loaded, and announces it among the running
JAStudios (`RunningJAStudios`), so Claude can connect to it, as `standalone-<branch>`, such as `standalone-unidic`, and
to several JAStudios at once, such as to compare how two branches parse (Magnus, 2026-11-13). See `jastudio-mcp.md`.

## Closing it

Closing the workspace ends the standalone JAStudio, after saving the workspace's state and the window's place. Inside
Anki it only hides the workspace (`WorkspaceWindow.ClosingItEndsJAStudio`, from Avalonia's shutdown mode), and
Ctrl+Shift+O, which hides it inside Anki, does nothing here: nothing could show it again.
