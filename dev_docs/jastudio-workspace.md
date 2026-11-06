# The JAStudio Workspace — Design Direction

> **Status: direction agreed with Magnus (2026-11-06); details are decided as each increment needs them.**
> Built incrementally. See [Increments](#increments) for where it stands.

## Why

JAStudio's UI grew as isolated windows opened from Anki:
- the "JAStudio Browser" main window, which held nothing but a main menu;
- the note search dialog;
- the note viewer;
- one editor window per note type, plus a separate matching rules window for vocab.

As Anki's role shrinks to scheduling, text-to-speech audio and importing (`corpus-data-model.md`), that design
stops making sense. Search results, the note you're viewing and its editors belong together.

## The shape

- **Workspaces.** A workspace is a window. Several can be open at once.
- **Tabs.** A workspace holds any number of tabs, and each tab shows one note:
  - the note's page (its card's answer side, rendered by the `CardServer`) in a web view, with its own back and
    forward history;
  - optionally, the note's editors, in a collapsible section on the right.
- **Search** lives in the workspace too, as a pane whose results open in tabs. The menu items that open lists of
  notes in Anki's browser today fill that pane instead.

Defaults, unless something changes them:
- Following a link navigates within the tab. Middle-clicking a link opens it in a new tab.
- Opening a note that is already open in the workspace switches to its tab. That is for convenience; sync doesn't
  require it.
- A link clicked in Anki's reviewer opens in the most recently used workspace.

## Technology: Avalonia, with the web view only showing the note

- **Avalonia** for the workspace and everything structured: the window, tabs, the search pane, editors, menus
  and dialogs.
- **The web view (Blazor) only shows the note's page**, follows its links and passes on right-clicks. HTML is the
  best tool for that richly formatted text, and the card pages already exist.

Magnus's experience is that building significant applications on web technology costs far more effort than a
proper UI framework, and the boundary between native UI and a web page costs effort at every edge. So the
boundary stays this narrow.

## MVVM

The workspace and its parts get view models. Views bind to them. Code-behind is limited to plumbing a view model
can't own: the web view's message channel, focus, the cursor position, and opening `PopupMenuWindow`. It
translates (for example "the page now shows this URL") and calls the view model.

The shared Avalonia view guideline's exception for bare window mechanics stays in general. It just doesn't
apply here.

## Data: live editing and sync

- **Live editing.** An edit is written to the note when a field is committed. There's no Save button. JSON files
  in git make this unusually safe. A planned "revert this note to its last committed version" covers mistakes.
- **One shared object per note.** `NoteCache` holds one object per note, and every view shows that object, so
  several editors on the same note need no special handling. An editor refreshes every field from the note except
  one being typed in.
- **Views hold note ids, not note objects.** Today the cache replaces note objects in two cases:
  - an edit coming in from Anki (`CreateNoteByMergingAnkiData`);
  - every full reload (`JPCollection.ReloadFromBackend`, after each Anki sync and when the profile reopens).

  A view holding the old object would then show and edit an orphan. Looking the note up by id keeps "one shared
  object" true. `JPCollection.ReloadedFromBackend` lets views refresh.
- **Update notifications are observables** (Compze's `SimpleObservable<T>`), so tabs and windows can
  unsubscribe when they close: `JPCollection.NoteUpdated` and `ReloadedFromBackend`, each note cache's
  `NoteUpdated`, `JapaneseConfig.Changed` and `ConfigurationValue<T>.Changed`. A subscriber may subscribe or
  unsubscribe from inside a notification. Notifications arrive on the thread that made the change.
- **Inactive tabs aren't refreshed in the background.** A tab refreshes when it's shown, if anything changed
  while it was hidden.

## Keyboard shortcuts

- **Note view shortcuts** (`NoteViewShortcuts`, in C#) are defined once and shared with Anki's reviewer and
  previewer, which Python binds from the same table. Most are single-key toggles for sentence breakdowns; a few
  act on the note being viewed.
- **Keys pressed on the note's page go to the web view,** so `card.js` reports each key press, and the tab asks the
  window for its shortcut before trying the note view shortcuts. The window's own shortcuts (Ctrl+O, Ctrl+Shift+O,
  Ctrl+W, Ctrl+Tab, Ctrl+Shift+Tab) also exist as Avalonia key bindings for when the keyboard is outside the page.
  Single-key shortcuts deliberately aren't window key bindings, because they would fire while typing in the
  editors to come.
- **A setting change reloads the page**, as do an update to the note on it and a reload of the collection after an
  Anki sync. A hidden tab reloads when it's shown instead.
- **Back and forward** are the page's history (Alt+Left, Alt+Right), per tab and not saved. Anki's saved
  history of recently shown cards may come back later, as a "recently viewed" list.

## Known technical points

- **Tabs keep their web views alive.** Removing a `NativeWebView` from the visual tree destroys its native
  control, and with it the page, its history and its scroll position. Hiding it only hides the native window
  (verified in Avalonia's `NativeControlHost`). A `TabControl` swaps the selected tab's content in and out of the
  tree, so the workspace uses a `TabStrip` for the headers and keeps every tab's `NoteTabView` in the tree, the
  hidden ones invisible.
- **The keyboard follows the shown tab.** Hiding a web view doesn't take the keyboard focus away from it, so a
  tab focuses its page when it's shown. Otherwise keys would go to the hidden tab's page and act on its note.
- **New tabs open in front.** The web view doesn't tell a hidden page that it's hidden, so a page loaded in a
  background tab would play its note's audio unseen.
- **Reverting a note** reads its file from the `src/jas_database` git repository as of `HEAD`. It goes through the
  normal update path, so the cache, the files, the snapshot and the notifications stay consistent. A note created
  since the last commit has no committed version, so that case still needs deciding.

## Increments

1. **The main window becomes the workspace** and takes over the note viewer: main menu, back and forward, and
   the note's web view. One note at a time. *(Built 2026-11-06: `WorkspaceWindow` and `WorkspaceViewModel`.)*
2. **Observables** for note updates and setting changes. *(Built 2026-11-06, on `Compze.Internals.SystemCE`
   0.4.1-internal.)*
3. **Tabs.** *(Built 2026-11-06: `NoteTabView` and `NoteTabViewModel`.)* Opening a note from outside the
   workspace selects a tab already showing it, or else opens a new tab. Middle-clicking a link opens a new tab.
   Ctrl+W or middle-clicking a tab closes it; Ctrl+Tab and Ctrl+Shift+Tab switch tabs. Each tab subscribes to
   the update notifications and unsubscribes when it's closed.
4. **The search pane**, moving the note search dialog in.
5. **The editor section**, one editor at a time, switching to live editing as each moves in. Matching rules
   first.
6. **Multiple workspaces.**
7. **Revert to the committed version.**

The order after step 1 is tentative.
