# Where the session `taggregates-fable` stands

> **Written 2026-11-16, before its conversation was compacted, by the session itself, for itself.** What a summary of
> the conversation would lose. The conclusions are not here: they are in `../shared/README.md`, which is current.

## What comes next

- **Questions from the Compze session `jastudio-rep-fable` about an updated design are on their way** (Magnus,
  2026-11-16). Answer them from the code and from these folders. Check a fact about JAStudio's code before stating it.
- **Three items wait for Magnus's answer,** kept by the `unanswered` script for this session: 7, `VocabNoteForms`
  written both ways and the conversion sized; 8, send my two reviewers' findings to Compze's representative; 10, work
  out a Compze component that holds the live taggregates and owns the order in which their tevents become visible.

## The sessions, and who speaks for what

| Session | Where | Role |
|---|---|---|
| `taggregates-fable` | JAStudio, `C:\Dev\JAStudio.wt2`, branch `compze-taggretates` | This one. Represents JAStudio to Magnus, and keeps `dev_docs/taggregates/shared` |
| `jastudio-rep-fable` | Compze, `C:\Dev\Compze` | My partner |
| `taggregates-opus` | JAStudio, the same checkout | The other team's JAStudio session. Stopped |
| `jastudio-rep-opus` | Compze | Represents Compze to Magnus, and keeps Compze's `dev_docs/taggregate-improvements/shared` |

Two reviewers, one Opus and one Fable, were subagents of this session. Don't count on reaching them again: what they
found is in `../shared/what-the-two-reviews-changed.md`.

## Magnus's rules for this work, as they stand

- **In this checkout, change only `dev_docs/taggregates/fable` and `dev_docs/taggregates/shared`.** Another session
  works in the same checkout. No permanent code changes: it is design and exploration.
- **Experiments go in the worktree `C:\Dev\JAStudio.worktrees\taggregates-fable-experiments`,** branch
  `taggregates-fable-experiments`, under `experiments/taggregates-fable/`. Commit there too.
- **Commit the documents as they change.**
- **The scope is what Compze has to change.** JAStudio's own design counts only where it changes what Compze must do.
- **Magnus reads no transcript.** Only what is said to him when he asks, or when the sessions have agreed to stop, reaches
  him, and such a report has to be complete by itself.
- **Don't start a conversation with the Compze sessions unasked** (his last word on it, about the component of item 10).
  Answering my partner's questions is asked for.
- **The two reviews of JAStudio's folder stay inside JAStudio,** until he answers item 8.

## What Magnus has said about the design, in his own words or close to them

- **Loading needn't go through the tevent appliers:** "That is not obligatory. Some sort of snapshot restore mechanism
  should reduce this to about the same performance we currently have I would assume?"
- **"Events should NEVER publish unless something actually changed.** Setting something to a value it already had would
  NOT publish any events."
- **Several threads changing one note** "is a really bad idea already with the current code".
- **Loading one note from its file** "should be a trivial thing to support".
- **The order in which changes across notes become visible is Compze's:** "a compze component of some sort should own
  this type of global event routing, not anything in JAStudio. Something like an in memory Taggregate database or session
  or whatever."

My answer to the last, in short: agreed. It is the same problem as the queue inside one taggregate, one level up; the
tevent store's session plays that role for stored history, so JAStudio would be a second consumer of one concept; the
component would hold the live taggregates by id, be what a taggregate reports its publishes to, commit, make tevents
visible in a defined order, offer the subscriptions, and replace a taggregate that refuses use, with persistence as a
seam; JAStudio keeps its indexes, its save and its study-notes as subscribers. It changes the hook's shape, so it should
be designed before the hook is settled. The hard part is order across threads, in a full rebuild. **Not read by me:**
the tevent store's session, the in-process bus, and all but the start of
`C:\Dev\Compze\src\Compze.Tessaging\dev_docs\tevent-delivery-model.md`, whose "observation" rung (committed facts only,
first-in-first-out for each observer) may already hold part of it.

## Facts about JAStudio's code that I checked, and that the folders don't all hold

