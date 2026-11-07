# Tags — Specification

> **Status: draft (2026-11-07).** The direction is agreed with Magnus. It's modeled on Anki's tags, which Magnus
> considers well designed, with the deliberate differences listed below. Details are settled as each increment
> needs them. Searching, including searching by tags, is specified in [note-search.md](note-search.md).
>
> **The editing mechanisms and keyboard shortcuts below are preliminary.** Anki is the reference for what can be
> done with tags. Matching its UI style or its keyboard shortcuts is not a goal in itself (Magnus, 2026-11-07).

## Why

Organizing in JAStudio is built on hierarchical tags. They can do most of what folders, tags and decks do in
other programs. Today tags can only be organized in Anki, and Anki's tag operations don't even reach the corpus
(`corpus-data-model.md`, "Anki's role and its consequences"). The goal is parity with Anki in what can be done,
not necessarily in how:
- navigating the tag tree;
- editing the tree;
- tagging and untagging one note or many at once.

## Terms

- **tag** — a label on a note, such as `source::anime::natsume::s3::01`. A note's tags are stored in its corpus
  file's `tags` list and held in memory by `NoteTags`.
- **tag path** — a tag's full name. `::` separates its levels: `source` is the parent of `source::anime`.
- **branch** — a tag together with all the tags below it. `source::anime` and `source::anime::natsume` are both
  in the `source::anime` branch.
- **tag tree** — the tree all tag paths form. It's computed from the notes' tags, so a tag no note has isn't in
  it.
- **corpus-tag** and **student-tag** — a tag that describes the language data, and a tag that is about one
  student's studying, such as a priority tag. See `corpus-data-model.md`, "Student-tags and corpus-tags".
- **jastudio-defined-tag** — a tag that JAStudio itself defines, in `Tags`. Some are generated, such as
  `-::kanji::in-any-vocab-form`. Others hold configuration that has its own editors, such as
  `-::vocab::matching::requires::godan`. Tags a person applies, including the ones under `-::` such as
  `-::priority::…` and `-::counting`, are not jastudio-defined-tags.

## Rules

- **No spaces in a tag.** Anki separates tags with spaces, so a tag with a space would become two tags in Anki.
  No tag in the corpus has one today.
- **Tags are case-insensitive, as in Anki.** Adding a tag that differs from an existing one only in case reuses
  the existing spelling. So JAStudio can't create two tags that Anki would treat as one. No two tags in the
  corpus differ only in case today. *(Done 2026-11-07: `Tag.FromName` ignores case.)*
