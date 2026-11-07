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
- **Tabs.** A workspace holds any number of tabs, and each tab shows one note's page (its card's answer side,
  rendered by the `CardServer`) in a web view, with its own back and forward history.
- **The editor section,** collapsible, on the right, shows the editors of the shown tab's note. There's one per
  workspace, opened or closed for all tabs at once.
- **The navigation pane,** on the left, holds ways to find notes, as tabs of their own, whose notes open in the
  workspace's tabs: the search, and the list a note menu's "Open" item shows. Later perhaps more, such as the tag
  tree.

Defaults, unless something changes them:
- Following a link navigates within the tab. Middle-clicking a link opens it in a new tab in the background.
- Opening a note that is already open in the workspace switches to its tab. That is for convenience; sync doesn't
  require it.
- A link clicked in Anki's reviewer opens in the most recently used workspace.

## Technology: Avalonia, with the web view only showing the note

- **Avalonia** for the workspace and everything structured: the window, tabs, the navigation pane, editors, menus
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

- **Live editing.** Every change is written to the note at once: a check box when it's clicked, a text field on
  every keystroke. There's no Save button. JSON files in git make this unusually safe. A planned "revert this note
  to its last committed version" covers mistakes.
- **One shared object per note.** `NoteCache` holds one object per note, and every view shows that object, so
  several editors on the same note need no special handling. An editor refreshes every field from the note whenever
  the note is updated.
- **A refresh never overwrites typing.** A text field is written on every keystroke, so the note holds what its text
  box shows. Note fields trim their text, and a comma separated list may be read back as the list joined again, so a
  text field editor (`TextFieldEditorViewModel`) keeps the text as typed as long as it means the same; otherwise a
  trailing space or ", " being typed would disappear. When the note changes what was typed on purpose, as a kanji's on readings are turned
  into hiragana, the text box keeps showing what was typed until the editor is shown again; the note is right either
  way. The one field that isn't written on every keystroke, a vocab's question, keeps its uncommitted text through a
  refresh.
- **Avalonia's bindings swallow exceptions from view model setters** (`BindingExpression.WriteValueToSource` catches
  them all and drops the change). So a setter that does real work, such as writing to a note or showing the tab
  clicked in the tab strip, runs it through
  `IFatalErrorHandler.HandleExceptionsFrom`, and an exception reaches the user.
- **Views hold note ids, not note objects.** Today the cache replaces note objects in two cases:
  - an edit coming in from Anki (`CreateNoteByMergingAnkiData`);
  - every full reload (`JPCollection.ReloadFromBackend`, after each Anki sync and when the profile reopens).

  A view holding the old object would then show and edit an orphan. Looking the note up by id keeps "one shared
  object" true. `JPCollection.ReloadedFromBackend` lets views refresh.
- **Editors read and write through the note as the collection holds it now.** Besides the note objects, a note's
  flag and rule objects are recreated whenever its tags change, and they cache their state. So each editor row
  holds a way to find its field on the current note, never the field object itself, and the tab refreshes its
  editors whenever the note is updated.
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
  Ctrl+E, Ctrl+W, Ctrl+Tab, Ctrl+Shift+Tab) also exist as Avalonia key bindings for when the keyboard is outside the page.
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
- **Taking the keyboard back from a web view.** Focusing an Avalonia control in code doesn't take the Windows
  keyboard focus away from a web view; Avalonia does that only when the window is clicked. So Ctrl+O first gives
  the window back its keyboard focus (`NativeKeyboardFocus`), then puts it on the search's condition tree.
  - **After Alt+Tab back to the window,** Windows gives the keyboard to the native window that had it last, which can
    be a tab's web view even when an Avalonia control has Avalonia's focus. The keys then went to the page, and
    clicking the control didn't help (found 2026-11-07, in the search). So the window takes the keyboard back
    whenever it's activated, unless Avalonia's focus is on a web view, and whenever it's clicked outside a page.
- **A background tab loads its page when it's first shown.** The web view doesn't tell a hidden page that it's
  hidden, so a page loaded in a background tab would play its note's audio unseen. Until it's first shown, the tab
  has only its header, taken from the link.
- **Reverting a note** reads its file from the `src/jas_database` git repository as of `HEAD`. It goes through the
  normal update path, so the cache, the files, the snapshot and the notifications stay consistent. A note created
  since the last commit has no committed version, so that case still needs deciding.

## Increments

1. **The main window becomes the workspace** and takes over the note viewer: main menu, back and forward, and
   the note's web view. One note at a time. *(Built 2026-11-06: `WorkspaceWindow` and `WorkspaceViewModel`.)*
2. **Observables** for note updates and setting changes. *(Built 2026-11-06, on `Compze.Internals.SystemCE`
   0.4.1-internal.)*