- **How a change travels today:** a part calls `NoteGuard.Update(mutation)`, which runs the mutation and then
  `JPNote.Flush`. `Flush` serializes the note's DTO with MemoryPack, hashes it, returns if the hash is the last saved
  one, otherwise **records the new hash first** and calls `UpdateInCache`. The cache refiles the note in its indexes
  under its lock, from a new snapshot (`VocabSnapshot` and the like), calls `MarkAsPersisted`, which hashes again, and
  notifies `NoteUpdated`. `JPCollection` subscribed the save first, then its own `NoteUpdated`, which the UI follows.
- **So a save that fails today** leaves the note changed in memory and counted as saved.
- **`INoteRepository` has `Save`, `Delete` and `LoadAll`,** nothing for one note. Each note is a JSON file of its own.
- **Setters that don't compare before changing,** and rely on the hash: `SentenceNote.SetParsingResult`,
  `VocabNote.SetReadings`, `VocabNoteForms.SetEnteredForms`. `ParsingResult` and `ParsedMatch` are classes without value
  equality. `WritableStringValue.Set`, `WordSet.Add` and `NoteTags.Set` do compare.
- **A part calls another part of its note in three places:** `VocabNoteQuestion.SetText` calls
  `Forms.OnQuestionChanged`, and `PerfectSynonyms` adds to `Synonyms` in two. **A part makes its note work in one:**
  `CachingSentenceConfigurationField` calls `_sentence.UpdateParsedWords(force: true)` on every change.
- **What reaches other notes today:** `PerfectSynonyms` pushes the answer to the vocab of its web, from inside the
  setting of the answer; `VocabNoteQuestion` renames the word-references through `JPCollection.RenameTheWordReferences`;
  `MutualWordRelationship`, `MutualKanjiRelationship` and `VocabNoteForms.Add` and `Remove` change the other side.
- **Who follows `NoteUpdated`:** the save, `ChangesThatOutdateNotePages`, `NoteTabViewModel`, `SearchResultsViewModel`,
  `ParsingCasesViewModel`. The note tabs hold ids, and look notes up again after a reload.
- **Reparsing after a change to a vocab is asked for by the UI:** the editors are handed
  `VocabSentencesReparserViewModel.ReparseSentences`, and are disabled while it runs.
- **The full rebuild** (`LocalNoteUpdater`) runs each batch on many threads, each note touched by one. Its rule, in
  `JPNote.UpdateGeneratedData`'s remarks: it changes that note only.
- **A note is built before it joins its collection:** `VocabNote.Create` sets question, answer, readings and forms on a
  note that doesn't flush yet, then `VocabCollection.Add`.
- **Counts:** 51 `NoteGuard.Update` sites in 22 files, 38 files that take a `NoteGuard`, 27 `WritableStringValue`s. A
  vocab composes about 36 guarded parts, a sentence 17, a kanji 19.
- **The specifications run several JAStudios in one process,** so nothing may rest on static state (the opus team's
  fact, not checked by me).
- **JAStudio already references** Compze.Abstractions, Contracts, DependencyInjection, Internals.SystemCE, Threading and
  TypeIdentifiers, at the versions Compze.Teventive 0.6.0-alpha needs. Compze.Teventive and
  Compze.Tessaging.Abstractions would be new.

## Where the work is

- **Documents:** `dev_docs/taggregates/shared` (current), `dev_docs/taggregates/fable` (this team's own, from before
  the comparison), `dev_docs/taggregates/opus` (the other team's).
- **Experiments, on the branch `taggregates-fable-experiments`:** `TaggregateCost`, `TeventiveChecks`,
  `RoutingForEachClassPrototype`, and `ReviewChecks`, the Fable reviewer's, whose project files point at a scratch
  folder and won't build as they stand.
- **Compze:** `dev_docs/taggregate-improvements/shared` (reviewed there separately, and revised), `fable` and `opus`.
  Prototypes on Compze's branches `taggregates-opus/combined` (604c00df4) and
  `taggregate-experiments-fable-static-slots` (fb75d60a0), in worktrees under `C:\Dev\Compze.worktrees\`.

## A trap met twice in this session

A Python script sent through the Bash tool in a heredoc: a doubled backslash arrives as one. `'\\n'` became a line
break in the written file, and a Windows path lost a letter to a tab. Write paths with forward slashes in such scripts,
write no backslash escapes, and use the Edit and Write tools for text that holds backslashes.