- **No empty levels:** `a::::b`, a leading `::` and a trailing `::` are invalid.
  *(2026-11-07: `Tag.WhyItCannotBeTheNameOfATag` checks a typed name against this rule and the one on spaces. Only
  typed names are checked: tags loaded from the corpus or Anki aren't.)*
- **jastudio-defined-tags can't be edited by hand.** They appear in the tag tree for navigating, but can't be
  renamed, moved, deleted, or added to or removed from notes by hand. They're either generated, so a hand edit
  would be overwritten, or they have their own editors. *(2026-11-07: `Tags.IsDefinedByJAStudio` says which they
  are: every tag declared in `Tags` and its nested classes.)*
- **Every tag change writes the note** to its corpus file, and through the Anki-projection to its Anki note, as
  any other edit does.

## Tag management: browsing the tag tree

**Tag management is its own view, separate from the search** (Magnus, 2026-11-07). In the search, a tag condition
is added by typing parts of its path, which covers what the search needs ([note-search.md](note-search.md)). A tag
tree in the search would mostly add complexity. It's a "Tags" tab in the navigation pane (Magnus, 2026-11-07).

| Anki | JAStudio |
|---|---|
| The sidebar's tag tree, with an "Untagged" item | The same, but **no "Untagged" item for now** (Magnus, 2026-11-07): it needs a search condition for notes with no tags. |
| The tree shows every tag, with no counts | **Each tag shows how many notes are in its branch.** The corpus is in memory, so counts are cheap. |
| A filter box above the sidebar finds tags | The same. |
| Clicking a tag searches for it | **A "search for this tag" action** replaces the workspace's search with a condition on that tag, as Anki does (Magnus, 2026-11-07). The search's undo goes back to the search before. |
| Expand and collapse, per tag or for its children | The same. |

The tree has 368 nodes today (330 distinct tags plus their parents), so its size is no concern.

## Tag management: editing the tag tree

| Anki | JAStudio |
|---|---|
| **Rename** (F2, inline): changes the tag and its children on every note | The same. |
| **Rename with parents:** edits the full path in a dialog, which moves the tag | The same. |
| **Drag and drop** onto another tag, or onto the root, moves tags, several at once | The same. |
| **Delete** (Del): removes the tag and its children from every note, with no confirmation, because Anki has undo | The same, **with a confirmation** that names how many notes change. |
| Renaming onto an existing tag merges the two | The same, **with a very clear warning**: after a merge, the notes of the two tags can't be told apart, so renaming back doesn't undo it. |
| **Clear unused tags** | Not needed: the tag tree is computed from the notes. |
| **Find and replace** on tags, with regex | Deferred. |

**Why confirm:** JAStudio has no undo. Git in `src/jas_database` can restore the corpus files, but a restore is
far more work than a confirmation. Renaming, moving and deleting all confirm when they change more than one
note, for example "Rename `source::anime` to `source::tv`? 28,693 notes change."

## Tagging notes

| Anki | JAStudio |
|---|---|
| Tag chips under the note editor's fields, with autocomplete. Ctrl+Shift+T jumps there. | **A Tags editor in every note type's editor section:** the tags as chips, each with a × that removes it, and a button that adds a tag picked in the tag picker dialog. It's on a vocab's Metadata tab, and at the bottom of the kanji and sentence editors. It leaves out jastudio-defined-tags, for now (Magnus, 2026-11-07). **No key jumps there:** Ctrl+T and Ctrl+Shift+T add and remove a tag on a note's tab directly (Magnus, 2026-11-07). |
| With notes selected: Add Tags (Ctrl+Shift+A) and Remove Tags (Ctrl+Alt+Shift+A) open a prompt with autocomplete | **The same on the search results, which allow selecting several notes** (Ctrl/Shift+click, Ctrl+A). Right-click a selection → Add tags… / Remove tags…. Remove offers only the tags the selected notes have. *Shortcuts (Magnus, 2026-11-07): Ctrl+T adds and Ctrl+Shift+T removes, rather than Anki's, which are hard to remember: T for tag, as everywhere in the search, and Shift for the reverse.* |
| Right-click tags in the sidebar → Add to / Remove from selected notes | Deferred. The Add and Remove prompts cover it. |
| Toggle Mark (Ctrl+K) adds or removes `marked` | Skipped: `marked` is on 2 notes, and it's a student-tag. |

## Tags and Anki

- **Today** the Anki-projection writes each note's whole tag list to its Anki note, and saving a note in Anki
  merges the Anki note's tag list back into the corpus. Anki's own tag operations skip that merge, and JAStudio's
  next write of the note undoes them. See `corpus-data-model.md`.
- **Once JAStudio can edit tags,** tags are edited only in JAStudio, and the Anki-ingestion stops taking tags back,
  except when importing notes, which takes all their tags along.
- **Student-tags** such as priority are still in the corpus today, so they behave like any other tag. When they
  move out of the corpus and are read from Anki instead, the tag tree should still show them. How is decided
  then.

## Later: branches with meaning, and tags with metadata

Magnus (2026-11-07):
- **Separate branches will probably decide a tag's kind.** That covers whether it's a corpus-tag or a student-tag,
  and whether it's written to Anki, rather than a list of exceptions.
- **Tags will probably get metadata,** such as a description, rather than being only strings.

## Costs

- **Bulk changes write each affected note one at a time,** to its corpus file and its Anki note, as the "Force
  flush all Anki notes by ID" batch does. Renaming `source::anime` touches about 28,700 sentences, so it runs
  with a progress dialog. That's the simple version. Making the Anki writes faster is for later, and only if it's
  too slow.

## Increments (tentative)

How these interleave with the search increments is in [note-search.md](note-search.md).

1. **Tagging notes:** the Tags editor in the editor section, and Add tags / Remove tags on selected search results.
   - **1a, built 2026-11-07: adding and removing a tag on the selected search results** (Magnus: the bulk case first).
     - **The selection's right-click menu** has "Add a tag..." (Ctrl+T) and "Remove a tag..."
       (Ctrl+Shift+T). Each opens a small dialog (`TagPickerDialog`) that picks one tag,
       and then changes each selected note in the background, with a progress dialog, as suspending does
       (`ActionsOnSelectedResults`). The rows show the change, and stay.
     - **The dialog's tag picker** (`TagPickerViewModel`) is the one the search's tag condition uses, moved out of
       it: typing parts anywhere in the path lists the matching tags, and the first is picked.
     - **Adding** offers every tag some note has (`JPCollection.TagsInUse`), and the typed text itself, listed last
       as "(a new tag)", when it can be the name of a tag no note has. Typing parts separated by spaces finds
       existing tags, since a tag has no spaces. A new tag that differs from an existing one only in case gets the
       existing spelling.
     - **Removing** offers only the tags the selected notes have.
     - **Neither offers a jastudio-defined-tag.**
     - **One tag at a time,** unlike Anki's prompt, which takes several. That's the simple version.
   - **1b, built 2026-11-07: seeing and editing a note's tags.**
     - **A Tags column in the search's results grid,** for every note type, so a bulk tagging shows in the rows it
       changed. It sorts with the numbers in the tags compared as numbers.
     - **A Tags editor in the editor section** (`NoteTagsEditorViewModel`): the note's tags as chips, each with a ×
       that removes it at once, and an "Add a tag... (Ctrl+T)" button that opens the tag picker dialog.
     - **Ctrl+T and Ctrl+Shift+T on a note's tab** add a tag to the note it shows, or remove one, through the same
       dialog (`OneNoteTagging`). They're window shortcuts, so they work on the page and in the editor section. So
       Ctrl+T always adds a tag to what's being looked at: on the results grid, which binds the same keys for
       itself, to the selected notes.
     - **A single note is changed at once,** on the UI thread, as the other editors change it, with no progress
       dialog, which would only flash for one note.
     - **The column and the editor leave out the jastudio-defined-tags,** as the dialogs do
       (`NoteTags.NotDefinedByJAStudio`). Otherwise a vocab's matching-configuration tags would crowd out the
       rest, and they have editors of their own.
2. **Tag management, browsing:** the tag tree with counts, the filter box, and "search for this tag". *(Built
   2026-11-07.)*
   - **Core:** `TagTree` is the tree, made by `JPCollection.TagTree`, each path a `TagTreeNode` with its last level,
     its children and the number of notes in its branch. A note with several tags in a branch counts once there.
     Siblings are in alphabetical order with the numbers in them compared as numbers, so `s2` comes before `s10`.
     `Tag.PathsOfTheBranchesItIsIn` lists the paths of a tag's branches, which `TagPathsInUse` uses too.
   - **The filter box** uses the tag picker's matching (`TypedTagPathParts`): the tree keeps the paths that contain
     every typed part, everything below them, and their parents (`TagTree.WithOnlyThePathsMatching`). A parent
     keeps the count of its whole branch. While there's a filter, the lines above the matches are expanded, so the
     matches show, collapsed. Clearing it brings back the lines expanded before.
     - **Not every match is expanded** (Magnus, 2026-11-07): the paths below a match all match too, since they
       contain its path, so filtering for `anime` expanded the whole anime branch, which was hard to navigate. Now it
       shows `source` expanded and `anime` collapsed. Ctrl+Right expands a match's whole branch.
   - **Searching for a tag:** Enter, a double-click, or the line's right-click menu. It replaces the whole search
     with "Tags include" the path, in an "all of" group, so conditions added later narrow it down
     (`EditableSearch.ReplaceTheWholeSearchWith`), and shows the Search tab.
   - **Counted when shown:** the tree is computed from the notes' tags each time the tab is shown, and on F5 or the
     Refresh button. It isn't kept up to date while shown. The expanded lines and the selected line stay. Computing
     it hasn't been measured on the real collection.
   - **The UI:** `TagManagementViewModel`, each line a `TagTreeNodeViewModel`, and `TagManagementView`, in
     `Navigation/TagManagement/`. Down in the filter box moves the keyboard to the tree. The tree's own keys expand
     and collapse: Right and Left a line, Ctrl+Right and Ctrl+Left its whole branch. A double-click on a line with
     lines below it would expand or collapse it too, so the line's content takes the double-click first.
   - **Not yet:** an "Untagged" item, and a key that opens the tab.
3. **Tag management, editing:** rename, rename with parents, move by drag and drop, delete, all with
   confirmation.

## Related

- **Studying actions on a selection,** such as suspending a selection's reading cards, belong to studying, not to
  tags. See [studying.md](studying.md).

## Noticed

- ~~`Tag.Bit` (`1L << id`) is unused, and it's wrong past 64 tags; the corpus has 330.~~ *Deleted 2026-11-07, with
  `Tag.Id` and `Tag.FromId`, which were unused too.*
