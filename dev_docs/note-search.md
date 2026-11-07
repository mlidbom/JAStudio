# Note Search — Language, Editor and Results

> **Status: research and proposal (2026-11-07). Nothing here is decided** except Magnus's requirements below.
> Tags themselves are specified in [tags.md](tags.md).

## Why

Magnus constantly uses complex Anki searches to find the right set of notes, for example
`tag:… -tag:… is:suspended`, with substring matches and boolean conditions. JAStudio's search can't do that.
It's the "Search" tab of the navigation pane (`NoteSearchViewModel`):
- it splits the text on ` && `;
- `r:` restricts a condition to reading fields;
- everything else is a substring match on the question, answer and similar fields;
- it lists at most 100 notes of each type.

Magnus (2026-11-07): the tag tree's tab should become a full search and navigation tab, with a search language
and a search box that can handle such searches. The results should be a list with a column per sortable value,
not today's two-line rows.

## Requirements (Magnus)

- **Boolean conditions** (and, or, not, grouping), **substring matches**, and **tag searches** that understand
  the tag hierarchy.
- **Card state,** at least `is:suspended`.
- **A search box** that helps write such searches.
- **Results with a column per sortable value,** sorted by clicking a column.
- **The tag tree** in the same place as the search.

## Research: can we use something that exists?

**Lucene-style query parsers.** Lucene's query syntax is the most widespread search-box language: `field:value`,
`AND`/`OR`/`NOT`, `-`, quotes, wildcards, parentheses and ranges.
- `Foundatio.Parsers.LuceneQueries` (8.0.2, Apache-2.0) parses it into a syntax tree that visitors can walk. That
  is the shape we'd want.
- **Tested 2026-11-07 on Magnus's kind of query, in a scratch project:**
  - `tag:source::anime::natsume` **fails to parse** ("Unexpected character ':'"). Hierarchical tags have to be
    quoted (`tag:"source::anime"`) or escaped (`tag:source\:\:anime`).
  - A lowercase `or` is a search word, not an operator. Lucene's operators are uppercase.
- Newer successors exist (`Foundatio.LuceneQuery`, `Foundatio.LuceneQueryParser`). I haven't tested them, but
  they parse the same Lucene syntax.
- Lucene.NET's own query parser has the same syntax and is tied to Lucene indexes.

**Programmer-oriented filter syntaxes,** such as Dynamic LINQ (`System.Linq.Dynamic.Core`), OData's `$filter`
and Gridify: expressions like `Tags.Contains("x") && !IsSuspended`. They're powerful, but nothing one wants to
type into a search box many times a day.

**Parser libraries** (Superpower, Pidgin, Parlot) are tools for building a parser, not ready languages.

**Anki's syntax:** I found no .NET implementation in a brief search. Anki's own parser is in Rust, in Anki's
source, and is AGPL-3.0. It's a reference for how Anki behaves, not code to copy.

**Conclusion:** a library could parse a search, but its syntax is the wrong one for hierarchical tags, and it
isn't the syntax Magnus already types every day. The parser is the small part of the work. The real work is
deciding the search keys and what each one matches, and no library does that for JAStudio's notes.

## Recommendation: Anki's syntax, with JAStudio's own keys

- **Use Anki's syntax.** Magnus knows it, it's designed for exactly this kind of search, and `tag:a::b` works
  without escaping. Where a key means the same in JAStudio, it keeps Anki's name and behavior. JAStudio adds
  keys of its own, and leaves out the Anki keys whose data JAStudio doesn't have.
- **The parser and the matching live in Core,** as a domain concept: a parsed search is a tree of typed search
  terms, and each term decides whether a note matches. This also moves today's search logic out of the UI, which
  was already planned.
- **The parser is hand-written,** a small recursive-descent parser over a tokenizer. The grammar below is small,
  and a hand-written parser gives exact error positions for the search box to mark. Superpower is the fallback if
  that turns out wrong.

### Syntax (draft, following Anki's manual)

- **Terms separated by spaces must all match** (implicit AND). `and` may be written out. `or` combines
  alternatives, and AND binds tighter than OR. Both are case-insensitive words.
- **`-` negates** a term or a group: `-tag:x`, `-(a or b)`.
- **Parentheses** group: `dog (cat or mouse)`.
- **Quotes** make one term of text with spaces: `"a dog"`, `question:"a dog"`, `"question:a dog"`.
- **`key:value`** splits at the first colon that isn't escaped, so `tag:source::anime` is the key `tag` and the
  value `source::anime`.
- **Wildcards:** `*` matches any run of characters, `_` exactly one.
- **Escapes:** `\"`, `\*`, `\_`, `\\`, `\:`, `\(`, `\)`, and `\-` at the start of a term.
- **`re:`** takes a regular expression, after a key (`question:re:…`, `tag:re:…`) or alone.

### Keys (draft)

