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
- **Tabs.** A workspace holds any number of tabs, and each tab shows one note's page, with its own back and forward
  history. For now that's the card's answer side, rendered by the `CardServer`, in a web view. The pages move to
  Avalonia one note type at a time, sentences first: see [Technology](#technology-avalonia-the-notes-pages-included).
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

## Technology: Avalonia, the notes' pages included

**Decided 2026-11-13 (Magnus): the notes' pages move to Avalonia too, and Blazor stays only where a web view is
required: the cards in Anki's reviewer.** Until then, the decision of 2026-11-06 was that the web view only showed
the note's page, followed its links and passed on right-clicks, since HTML is good at richly formatted text and the
card pages existed.

Magnus's experience is that building significant applications on web technology costs far more effort than a
proper UI framework, and the boundary between native UI and a web page costs effort at every edge. The note's page
is now becoming the kind of UI that boundary taxes most: complex logic to edit and display, such as a vocab's
match-condition checked against a word in a sentence's breakdown.

- **Avalonia for the whole workspace:** the window, tabs, the navigation pane, editors, menus, dialogs, and the
  notes' pages.
- **The pages move one note type at a time, sentences first,** where words matched wrongly are seen. Until a type's
  page is built, its notes show as before: the card page in a web view. Once the last type's is built, the workspace
  has no web view, and the three things tying JAStudio to Windows that the web view brings go with it (see
  `multiplatform.md`).
- **Why it pays:**
  - An Avalonia page knows what was clicked: a breakdown entry holds its match, and can open the vocab's
    match-condition window checked against that very word. Through a web view, each such action is a message from the
    page's script, and what was clicked has to be found again from what the message says, the way the right-click
    menu guesses it from the text selected.
  - The headless JAStudio can show, click and read an Avalonia page, as it does the rest of the workspace. A web
    view's page is only an image there (`NoteTabImageView`), since a native web view needs a real window.
- **The cards in Anki's reviewer keep their Blazor pages,** to read while reviewing. Once the workspace's pages do
  what the reviewer's right-click menu does, the menu goes, with whatever else of the Blazor pages reviewing doesn't
  need: the workspace is a click or a shortcut away.
- **So the sentence breakdown is shown twice, for good:** by Blazor in the reviewer, and by Avalonia in the workspace.
  Both are views of the same view models in Core (`SentenceViewModel`, `MatchViewModel`, `CompoundPartViewModel`), so
  what the breakdown shows is decided once. The workspace's will grow editing; the reviewer's stays for reading.
- **The look (Magnus, 2026-11-13):** the same information and a similar feel as the cards, the same colours, but no
  aim at matching them pixel for pixel: the workspace's pages are built for editing.
- **The notes' texts hold markup,** which the pages show as styled text: HTML, such as `<b>` in a sentence's source
  question, and JAStudio's own tags, such as `<kanji>`, `<vocabulary>` and `<read>` in a vocab's answer or a kanji's
  mnemonic. A quarter of the vocab have some (7,258 of 28,004, counted 2026-11-13).

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
- **Views hold note ids, not note objects.** The cache replaces note objects on every full reload
  (`JPCollection.ReloadFromBackend`, after each Anki sync and when the profile reopens). Until 2026-11-08, TTS audio
  coming in from Anki did too.

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

## The sentence page in Avalonia, as built

Built 2026-11-13, as step 3 of [increment 10](#increments).

- **A tab shows a sentence's page in Avalonia** (`NoteTabViewModel.NotePage`, a `SentencePageViewModel`), and the other
  notes' card pages in its web view as before (`ShowsTheCardPage`). The view shows one or the other: the page in a
  `NotePagePresenter`, which scrolls it, and the card page in the `NativeWebView`, or as an image in the headless
  JAStudio. Which view shows which page model is the application's data template for it, in `App.axaml`.
- **The page is built from the same view models in Core as the card's** (`SentenceViewModel`, `MatchViewModel`,
  `CompoundPartViewModel`, and now `SentenceViewToggleViewModel` and `TokenTableViewModel`, which the card's razor
  components use too), so what the breakdown shows is decided once. A vocab's meta tags are a list
  (`VocabNoteMetaData.MetaTags`), which the card's HTML is made from.
- **A page shows the note as it was when it was made:** the tab makes it anew when the note or a setting changes, as the
  web view reloads a card's page, keeping the keyboard on it if it had it. A page of another note starts at the top.
- **Links:** a word's vocab, a compound's parts and a word's kanji are links (`NoteLinkViewModel`). A click shows the
  note in the tab, a middle-click opens it in a background tab.
- **Audio:** the ▶ buttons play the note's audio through `WorkspaceAudioPlayer`, and the sentence's own plays when the
  tab is first shown with it, as the card's page does. The headless JAStudio plays nothing
  (`WorkspaceAudioPlayer.PlayNothingFromNowOn`): an agent drives it, on Magnus's computer.
- **Keys:** the note view shortcuts work on the page as on a card's page (`NoteViewKeyGesture`), and the page scrolls as
  a browser's does: the arrows, Page Up and Page Down, Home and End.
- **Right-click** opens the note's menu (`NoteContextMenu`), for the text right-clicked: the text selected there, or the
  whole text, such as a word of the breakdown.
- **The layout:** a breakdown entry's answer stands beside the word, or below it when there's less than 250 pixels left
  beside it (`BesideOrBelowPanel`), rather than being squeezed into a sliver. The token tables, wider than most pages,
  scroll sideways on their own, so the rest of the page keeps wrapping at its width.
- **Left out of the card's styles:** those for a breakdown entry's `secondary_match`, `shadowed`, `configured_hidden`
  and `configured_incorrect_match` classes. The card puts a match's hiding reasons among its classes, and those read like
  `forbids::configured_hidden`, so the styles never applied.

## Proposed: the right-click menu by what's right-clicked

*An analysis for discussion, 2026-11-13. Magnus: "I'm rather eager to make the menu understand context rather than do
string magic. So that you immediately get the options you should be getting for that element, not everything under the
sun."*

### How it works now: from a string

A right-click on a note's page passes `NoteContextMenu` the note shown and two strings: the text right-clicked (the text
selected, or else the whole text of the block under the mouse) and the clipboard's text. Each string gets a submenu of
everything a string might be for. The menu then works out from the string what was meant. For example, "Add > Incorrect
matches" analyses the sentence again and finds every match whose form is the string; when the word occurs twice, a
submenu asks which, by its position. The Avalonia sentence page knows exactly which element was clicked (each control's
`DataContext` is its `BreakdownEntryViewModel`, with the `Match` behind it), but it hands the menu only the text.

