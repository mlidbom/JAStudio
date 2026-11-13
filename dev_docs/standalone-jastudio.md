# The standalone JAStudio

**JAStudio with its workspace window, exactly as inside Anki, but with Anki entirely out of the loop** (Magnus,
2026-11-13). It runs from Rider, on the checkout Rider has open, so two checkouts' JAStudios, such as main's and a
branch's, can run side by side and be compared: Anki allows one instance at a time.

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

## What is different, because it is Anki

There's no Anki at all (`AnkiAvailability.Unavailable`), not even read-only:

- **No note has a study-note:** every note shows as not studied, none has Anki tags (`anki::`, the priority tags), a
  kanji has no default primary vocab, and searches on studying find nothing. Nothing saved to the corpus depends on
  these.
- **Not offered:** creating a note (it creates the note's Anki note), importing, opening a note in Anki's browser or
  previewer, and suspending or unsuspending cards. Their menu items are greyed out.
- **A change to a note isn't written to its study-note in Anki.** JAStudio inside Anki writes a study-note only as its
  note changes, so a note changed by the standalone JAStudio leaves its study-note behind until it changes again
  inside Anki (see `IAnkiStudyNotes`, and the MCP tools `list_the_study_notes_that_differ` and
  `write_the_study_notes_that_differ`).

The seams that make this so: `AnkiAvailability`, `IAnkiWindows` (Anki's windows, `NoAnkiWindows` without Anki), and
the standalone host's `NoAnki…` stand-ins for the rest.

## One JAStudio per corpus checkout

Two JAStudios saving notes to one corpus checkout would each overwrite the other's edits. So every JAStudio, inside
Anki, standalone or headless, holds the corpus checkout it works on for as long as it runs (`CorpusCheckoutLock`, in
`CoreApp`), and one started on a checkout another holds refuses to start, saying so. On the main checkout, that means
the standalone JAStudio runs only while Anki is closed, and Anki's JAStudio won't start while the standalone one runs.

The hold is a file beside the corpus checkout, `src/jas_database.lock`, held open and shared with no one. The system
deletes it when the process ends, however it ends, so a crash leaves no lock behind.

## Closing it

Closing the workspace ends the standalone JAStudio, after saving the workspace's state and the window's place. Inside
Anki it only hides the workspace (`WorkspaceWindow.ClosingItEndsJAStudio`, from Avalonia's shutdown mode), and
Ctrl+Shift+O, which hides it inside Anki, does nothing here: nothing could show it again.
