# Note Search — Model, Editor and Results

> **Status: agreed direction (2026-11-07).** Details are settled as each increment needs them. Tags themselves
> are specified in [tags.md](tags.md).
>
> **The editor's mechanisms and keyboard shortcuts are preliminary.** Matching Anki's UI style or its shortcuts is
> not a goal in itself (Magnus, 2026-11-07).

## Why

Magnus constantly uses complex Anki searches to find the right set of notes, for example
`tag:… -tag:… is:suspended`, with substring matches and boolean conditions. JAStudio's search can't do that.
It's the "Search" tab of the navigation pane (`NoteSearchViewModel`):
- it splits the text on ` && `;
- `r:` restricts a condition to reading fields;
- everything else is a substring match on the question, answer and similar fields;
- it lists at most 100 notes of each type.

Magnus (2026-11-07): the navigation pane should get a full search that can express such searches. The results should be a list with a column per sortable value, not today's two-line rows.

## Requirements (Magnus)

- **Boolean conditions** (and, or, not, grouping), **substring matches**, and **tag searches** that understand
  the tag hierarchy.
- **Regular expressions** too, unless that turns out unfeasible.
- **Studying state,** such as whether a note's reading or listening is being studied.
- **An editor that makes building such searches quick,** almost as quick as typing them.
- **Results with a column per sortable value,** sorted by clicking a column.
- **No cap on how many notes a search lists,** until a search proves unmanageably slow.
- **The search lives in the navigation pane,** which is resizable, so it can be made as wide as needed.
- **Several searches open at once,** designed in from the start. A search can take time to set up, and one
  doesn't want to lose it to check something else. Leaning: one search per workspace, and several workspaces (see
  [Where the search lives](#where-the-search-lives)).
- **Tag management is separate from the search** (Magnus, 2026-11-07). In the search, a tag condition is added by
  typing, which covers what the search needs. See [tags.md](tags.md).
- **Results are notes.** The corpus has no concept of cards.

## Decided (Magnus, 2026-11-07)

- **The search is a domain concept in Core: a tree of conditions on typed note properties.** See
  [The search model](#the-search-model). This also moves today's search logic out of the UI, which was already
  planned.
- **A search is built in a tree editor, not typed as text.** Every edit leaves a valid search, and with keyboard
  shortcuts that act on the selected part of the tree, building a search is almost as quick as typing it. See
  [The search editor](#the-search-editor).
- **No text syntax and no parser, at first.** The domain model is the tree either way, so a text form can be added
  later if it's missed. See [Maybe later: a text form](#maybe-later-a-text-form).
- **Matching is in memory, by scanning the notes.** If the corpus ever outgrows a plain scan, an index can go in
  underneath the search model. See [Research](#research-can-we-use-something-that-exists).

## The search model

The names below are placeholders, chosen when it's built.

- **A condition is on a property of the note, and the property's type decides how it matches.** There's no
  separate syntax for true-or-false states, such as Anki's `is:`.
- **The property types,** each with its own .NET implementation and its own condition editor in the UI:
  - **text,** one or more texts, such as the question or a vocab's forms: equals, contains, matches a pattern with
    wildcards, or matches a regular expression, and matches when any of the texts does;
  - **tags:** a list whose entries also match their parent tags, so a condition on `x` includes the `x` branch;
  - **true-or-false,** such as "studying reading";
  - **one of a fixed set,** such as the note type;
  - **number,** if one is ever wanted, such as the sentence count.
- **A property** has a name, the note types that have it, and a way to read its value from a note.
- **A search is a literal tree of conditions:**
  - a **condition on a property,** such as "tags include `source::anime`";
  - **all of** a group of conditions (AND), and **any of** a group (OR);
  - **not** a condition or a group.
- **An empty group adds nothing to the search,** and an empty search lists every note.
- **The operations on the tree are part of the model:** adding, editing and removing a condition, adding a group,
  switching a group between "all of" and "any of", negating, wrapping in a new group, and moving within a group.
  The view models only call them, so specs cover them without the UI.
- **Searches are values.** Two searches with the same conditions are equal. Specs can then compare the tree an
  operation produced with one written out, and an undo can keep earlier trees.

### Conditions in code: nested calls that indent like the tree (Magnus, 2026-11-07)

A search written in code reads like the tree and indents like the tree, which keeps specs readable. **Factory
methods are used wherever they read better than constructors;** constructors only where they really are better.
For example (the names are placeholders):

```csharp
ConditionGroup.AllOf(
   NoteProperties.Tags.Include("source::anime::natsume"),
   ConditionGroup.Not(NoteProperties.Tags.Include("-::priority::low")),
   ConditionGroup.AnyOf(
      NoteProperties.Question.Contains("行く"),
      NoteProperties.Answer.MatchesRegex("go(ing)?")),
   NoteProperties.StudyingReading.Is(false))
```

- **A condition on a property is created by the property itself,** such as `Question.Contains(…)`. The property's
  type then decides, at compile time, which matches exist: `StudyingReading.Contains(…)` doesn't compile.
- **Groups and negation** come from factory methods too: `ConditionGroup.AllOf(…)`, `ConditionGroup.AnyOf(…)` and
  `ConditionGroup.Not(…)`.

A failing spec should print the search indented like the tree, so the difference is easy to see.

### Properties (draft)

| Property | Type | Matches | Notes |
|---|---|---|---|
| searchable text | text | Notes with any searchable field that matches | The fields the old search box covered (open question 2). In Anki it's every field except tags. |
| tags | tags | Notes with the tag or a tag in its branch. Also: a tag pattern, a regex, or no tags at all. | See [tags.md](tags.md). |
| studying reading, studying listening | true-or-false | Notes whose reading, or listening, is being studied: the note is in the studying-add-on (today Anki) and that card isn't suspended | Magnus's proposal. Replaces Anki's `is:suspended` and `card:`, which are about cards. See [studying.md](studying.md). |
| note type | one of a fixed set | vocab, kanji or sentence | |
| question, answer, reading, and other fields | text, or list of texts | Notes whose field matches | The fields per note type are to be decided. |
| id | text | Notes with those JAStudio note ids | |
| JAStudio-specific properties (later) | | For example the vocab a sentence contains, the kanji, the forms, the parts of speech | Each one when a workflow needs it. "Contains vocab" fits Magnus's habit of adding sentences that contain the vocab he's studying. |

**Anki's searches about its cards and scheduling are left out:** decks, flags, card properties, review history,
and card states other than suspended. Studying state that matters can be added as properties of notes, as the two
studying properties are, without tying the search to Anki ([studying.md](studying.md)).

**Regular expressions** work wherever text is matched, using .NET's regex dialect.

**Maybe later:** whole-word matching, and matching that ignores accents and, usefully for Japanese, voicing marks,
so は matches ば and ぱ (Anki's `nc:`).

## The search editor

Magnus (2026-11-07): proper UI elements combined with menu accelerator keys and keyboard shortcuts that act on
the selected part of the condition tree. With some design, editing a tree this way can be almost as quick as
typing, and the search is never invalid.

**Adding a condition,** for example a tag:
1. Select a group, or start with the empty search, and click its + icon or press the add key.
2. A menu lists the properties. A single key picks one: T for tags.
3. That property type's editor opens. For tags, type parts of the tag's path and pick it from the matching tags
   (see [The tag condition's editor](#the-tag-conditions-editor)).
4. Enter, or the Add button, adds the condition to the selected group.

**The menu is a JAStudio menu,** with single-key accelerators shown in front of each item, as in the note menus
(`SpecMenuItem`, `ShortcutFinger.Mnemonic`). Its keys are picked to be remembered: s searchable text, q question,
a answer, t tags, n note type, r studying reading, l studying listening, g group.
- **A text property opens a submenu** of the ways to compare its text: c contains, e is exactly, w matches the
  wildcard pattern, r matches the regular expression. So `+` `s` `c` starts a "searchable text contains" condition,
  and the dialog still lets the way be changed (Magnus, 2026-11-07).
- **A property with only a few values opens a submenu of them, which adds the condition at once,** with no dialog,
  since a dialog only slows the pick down (Magnus, 2026-11-07). The note type's submenu has v vocab, k kanji and
  s sentence, so `+` `n` `k` adds "Note type is KanjiNote". Studying reading and studying listening have t "is
  true" and f "is false". Changing such a condition, with Enter or a double-click, still opens its dialog.
- **The group submenu** adds an "all of" (a) or an "any of" (o) group, and selects it, so the next `+` adds into it.

**A condition's editor is a small dialog** (decided, Magnus, 2026-11-07), not inline in the tree. The tag editor
needs room for its list, and the results need room for their tabs and columns, so an editor opening inline would make
the tree jump around. To keep it as quick as typing:
- it opens next to the tree, with the keyboard in its input;
- Enter adds the condition, and Esc cancels;
- the keyboard goes back to the tree, with the new condition selected;
- it's modal, so the search can't change while it's open;
- the same dialog edits an existing condition, opened by Enter on the condition;
- it's small, sized to its content: one line for text, about ten rows for the tag list.

**Typing a key from the add menu on the tree does what it does in the add menu** (Magnus, 2026-11-07, replacing
an earlier decision that typing a letter starts a searchable-text condition). So `t` opens the tag dialog without
`+` first. `q` opens the whole add menu with the question's submenu open, so it's clear what's happening and another
choice is one key away. A quick lookup is Ctrl+O, `s`, `c`, a word, Enter.

**When a menu closes without a pick, as with Esc, the keyboard goes back to the selected line.** Otherwise it's left
on the window's main menu. When an item is picked, what it does decides where the keyboard goes, such as into the
dialog it opens.

**Keys act on the selected node,** the highlighted line in the tree, which is a condition or a group. Letters
start a searchable-text condition, so the direct keys are other keys:
- **`+` or `Ctrl++`** (Magnus, 2026-11-07) adds a condition to the selected group, or to the group the selected
  condition is in. `+`, `-` and the add menu's keys are recognized by the symbol the key types in the current
  keyboard layout (`KeyEventArgs.KeySymbol`), not by key code, which differs between layouts;
- **Del** removes the selected node, and a group goes with everything in it. The root group can't be removed;
- **`-`** negates the selected node, putting it under "Not", or takes it out again, as Anki's `-tag:x` negates;
- **Enter, or a double-click,** edits the selected condition in its dialog;
- **Ctrl+Z** undoes the last change, since one misplaced Del on a group could lose a whole search;
- **the arrow keys** move through the tree.

**Every action is also in the node's right-click menu,** which the menu key and Shift+F10 open too, with
single-key accelerators like the note menus. Actions without a key of their own live only there, such as switching a
group between "all of" and "any of". Later actions belong there too, such as adding a subgroup, wrapping a node in a
new group, and moving a node within its group.

**The tree shows each condition as a readable line,** such as "Tags include `source::anime::natsume`" or "Not:
studying reading", and each group as "All of" or "Any of".

**The search runs after every change,** since every change leaves a valid search. Scanning takes a few
milliseconds per condition today. Sorting 49,000 rows takes tens of milliseconds (measured with increment 3,
below), so re-sorting the grid after every change looks fine.

**Between changes the results keep showing the same notes** (Magnus, 2026-11-07), until the search is refreshed
with F5 or the Refresh button. Notes don't disappear while one works on them, even when the search would no longer
find them, so a change made by mistake can be undone on the same rows. See increment 3 below.

**Undo** keeps the earlier trees, which is cheap since the tree is small.

**Space:** the condition tree takes height in the pane, so it can be collapsed.

## The tag condition's editor

There's no tag tree in the search (Magnus, 2026-11-07). A filtered list of tags covers what the search needs, and
is quicker from the keyboard than browsing a tree.
- **It matches several parts anywhere in the path.** Typing `nat s3 05` finds `source::anime::natsume::s3::05`, so
  an episode is found without typing its full path.
- **It shows full paths,** perhaps with the parent part dimmed, so similar tags can be told apart.

**What's given up** is seeing how the current results spread over tags, for example which episodes still have
sentences that aren't studied. That's analysis rather than search. If it's missed, it can come back as a feature of
the results, such as counting the results by a tag level.

Browsing and editing the tag tree is tag management, separate from the search. See [tags.md](tags.md).

## The results

- **A grid with a column per value,** sorted by clicking a column header, with columns that can be shown or
  hidden, as in Anki's browser.
- **One tab per note type when several types are found,** as the lists already do, because each type has
  different columns.
- **Candidate columns:**
  - vocab: question, answer, readings, parts of speech, forms, sentence count, studying (reading, listening), tags;
  - kanji: kanji, answer, on readings, kun readings, radicals, studying, tags;
  - sentences: question, answer, source, external id, studying, tags.
- **A sentence's external id** (`SentenceNote.ExternalId`) is the id it had in the deck it was imported from. A
  subs2srs id such as `ny4-nodup_11_0.15.23.540-0.15.25.620` is the deck, episode 11, and the line's start and end
  times. It's sorted with numbers compared as numbers, so episode 2 comes before episode 11, and an episode's
  sentences come in the order their lines are spoken. That's Magnus's "next episode" workflow. On 2026-11-07, 20,041
  of the 49,162 sentences had a subs2srs id, 14,696 had none, and the rest had other formats, such as
  `A_VioletEvergarden_S01_E01_123` and `sentence:1234`, which sort sensibly the same way.
- **Selecting several rows** (Ctrl/Shift+click, select all), right-click actions on the selection (adding and
  removing tags, see [tags.md](tags.md)), double-click or Enter to open, middle-click to open in a background
  tab, as the lists do now.
- **No cap on the number of results** (decided, Magnus, 2026-11-07), until a search proves unmanageably slow. The
  grid only creates the rows on screen.
- **Control:** `Avalonia.Controls.DataGrid` (12.1.2, MIT, for Avalonia 12.1). `TreeDataGrid` is out: since
  October 2025 it needs a commercial Avalonia Accelerate license.

## Where the search lives

**Decided (Magnus, 2026-11-07): in the navigation pane.** It's resizable, so it can be made as wide as the
columns need, and the note's page stays visible beside the results.

**A search can take time to set up, and one doesn't want to lose it to check something else.**
- **Leaning (Magnus, 2026-11-07): one search per workspace, and several workspaces,** rather than several search
  tabs in one workspace. A workspace then keeps a whole context: its search, the results, and the notes opened
  from it. It's step 6 of `jastudio-workspace.md`, so this gives that step a purpose.
- **A note menu's list (today's "List" tab) stays separate** from the search, for now. Mixing the two risks odd
  dependencies between them (Magnus, 2026-11-07).

## Increments (tentative), together with [tags.md](tags.md)

1. **The search model in Core:** the properties and their types, conditions, groups and negation, and matching,
   with specs written as nested calls that indent like the tree. *(Built 2026-11-07, without the operations on the
   tree, which come with the search editor that uses them.)*
   - **The namespaces:** `JAStudio.Core.Note.Search` holds `NoteCondition`, `NoteProperty` and the catalogue
     `NoteProperties`. `Grouping` holds `ConditionGroup`, `AllOfGroup`, `AnyOfGroup` and `NegatedCondition`. Each
     property type has its own namespace: `TextProperties`, `TagProperties`, `TrueOrFalseProperties` and
     `NoteTypeProperties`.
   - **The properties:** question, answer, tags, note type, studying reading and studying listening. Searchable text
     waits for open question 2.
   - **The note type** is a condition on the note's class, such as `NoteType.Is<SentenceNote>()`. `NoteTypes` holds
     Anki's note type names, which the search shouldn't depend on.
   - **`JPCollection.NotesMatching`** finds the notes a search matches, by checking every note in memory.
   - The specs are in `JAStudio.Core.Specifications/AICreatedSpecs/Search/`.
2. **The search editor** in the navigation pane, replacing the current Search tab's text box, in two parts:
   - **2a, a working search editor:** the operations on the tree in Core (add, remove, negate, switch a group), with
     specs; the searchable-text property; the condition tree with the keys above, undo included; the node's
     right-click menu; and a dialog per property type, for text, tags, true-or-false and the note type. The results
     show in today's list, without the cap. *(Built 2026-11-07.)*
     - **Core:** `EditableSearch` holds the search being edited, its operations and undo. A `ConditionPosition`
       finds a condition in the tree. `NoteCondition.ConditionAt` and `WithConditionAt` read and replace one.
       `JPCollection.TagPathsInUse` lists the tag paths the tag dialog offers, parents included.
     - **Searchable text** is `NoteProperties.SearchableText`. Each note type says which of its texts are searchable
       (`JPNote.SearchableTexts`). A `TextProperty` holds one or more texts and matches when any of them does, so
       "text" and "list of texts" turned out to be one property type.
     - **A negation shows on the line of what it negates,** such as "Not: Tags include x" or "Not: All of:".
     - **The UI:** the condition tree is `NoteSearchViewModel`, each line a `SearchConditionNodeViewModel`. The
       dialog is `SearchConditionDialog`, with an editor view model per property type in
       `ViewModels/Workspace/Navigation/Search/`. Menus and dialogs open just below the condition tree.
     - **An empty search lists no notes in the UI,** although the model's empty search matches every note. Listing
       every note before anything is searched would be slow and of no use.
     - **Not yet:** dimming the parent part of the tag paths in the tag dialog.
   - **2b, the rest:** wrapping a node in a new group, moving a node within its group, and collapsing the tree.
     Adding groups moved into 2a (Magnus, 2026-11-07).
3. **The results grid** with sortable columns, selecting several rows, and the external id. *(First version built
   2026-11-07.)*
   - **It replaces the results list in the Search tab only.** The note menus' List tab keeps its list
     (`ListedNotesView`).
   - **The UI:** `SearchResultsViewModel` holds the results, in a `SearchResultsOfOneTypeViewModel<TSearchResult>`
     per note type, each row a `SearchResultViewModel` subclass with the type's columns
     (`VocabSearchResultViewModel`, `KanjiSearchResultViewModel`, `SentenceSearchResultViewModel`). The view is
     `SearchResultsView`, with a `DataGrid` per type. Only the shown type's grid is visible, so each keeps its own
     sort and selection.
   - **The columns, fixed for now:**
     - vocab: question, readings, answer, studying reading, studying listening;
     - kanji: kanji, answer, on readings, kun readings, studying reading, studying listening;
     - sentences: question, answer, source, external id, studying reading, studying listening.
   - **Sorting is the grid's own** (Magnus, 2026-11-07: no special sorting while this is built in). Clicking a
     header sorts by that column alone. Shift+click adds a column that sorts the rows the earlier columns leave
     equal, and Ctrl+click stops sorting. A grid starts out sorted by the question (Magnus, 2026-11-07). The
     source and the external id compare their numbers as numbers (`TextSortedWithNumbersAsNumbers`).
   - **The grid keeps its sort when the search changes:** the rows are replaced in the same collection
     (`ReplaceableObservableCollection`), with one notification, and the grid sorts the new rows the same way.
   - **Selecting and opening:** Ctrl/Shift+click and Ctrl+A select several rows. Enter or a double-click opens
     the note, and middle-click opens it in a background tab.
   - **The selection's right-click menu,** also on the menu key: "Unsuspend all cards" and "Suspend all cards"
     for every selected note, run in the background with a progress dialog. Suspending reading and listening
     separately waits for [studying.md](studying.md). Adding and removing a tag came with increment 4.
   - **The results keep showing the same notes, kept up to date** (Magnus, 2026-11-07). The first version ran
     the search again after the selection's menu, which dropped the changed notes from under him and cleared the
     selection, so a mistake couldn't be undone on the same rows.
     - **Each row shows its note as it is now.** Its values are read from the note, and it's told to read them
       again when the note is updated (`JPCollection.NoteUpdated`), whatever updated it: the editor section or a
       note menu. The rows stay the same objects, so the selection stays. The grid doesn't re-sort a row whose
       values changed, so it stays where it is.
     - **The studying status has a notification of its own** (`JPCollection.StudyingStatusChanged`). It's held in
       memory, not stored with the note, so suspending or unsuspending updates no note, and `NoteUpdated` doesn't
       report it. Found 2026-11-07: the rows didn't show the selection menu's changes, unless Anki's browser had
       the note open, since its refresh saved the note on Anki's side, and that save was reported as an update. The
       new notification is raised when JAStudio suspends or unsuspends a note's cards, and when Anki reports a
       change to a card (`JPCollection.UpdateCardStudyingStatus`). The add-on reports Anki's own suspends and
       unsuspends, such as those in its browser, through a patch of Anki's scheduler that is put in place when a
       card is first shown (`clear_studying_cache_on_card_suspend_unsuspend.py`), so not before.
     - **Refreshing,** with F5 anywhere in the Search tab or the Refresh button, runs the same search again. The
       rows of the notes still found stay, where they are and selected if they were, and read their values again,
       so a refresh shows every change, even one no notification reported. The rows of the notes no longer found
       go, and the newly found notes get rows. Changing the search still replaces all the rows.
     - **After an Anki sync,** the collection is reloaded and every note object replaced
       (`JPCollection.ReloadedFromBackend`). Each row then shows the object with its note's id, and the rows of
       notes that no longer exist go. Before this, rows kept the discarded objects, and a menu action on them would
       have flushed a discarded note.
     - **Not followed until a refresh:** a note deleted from a note menu keeps its row.
     - **A refresh that adds or removes many rows is slow,** since each row is added or removed on its own and the
       grid finds each one's place. A few hundred is no concern. Tens of thousands would probably take seconds (not
       measured).
   - **Measured (2026-11-07),** on the 49,162 sentence texts, outside the grid: sorting by question took about
     25 ms, sorting by external id about 60 ms, and stripping the markup from them about 5 ms. The grid's own
     overhead isn't measured.
   - **Not yet:** a tags column, showing and hiding columns, and the studying-add-on's other card states.
4. **Tagging notes** ([tags.md](tags.md), increment 1). *(Adding and removing a tag on the selected results built
   2026-11-07.)*
5. **Tag management:** browsing and editing the tag tree ([tags.md](tags.md), increments 2 and 3).
6. **Several workspaces** (`jastudio-workspace.md`, step 6), if that's how several searches are kept open.

## Maybe later: a text form

Added only if it's missed. The tree editor may well make it unnecessary.
- **What text would give:** typing speed for well-known searches, pasting and sharing, and a one-line summary of a
  search, for a history list or a window title. A one-line summary only needs a formatter (tree to text), which is
  small.
- **The syntax would be Anki's,** which Magnus knows and which handles `tag:a::b` without escaping:
  - terms separated by spaces must all match;
  - `or` combines alternatives, and binds looser than AND;
  - `-` negates;
  - parentheses group;
  - quotes make a term of text with spaces;
  - `key:value` splits at the first colon that isn't escaped;
  - `*` and `_` are wildcards;
  - `re:` takes a regular expression.

  It would come with a hand-written recursive-descent parser for exact error positions.
- **Text and tree together** would need the tree to show every search the text can express. Text written from
  the tree would be normalized, but only when the tree is edited, never while typing. While the text has a syntax
  error, the tree keeps the last valid search.

## Research: can we use something that exists?

Done 2026-11-07, when a text syntax was still planned.

**Lucene-style query parsers.** Lucene's query syntax is the most widespread search-box language.
`Foundatio.Parsers.LuceneQueries` (8.0.2, Apache-2.0) parses it into a syntax tree that visitors can walk. Tested
in a scratch project:
- `tag:source::anime::natsume` fails to parse ("Unexpected character ':'"), so hierarchical tags have to be quoted
  or escaped.
- A lowercase `or` is a search word, not an operator.

**Programmer-oriented filter syntaxes,** such as Dynamic LINQ, OData's `$filter` and Gridify, aren't meant to be
typed into a search box many times a day. **Parser libraries** (Superpower, Pidgin, Parlot) are tools for
building a parser. **Anki's syntax:** no .NET implementation found in a brief search. Anki's own parser is Rust
and AGPL-3.0.

**Whole search engines** (Lucene.NET being the obvious one) would do the matching once an index is filled from
the notes. A plain in-memory scan over ten copies of the sentences (about 490,000) took 40 to 95 ms per
condition:
- substring in question or answer: 90 ms;
- tag branch with a negated tag: 40 ms;
- regex: 65 to 95 ms.

Today's corpus is a tenth of that, so an engine's main benefit, speed at scale, isn't needed. Its costs:
- a second copy of every note, kept in step with every edit, flush and reload;
- word-based matching, which Japanese text, having no spaces, needs a Japanese analyzer or character n-grams for;
- regex on single indexed words, in Lucene's own dialect;
- the domain properties (tag branches, studying state, the vocab a sentence contains) still computed into the
  index by us.

## Open questions

*Answered 2026-11-07:* `source:` in Magnus's searches meant the source tags; the search lives in the navigation
pane; results are notes; conditions are on typed note properties; matching is our own, in memory, in Core; the
note menus' lists stay separate; the search is built in a tree editor, with no text syntax at first; in code,
conditions come from factory methods where those read better, and from the property for a condition on it.

1. **Several workspaces instead of several search tabs?** Magnus's leaning. See
   [Where the search lives](#where-the-search-lives).
2. ~~Which fields are searchable text?~~ *Answered 2026-11-07:* exactly what the old search box covered, with HTML
   stripped:
   - vocab: question, answer, readings, readings in romaji, forms;
   - kanji: question, answer, readings, readings in romaji;
   - sentences: question and answer.

   The old box's `r:` (readings only) becomes a readings property later.
3. **Which of Anki's left-out searches does Magnus use?** For example new cards, or cards answered recently. Each
   would need its data loaded from the studying-add-on, as a property of the note.