The strings do two different jobs:
- **Naming an element on the page:** the match to mark incorrect or hidden, the sentence to highlight a vocab in, the
  vocab to make one of a kanji's primary vocab, the reading to make primary. The element itself should do this job.
- **Input:** a word to add as a synonym, a text to search the web for or to create a note from, where to split the
  sentence with a word-break tag. A selection does this job well, as it does now.

### The proposal

The right-click hands the menu the element clicked: its view model, and through it the domain objects (a `Match`, a
compound part's `VocabNote`, a kanji's `KanjiNote`). The menu is then:

```
<the element's own items: only those that apply to it as it is>
Vocab: 食べる ▸        the vocab's own note menu, as on its page (for an element with a vocab)
Sentence ▸             the sentence's note menu, as now
Selection: "…" ▸       only when text is selected
```

- **A fixed key for each item,** whichever element shows it, and the two halves of a toggle on the same key (mark
  incorrect and unmark, hide and unhide, highlight and remove the highlight), so that the keys stay muscle memory.
- **What doesn't apply is left out,** not shown disabled: a shown match has no "Unhide".

### What each element offers

**A breakdown entry for a vocab's match (`VocabMatch`).** Edit mode lists every match of every candidate word, so the
entry is one of these:

| The match | Its items |
|---|---|
| Any | "Match-condition of 食べる, checked against this word" (step 4 of increment 10). "Open 食べる", also in a background tab, for the keyboard. |
| Shown | "Mark incorrect here" (`IncorrectMatches`, at the word's position: `Match.ToExclusion`). "Hide here" (`HiddenMatches`, likewise). "Highlight" or "Remove the highlight" (`HighlightedWords`). |
| Hidden by hand (`configured_hidden`) | "Unhide": removes the exclusion that hides it. When that one is for the whole sentence (no position), the item says so, since it unhides the word everywhere in the sentence. |
| Hidden, shadowed by a longer word | The shadowing word's items: "Open 食べ物", "Mark 食べ物 incorrect here". That's usually what's wrong when a right word doesn't show. |
| Hidden by the vocab's flags (compositionally transparent compound, yields to a surface) or the setting that hides compounds | The vocab's matching flags, or the view toggle. |
| Invalid, marked incorrect by hand (`configured_incorrect`) | "Unmark incorrect", with the same caveat as "Unhide". |
| Invalid, its match-condition not met (`not_met`) | The match-condition item, first. "Highlight": a highlighted match is valid whatever fails (`Match.IsValid`), so it's the by-hand "it does match here". |
| Invalid, another match owns the form or wins by priority | The winning vocab's items: "Open 食べる", "Mark 食べる incorrect here". |
| Invalid for other reasons (poison word, dictionary form, inflection rules) | The vocab's matching flags. |

**A breakdown entry for a word only the dictionary has (`DictionaryMatch`):** "Create the vocab from the dictionary"
(`TheVocabNamedOrCreatedFromTheDictionary`), "Mark incorrect here", "Hide here", web searches for the word.

**A breakdown entry for a token nothing has (`MissingMatch`):** "Create a vocab", web searches, and "Show the tokens"
(edit mode's token tables), since how the tokenizer split the sentence is usually why.

**A compound part, under an entry:** "Open", "Remove from 食べ物's compound parts" (the compound right above it), and
"Vocab: 食べ ▸". No match-condition item: a compound part isn't matched in the sentence.

**A kanji, under an entry:** "Open", "Make 食べる one of 食's primary vocab" (last, or at a position) or "Remove 食べる from
食's primary vocab" (`KanjiNote.PrimaryVocab`), and "Kanji: 食 ▸". Today this takes selecting the vocab's text on the
kanji's page.

**The rest of the page** (question, answer, comments, empty space): the sentence's note menu, as now, with the selection
first when there is one. The audio buttons: "Text-to-speech audio…".

### Where today's string items go

- **The sentence's string menu:** adding and removing highlighted, hidden and incorrect matches go to the breakdown
  entries. Splitting with a word-break tag stays with the selection: the text is the input.
- **The vocab's string menu:** "Sentence > Add highlight / Remove highlight / Mark as incorrect match in sentence", which
  find the sentence by its whole question text, go to the sentence rows of the vocab's page (step 5). Removing a synonym,
  antonym, see-also and the like go to those rows on the vocab's page. Adding them, and creating combined vocab, stay with
  the selection and the clipboard: the text is the input.
- **The kanji's string menu:** the primary vocab go to the kanji's vocab rows and to the sentence page's kanji rows; the
  primary readings to the kanji's reading elements. Adding a similar-meaning or confused-with kanji stays with the
  selection.
- **The generic items** (web searches, exactly matching notes, create a note, reparse matching sentences) stay with the
  selection.

Anki's reviewer keeps the string menus until its menu goes (step 6 of increment 10).

### Found along the way

- **"Incorrect" and "invalid" name one thing.** By hand it's an incorrect match (`IncorrectMatches`,
  `configured_incorrect`), but the vocab's menu lists the sentences as "Marked invalid in sentences"
  (`vocab.Sentences.InvalidIn`). And a match is invalid (`Match.IsValid`) for many more reasons than being marked so.
- **"Highlighted Vocab" names two things:** a sentence's highlighted words (`HighlightedWords`), and, in the kanji's string
  menu, the kanji's primary vocab (`KanjiStringMenus`).
- **The vocab's "Sentence > Add Highlight" highlights in the first sentence with the question text only,** while "Remove
  highlight" and "Mark as incorrect match" act on all of them (`VocabStringMenus.BuildSentenceMenuSpec`). It goes away
  with the move to the sentence rows.

### Open questions

1. Leave out what doesn't apply, rather than show it disabled, with a fixed key for each item?
2. Mark incorrect and hide at the word's position only, or also offer "everywhere in this sentence" (a `WordExclusion`
   with no position)?
