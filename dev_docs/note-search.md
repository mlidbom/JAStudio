# Note Search — Language, Editor and Results

> **Status: research and proposal (2026-11-07).** Only Magnus's requirements and the points marked as decided are
> settled. Tags themselves are specified in [tags.md](tags.md).
>
> **The search box's mechanisms and keyboard shortcuts are preliminary.** Matching Anki's UI style or its
> shortcuts is not a goal in itself (Magnus, 2026-11-07).

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
- **Regular expressions** too, unless that turns out unfeasible.
- **Studying state,** such as whether a note's reading or listening is being studied.
- **A search box** that helps write such searches.
- **Results with a column per sortable value,** sorted by clicking a column.
- **No cap on how many notes a search lists,** until a search proves unmanageably slow.
- **The search lives in the navigation pane,** which is resizable, so it can be made as wide as needed.
- **Several searches open at once,** as tabs in the navigation pane, designed in from the start. A search can take
  time to set up, and one doesn't want to lose it to check something else.
- **The tag tree** in the same place as the search.
- **Results are notes.** The corpus has no concept of cards.

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
| `studying-reading:true`, `studying-listening:false` (Magnus's proposal) | Notes whose reading, or listening, is being studied: the note is in the studying add-on (today Anki) and that card isn't suspended | Replaces Anki's `is:suspended` and `card:`, which are about cards. Whether boolean keys use `key:true/false` or an `is:` form is open question 1. See [studying.md](studying.md). |
| `note:vocab`, `note:kanji`, `note:sentence` | Notes of that type | Anki's `note:` takes a note type's name, so this keeps the habit. |
| `question:…`, `answer:…`, `reading:…` and other field names | Notes whose field matches. **Exact by default**, as in Anki: `question:*行く*` for "contains". `question:` with no value matches an empty field, `question:_*` a non-empty one. | The field names per note type are to be decided. |
| `id:…` | Notes with those JAStudio note ids | Anki's `nid:` takes Anki ids. |
| JAStudio-specific keys (later) | For example `vocab:…` (sentences containing a vocab), `kanji:…`, `form:…`, `pos:…` | Each one when a workflow needs it. `vocab:` fits Magnus's habit of adding sentences that contain the vocab he's studying. |

**Anki keys left out,** because they're about Anki's cards and scheduling, which JAStudio doesn't load: `deck:`,
`flag:`, `prop:`, `rated:`, `added:`, `edited:`, `introduced:`, `card:`, and the `is:` card states. Studying state
that matters can be added as keys about notes, as the two studying keys above are, without tying the search to
Anki ([studying.md](studying.md)).

**Regular expressions** (`re:`) work wherever text is matched: alone, after a field key, and after `tag:`. They
use .NET's regex dialect, which differs from Anki's Rust dialect only in details.

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
- **No cap on the number of results** (decided, Magnus, 2026-11-07), until a search proves unmanageably slow. The
  grid only creates the rows on screen. Whether sorting tens of thousands of rows stays quick is to be measured.
- **Control:** `Avalonia.Controls.DataGrid` (12.1.2, MIT, for Avalonia 12.1). `TreeDataGrid` is out: since
  October 2025 it needs a commercial Avalonia Accelerate license.

## Where the search lives

**Decided (Magnus, 2026-11-07): in the navigation pane.** It's resizable, so it can be made as wide as the
columns need, and the note's page stays visible beside the results.

**Several search tabs in the pane, designed in from the start.** Each search tab has its own search, results and
sort order, so a carefully built search isn't lost to check something else. How the tag tree fits in (one per
search tab, or one that edits the shown search tab's search) is open, as is whether a note menu's list (today's
"List" tab) becomes a search tab too.

## Increments (tentative), together with [tags.md](tags.md)

1. **The search language in Core:** the parser, and matching for bare text, `re:`, `tag:`, `note:`, the studying
   keys, the field keys, `-`, `or` and grouping, with specs. The current Search tab uses it, without the result
   cap, and the error message shows under the box.
2. **The results grid** with sortable columns, selecting several rows, and the place in the source; and several
   search tabs in the navigation pane.
3. **The tag tree** beside the search, with counts. Clicking and modifier-clicking edit the search
   ([tags.md](tags.md), increment 1).
4. **Tagging notes** ([tags.md](tags.md), increment 2).
5. **Editing the tag tree** ([tags.md](tags.md), increment 3).
6. **The search box's help:** autocomplete, then coloring and history.

## Open questions

*Answered 2026-11-07:* `source:` in Magnus's searches meant the source tags (`tag:source::…`); the search lives in
the navigation pane; results are notes, so there's no card-level matching.

1. **Boolean keys: `key:true/false`, or an `is:` form** such as `is:studying-reading`? Under discussion.
2. **A parser library, or a whole search engine, or our own parser?** Under discussion. Magnus (2026-11-07): the
   need to quote or escape `::` doesn't by itself seem a big deal. What he had in mind was something that does
   the entire search, once it's populated and configured from the notes.
3. **Which fields are searchable text** for bare-text searches, per note type? Anki searches every field. For
   JAStudio, the question and answer at least, and perhaps readings, forms and comments.
4. **Which of Anki's left-out keys does Magnus use?** For example `is:new` or `rated:`. Each would need its data
   loaded from the studying add-on.
