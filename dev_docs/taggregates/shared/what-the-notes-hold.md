# What the notes hold, and what each part becomes

> **Written 2026-11-16 by `taggregates-opus`, from a survey of every part class the three notes are built from,** at
> Magnus's word: "apply the same thinking to other data stored in the notes. There may be other overcomplications in
> there." It decides what each part becomes when step 10 of [the migration](the-migration.md) converts it. Paths are
> under `src/src_dotnet/JAStudio.Core/Note/`.

## The headline

**About two thirds of the parts are simple data wrapped in objects.** Each becomes a plain property of its note, a string,
a list or a value, with a `PropertyUpdated` tevent. The real behavior is in a few places:
- renaming a vocab;
- the forms;
- the perfect synonyms;
- the two-way relationships;
- the match-condition;
- the kanji's readings' invariants.

## Simple: a plain property and its `PropertyUpdated` tevent

- **`WritableStringValue`, all 27 text fields,** with its interfaces `IStringValue` and `IWritableStringValue`.
- **The source data, `VocabSourceData`, `KanjiSourceData`, `SentenceSourceData`:** only groupings of fields.
- **`WordSet`, `KanjiSet`, `WordRelationship`, `DerivedFrom`.** `WordRelationship` is used directly only by
  `ConfusedWith`.
- **`MediaReferenceList<ImageReference>`,** the five image lists.
- **`VocabNoteMetaData`,** whose one stored value, `SentenceCount`, is generated data.
- **`VocabNoteRegister`, with its 14 `FlagField`s and a `FlagFieldList`:** a set of register values.
- **`VocabMatchingFlags`, `VocabYieldToSurface`, `VocabNoteUsuallyWrittenInKana`.**
- **`KanjiRadicals`, `KanjiPrimaryVocab`.**
- **`ParsingResult`,** generated data.

## Real behavior: a component, or a class of its own

- **`VocabNoteQuestion`:** its validation, and renaming a vocab, which renames the word-references to it in other notes.
- **`VocabNoteForms`:** the matched forms, owned forms and dictionary-form expansions it computes, and the forms it mirrors
  into the vocab whose question a form is.
- **`PerfectSynonyms`:** it connects a web of vocab, and pushes the answer to every synonym.

## Mixed: simple stored data, with logic that is real

- **`VocabNotePartsOfSpeech`:** the stored set is simple. Its queries, such as `IsSuruVerbIncluded`, read the question
  and the compound parts, and belong to the vocab.
- **`VocabNoteCompoundParts`:** two simple lists, and `AutoGenerate`, a real operation.
- **`RelatedVocab`:** a container of seven relationships, and read-only queries.
- **`MutualWordRelationship` and `MutualKanjiRelationship`:** each relationship's set is simple. Changing the other side
  is one real, reusable piece, written twice today.
- **`Synonyms`:** a subclass for one command, `AddTransitivelyOneLevel`.
- **`AudioReferenceList`:** the list is simple. Its two text-to-speech operations are list transforms.
- **`KanjiNoteReadings` and `KanjiReadingList`:** real invariants, on readings and kana, that fit an immutable value. The
  mutable part behind a guard is the overcomplication.
- **`NoteTags`:** one set of tags. The rules for tag branches are real, and pure functions over that set.
- **`VocabNoteMatchingConfiguration`:** the match-condition is a real component, and already immutable. The flags around
  it are simple: `VocabMatchingRequireForbidFlags`, `RequireForbidFlagField`, `YieldLastTokenToOverlappingCompound`,
  `IsInflectingWord`.
- **The sentence's configuration:** `SentenceNoteConfiguration` (`CachingSentenceConfigurationField` when this was written) wraps `SentenceConfiguration`, caches nothing, and
  reparses on a change. `SentenceConfiguration` should be an immutable value. Its `HighlightedWords` is a public mutable
  list that allows duplicates, edited as a set. `WordExclusionSet` keeps real queries for the parsing hot path, but saves
  through a callback, never checking for no change.

## Views with no stored data

**`VocabNoteKanji`, `VocabNoteConjugator`, `VocabNoteSentences`, `VocabCloner`, `AllAudioOfANote`,
`SentenceQuestionField`, `FallbackStringField` and `StripHtmlOnReadFallbackStringField`** hold nothing that is stored.
They stay as queries, or become methods of their note. `VocabNoteSentences`'s `SentenceCounts` is a cache of up to 6,000
seconds.

## The same thing written more than once

- **`WordSet` and `KanjiSet`:** "a set of unique items, changed only when the change changes it".
- **`MutualWordRelationship` and `MutualKanjiRelationship`:** "also change the other side, and refuse oneself".
- **`VocabMatchingRequireForbidFlags`** repeats `FlagFieldList`'s assertion of known names, its ordinal `ToDto`, and its
  `OverwriteWith`.
- **Four renames of a word-reference:** `WordSet.Rename`, `KanjiPrimaryVocab.Rename`, `VocabNoteCompoundParts.Rename`,
  `DerivedFrom.Rename`.
- **"The corpus's value, else the source's", four times:** `VocabNote.GetAnswer`, `KanjiNote.GetAnswer`,
  `FallbackStringField.Get`, and `SentenceQuestionField`'s raw value.

## What the conversion must keep

- **A `CorpusAnswer` that is set to the value it has publishes nothing.** That is what stops `PerfectSynonyms` from pushing
  an answer back and forth forever: each set pushes it to every synonym, which pushes it back.
- **`WordExclusionSet` changes its set outside the guard.** The guard wraps only the reparse that follows
  (`Sentences/SentenceNoteConfiguration.cs`). Its tevents must cover the
  change itself. Done in step 10: the set changes through the action it's given, which publishes.

## Noticed along the way

- **A kanji's question could be edited, but nothing renamed the references to it** in other kanji's radicals,
  similar-meaning and confused-with relationships. Settled by Magnus: a kanji's question is its character, set when it is
  created and never changed (commit f75555a65). So a kanji has no question tevent.
- **The stored order of related vocab depends on the current culture** (`Storage/Converters/VocabNoteConverter.cs`, which
  already has a `review:` comment), where the alternative compound parts are in ordinal order.
- **`VocabNotePartsOfSpeech.Remove` has no caller,** and `MediaReferenceList.Add` doesn't check what its documentation
  says it does: that the note doesn't reference the file yet.
- **Dead code removed** (commit 5e85cf9b3): `ValueWrapper<T>`, `IObjectSerializer<T>`, and
  `CachingSentenceConfigurationField.SetValueDirectlyTestsOnly`.