3. Leave the clipboard out of the elements' menus, keeping it in the sentence's menu (a right-click on empty space)?
4. One word for a match marked by hand, "incorrect" or "invalid"?

## Keyboard shortcuts

- **Note view shortcuts** (`NoteViewShortcuts`, in C#) are defined once and shared with Anki's reviewer and
  previewer, which Python binds from the same table. Most are single-key toggles for sentence breakdowns; a few
  act on the note being viewed.
- **Keys pressed on the note's page go to the web view,** so `card.js` reports each key press, and the tab asks the
  window for its shortcut before trying the note view shortcuts. The window's own shortcuts (Ctrl+O, Ctrl+Shift+O,
  Ctrl+E, Ctrl+W, Ctrl+T, Ctrl+Shift+T, Ctrl+M, Ctrl+Tab, Ctrl+Shift+Tab) also exist as Avalonia key bindings for when the
  keyboard is outside the page. Ctrl+M opens the match-condition window of the vocab the shown tab shows.
  Single-key shortcuts deliberately aren't window key bindings, because they would fire while typing in the
  editors to come.
- **A setting change reloads the page**, as do an update to the note on it and a reload of the collection after an
  Anki sync. A hidden tab reloads when it's shown instead.
- **Back and forward** (Alt+Left, Alt+Right, and the buttons) go through the tab's history of the notes it showed
  (`NoteTabHistory`), per tab and not saved. The tab keeps it, not the web view, so that one note's page can be shown
  in Avalonia and the next one's in the web view. Notes that leave the collection, deleted or gone after a reload, are
  forgotten, and a tab closes when the note it shows leaves. Anki's saved history of recently shown cards may come
  back later, as a "recently viewed" list.

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
    This was written while Windows' keyboard input was broken system-wide (see below), so the problem it answers
    may well have been that fault. It's kept because it's harmless and was tested together with the rest.
  - **Not while a dialog is open over it.** Code that gives the workspace the keyboard runs a moment after what
    asked for it, so a modal dialog, such as a search condition's editor, may have opened meanwhile. That code holds
    back while a window the workspace owns is open, and a dialog takes the Windows keyboard focus for itself when it
    opens. Otherwise typing in the dialog went to the workspace behind it (found 2026-11-07). This one was real:
    removing these safeguards brought the problem back with Windows healthy.