| Key | Matches | Notes |
|---|---|---|
| bare text, such as `行く` | Notes whose searchable text contains it, case-insensitively | Which fields count as searchable text, per note type, is to be decided. In Anki it's every field except tags. |
| `tag:x` | Notes with tag `x` or a tag in its branch. `tag:x*` matches by prefix, `tag:none` matches notes with no tags, `tag:re:…` matches by regex. | As in Anki. See [tags.md](tags.md). |
| `is:suspended` | Notes with a suspended card | JAStudio knows, per card, its card type and whether it's suspended (`CardStudyingStatus`). It doesn't load Anki's other card states. |
| `card:reading`, `card:listening` | Notes with a card of that type | Combined with `is:suspended` it must apply to the same card: `card:listening is:suspended` means the listening card is suspended. That's how Anki behaves, and it needs card-level matching. To be designed. |
| `note:vocab`, `note:kanji`, `note:sentence` | Notes of that type | Anki's `note:` takes a note type's name, so this keeps the habit. |
| `question:…`, `answer:…`, `reading:…` and other field names | Notes whose field matches. **Exact by default**, as in Anki: `question:*行く*` for "contains". `question:` with no value matches an empty field, `question:_*` a non-empty one. | The field names per note type are to be decided. |
| `id:…` | Notes with those JAStudio note ids | Anki's `nid:` takes Anki ids. |
| `source:…` (proposed) | Shorthand for `tag:source::…` | Open question 1. |
| JAStudio-specific keys (later) | For example `vocab:…` (sentences containing a vocab), `kanji:…`, `form:…`, `pos:…` | Each one when a workflow needs it. `vocab:` fits Magnus's habit of adding sentences that contain the vocab he's studying. |

**Anki keys left out,** because JAStudio doesn't load that data: `deck:`, `flag:`, `prop:`, `rated:`, `added:`,
`edited:`, `introduced:`, `is:new`, `is:due`, `is:learn`, `is:review` and `is:buried`. Anki's database has all of
them, so any one can be added later by loading it, as the studying status already is.

**Maybe later:** `w:` (whole words) and `nc:`, which ignores accents and, usefully for Japanese, voicing marks:
`nc:は` matches は, ば and ぱ.

## The search box

- **Searches run on Enter,** as in Anki, not on every keystroke.
- **A search with a syntax error** shows the error, and marks where it is in the text.
- **Autocomplete** for keys after typing a word start, for tags after `tag:`, and for the values of `is:`,
  `card:` and `note:`.
- **Later:** coloring of keys, operators and quoted text, and a history of recent searches (Anki's search box
  has one).
- **Implementation:** a plain text box with an autocomplete popup first. AvaloniaEdit has syntax coloring and a
  completion window, if a plain text box isn't enough; I haven't checked its support for Avalonia 12.

## The results

- **A grid with a column per value,** sorted by clicking a column header, with columns that can be shown or
  hidden, as in Anki's browser.
- **One tab per note type when several types are found,** as the lists already do, because each type has
  different columns.
- **Candidate columns:**
  - vocab: question, answer, readings, parts of speech, forms, sentence count, studying (reading, listening), tags;
  - kanji: kanji, answer, on readings, kun readings, radicals, studying, tags;
  - sentences: question, answer, source, place in the source, studying, tags.
- **The place in the source** is the subs2srs id (`externalId`, for example
  `ny4-nodup_11_0.15.23.540-0.15.25.620`), sorted with numbers compared as numbers, so episode 2 comes before
  episode 11. That's Magnus's "next episode" workflow.
- **Selecting several rows** (Ctrl/Shift+click, Ctrl+A), right-click actions on the selection (adding and
  removing tags, see [tags.md](tags.md)), double-click or Enter to open, middle-click to open in a background
  tab, as the lists do now.
- **No 100-note cap:** the grid only creates the rows on screen. Whether sorting tens of thousands of rows stays
  quick is to be measured.
- **Control:** `Avalonia.Controls.DataGrid` (12.1.2, MIT, for Avalonia 12.1). `TreeDataGrid` is out: since
  October 2025 it needs a commercial Avalonia Accelerate license.

## Where the search lives (open)

A grid with useful columns needs width, and the navigation pane is a narrow column on the left of the note's page.

- **A. In the navigation pane, as Magnus described:** the search box, the tag tree and the results in one tab of
  the pane. The note's page stays visible beside the results. The pane is dragged wider when more columns are
  wanted, which narrows the page.
- **B. As a kind of workspace tab, beside the note tabs:** the full width for the grid, and several searches open
  at once, each in its own tab. The tag tree could sit inside the search tab, or stay in the navigation pane and
  drive the shown search tab's query, the way Anki's sidebar drives its browser. Seeing a result's page means
  switching tabs, unless the search tab also shows the selected note's page below the grid, as Anki's browser
  can.

My lean is B, for the width and the several searches. But A is what Magnus described, and it is simpler to
build. Open question 2.

## Increments (tentative), together with [tags.md](tags.md)

1. **The search language in Core:** the parser, and matching for bare text, `tag:`, `note:`, `is:suspended`, the
   field keys, `-`, `or` and grouping, with specs. The current Search tab uses it, and the error message shows
   under the box.
2. **The results grid** with sortable columns, selecting several rows, and the place in the source.
3. **The tag tree** beside the search, with counts. Clicking and modifier-clicking edit the search
   ([tags.md](tags.md), increment 1).
4. **Tagging notes** ([tags.md](tags.md), increment 2).
5. **Editing the tag tree** ([tags.md](tags.md), increment 3).
6. **The search box's help:** autocomplete, then coloring and history.

## Open questions

1. **What does `source:` mean** in Magnus's searches? None of the Anki note types has a field named `source`. Is
   it shorthand for `tag:source::…`? If so, should JAStudio have a `source:` key that means exactly that?
2. **Where does the search live:** A (the navigation pane) or B (a workspace tab)? See above.
3. **Which fields are searchable text** for bare-text searches, per note type? Anki searches every field. For
   JAStudio, the question and answer at least, and perhaps readings, forms and comments.
4. **Card-level matching for `card:` with `is:`.** Is matching per card, as in Anki, needed from the start, or is
   note-level matching enough at first?
5. **Which of Anki's left-out keys does Magnus use?** For example `is:new` or `rated:`. Each needs its data loaded
   from Anki.
