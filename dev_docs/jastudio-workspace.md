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
  object" true. A "collection reloaded" notification lets views refresh.
- **Update notifications become observables** (Compze's `SimpleObservable<T>`), so tabs and windows can
  unsubscribe when they close. The current listener lists can't be unsubscribed from.
- **Inactive tabs aren't refreshed in the background.** A tab refreshes when it's shown, if anything changed
  while it was hidden.

## Known technical points

- **Tabs must keep their web views alive.** An Avalonia `TabControl` is believed to detach unselected content,
  which would destroy a web view and lose its history and scroll position (unverified). The plan is to keep each
  tab's content in the tree and toggle visibility. Web views are created when a tab is first shown.
- **Reverting a note** reads its file from the `src/jas_database` git repository as of `HEAD`. It goes through the
  normal update path, so the cache, the files, the snapshot and the notifications stay consistent. A note created
  since the last commit has no committed version, so that case still needs deciding.

## Increments

1. **The main window becomes the workspace** and takes over the note viewer: main menu, back and forward, and
   the note's web view. One note at a time. *(Built 2026-11-06: `WorkspaceWindow` and `WorkspaceViewModel`.)*
2. **Observables** for note updates, once Compze's fixed `SimpleObservable` is released. That means a deliberate
   Compze version bump.
3. **Tabs.**
4. **The search pane**, moving the note search dialog in.
5. **The editor section**, one editor at a time, switching to live editing as each moves in. Matching rules
   first.
6. **Multiple workspaces.**
7. **Revert to the committed version.**

The order after step 1 is tentative.