- **A Windows input fault, not JAStudio's (2026-11-07).** For a while, keys stopped reaching the search's condition
  tree after Alt+Tab or a switch of virtual desktops, and came back only after another window had taken activation.
  It turned out Windows' keyboard input was broken system-wide: even the Start menu's search typed odd kanji with a
  Western layout active. A reboot fixed it. So if keys stop reaching a control, check that other applications still
  get normal text before changing the code.
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
     "Matching" is the rest of the matching config, everything else that affects parsing, with the match-condition
     shown read-only and edited in a window of its own (see `matching-configuration.md`); "Metadata" is what doesn't,
     such as the register; "Related" is its relationships to other words, such as its synonyms, each added with a picker
     of the corpus's vocab (`VocabRelatedEditorViewModel`, since 2026-11-09).
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
8. **The workspace comes back after a restart.** *(Built 2026-11-08, Magnus: "immensely helpful".)* When the
   workspace window is left (`WorkspaceViewModel.SaveTheState`), its `WorkspaceState` is saved in
   `workspace-state.json` in the user files folder (`SavedWorkspaceState`), per machine and not versioned:
   - the notes the tabs show, and which tab is shown, though not each tab's back and forward history;
   - the search (`NoteConditionJson`), the List tab's list, which navigation tab is shown, and whether the navigation
     pane and the editor section are open, with their widths.

   It's shown again once the notes are loaded, at Anki's start when the collection is first loaded. Notes deleted
   meanwhile are left out. Until it's shown, nothing is saved, so leaving the window early doesn't overwrite it.
   Where the window was is kept separately (`WindowPlaceKeeper`, `multiplatform.md`).
9. **A deleted note leaves the workspace.** *(Built 2026-11-08.)* `JPCollection.NoteDeleted` closes the tabs showing
   the note, and takes it out of the search results and the List tab.

10. **The notes' pages in Avalonia.** *(Decided 2026-11-13: see
    [Technology](#technology-avalonia-the-notes-pages-included).)*
    1. **The tab keeps its own history,** of the notes it showed, rather than the web view's: a tab can then show
       one note's page in Avalonia and the next one's in the web view, and go back and forward between them.
       *(Built 2026-11-13: `NoteTabHistory`. The page reports a note link clicked, and the browser's own back and
       forward, and the tab has its web view load the page of the note it shows.)*
    2. **The notes' texts' markup as styled text in Avalonia.** *(Built 2026-11-13: `StyledText` in Core reads a text's
       HTML and JAStudio's own tags into styled runs, and `StyledTextBlock` shows them, in the cards' light colours, the
       note pages' brushes in `App.axaml`.)*
    3. **The sentence page in Avalonia,** with what the card's page has:
       - the screenshots, the question with its audio, the answer, and the comments;
       - the breakdown, with its view settings, each word's vocab with its audio, readings, meta tags and answer, its
         compound parts and its kanji, and in edit mode the reasons a match is incorrect or hidden;
       - in edit mode, the token tables;
       - following a note's link, and opening it in a background tab with a middle-click;
       - the note view shortcuts, and the note's right-click menu, for the word right-clicked.

       *Built 2026-11-13*, as described in [The sentence page in Avalonia, as built](#the-sentence-page-in-avalonia-as-built).
    4. **Opening a vocab's match-condition window from a word in the breakdown,** checked against that word (see
       `matching-configuration.md`).
    5. **The vocab page, then the kanji page.** Then the web view leaves the workspace.
    6. **The reviewer's right-click menu goes,** with what else of the Blazor pages reviewing doesn't need.

The order after step 1 is tentative. Steps 6 and 7 are parked because they free nothing from Anki, and the
workspace exists for the goal in `corpus-data-model.md`, where import and organizing come next.