3. **Tabs.** *(Built 2026-11-06: `NoteTabView` and `NoteTabViewModel`.)* Opening a note from outside the
   workspace selects a tab already showing it, or else opens a new tab. Middle-clicking a link opens a new tab in
   the background.
   Ctrl+W or middle-clicking a tab closes it; Ctrl+Tab and Ctrl+Shift+Tab switch tabs. Each tab subscribes to
   the update notifications and unsubscribes when it's closed.
4. **The navigation pane**, moving the note search dialog in, and then the lists of notes the note menus opened in
   Anki's browser. *(Built 2026-11-06: `NavigationPaneViewModel`, with `NoteSearchViewModel` and
   `NoteListViewModel`; the dialog is gone.)*
   - The pane is on the left, resized by dragging its edge, and opened or closed with the "Navigation" button. Its
     tabs are "Search" and "List".
   - Ctrl+O, anywhere including in Anki, brings the workspace forward, opens the pane on its search and puts the
     keyboard on the search's condition tree; pressed in the workspace while the pane is open on its search, it closes the pane
     and gives the keyboard to the shown page.
   - The "Open" items of the note menus show a `NoteList`, a title and notes, in the "List" tab, replacing the list
     shown before, ordered by question. None of them opens Anki's browser any more.
   - The "List" tab's "Show in Anki's browser" opens Anki's browser on the same notes, by their Anki ids, for what
     only Anki does with them, such as suspending their cards. It's disabled when no listed note is in Anki: the
     search would be empty, and Anki's browser would show its default search instead.
   - Both tabs list their notes the same way (`ListedNotesViewModel`): each row is a note's question and answer.
     When notes of more than one type are listed, each type gets a tab of its own, and the type picked last stays
     shown as long as there are notes of it, since one usually knows which type one is after. A search lists at most
     100 notes of each type; the search keeps its own order, shortest question first, which tends to put the closest
     match on top.
   - Double-click or Enter opens a note the way opening a note from outside does; middle-click opens it in a
     background tab.
5. **The editor section**, one editor at a time, switching to live editing as each moves in. Matching rules
   first. *(Built 2026-11-06: every note type's editors are in the workspace, `VocabEditorSectionViewModel`,
   `KanjiEditorSectionViewModel` and `SentenceEditorSectionViewModel`, and the editor dialogs are gone.)*
   - **One editor section per workspace,** on the right, opened and closed with Ctrl+E or the "Editor" button and
     resized by dragging its edge. It shows the editors of the shown tab's note, so it stays open as links are
     followed and tabs are switched. Each tab keeps its own editors, for the note it shows.
   - **A vocab's editors are tabs:** "Text" is its text fields, forms and parts of speech (`VocabTextEditorViewModel`);
     "Matching" is the rest of the matching config, everything else that affects parsing; "Metadata" is what doesn't,
     such as the register.
   - **Every change is written to the note at once.** A change to what the vocab matches, on the "Matching" tab or to
     its question, forms or parts of speech, reparses the vocab's sentences in the background
     (`VocabSentencesReparserViewModel`), and the vocab's page then reloads to show the sentences it now matches. The
     section shows that it's reparsing, and no tab can be changed meanwhile, because the reparse reads the vocab. The
     reparse shows no progress dialog (`ReparseSentencesForVocabWithoutProgressDialog`): one popping up after every
     change would be in the way.
   - **A vocab's Question, Forms and Parts of speech aren't written on every keystroke,** since every keystroke would
     start a reparse. The Question box saves on Enter or when the keyboard leaves it, light red while it holds unsaved
     text (`CommittingTextFieldEditorViewModel`). Forms and Parts of speech are lists, where each added or removed
     entry is one change. The Forms list shows only the forms besides the question: the question is always a form,
     and isn't stored as one. The other text fields are written on every keystroke.
   - **A kanji's editor** (`KanjiEditorSectionViewModel`) is its text fields. Every keystroke is written to the note,
     and the kanji's generated data, such as its active answer, is updated with it.
   - **A sentence's editor** (`SentenceEditorSectionViewModel`) is its text fields, written on every keystroke with its
     generated data. A change to its question reparses the sentence on the spot, which is quick enough for every
     keystroke.
   - "Edit" in each note type's note menu opens the note in the workspace with the section open. The editor dialogs
     are gone.
6. **Multiple workspaces.** *(Parked 2026-11-06. Leaning, 2026-11-07: this is how several searches stay open,
   each workspace keeping its own search, results and opened notes. See `note-search.md`.)*
7. **Revert to the committed version.** *(Parked 2026-11-06.)*

The order after step 1 is tentative. Steps 6 and 7 are parked because they free nothing from Anki, and the
workspace exists for the goal in `corpus-data-model.md`, where import and organizing come next.
