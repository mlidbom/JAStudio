# Code issues — TODO

> **Status: a list to work through, started 2026-11-09.** Bugs found in JAStudio's code while working on something else,
> that aren't fixed when they're found, so that they do get fixed (Magnus, 2026-11-09). Remove an entry once it's fixed.
>
> The corpus's data has a list of its own: `corpus-data-issues.md`.

## `DictLookup.LookupName` checks the word forms, not the name forms

Found 2026-11-09, while speeding up JMdict's lookups. `DictLookup.LookupName` returns no result unless `MightBeWord` holds,
where it means `MightBeName`: JMdict's forms rather than JMnedict's. So a name whose form isn't also one of JMdict's word
forms is never found by `LookupName`, nor by `LookupWordOrName`, which falls back to it when no word is found.

**What it affects:** `LookupVocabWordOrName` for a vocab without readings, and so what JAStudio derives from the dictionary
for such a vocab: its parts of speech (`VocabNotePartsOfSpeech`), whether it's usually written in kana (`VocabNoteFactory`)
and the answer generated for it (`VocabNote.GenerateAndSetAnswer`); and the MCP server's `look_up_in_the_dictionary` given
a word.
`TryLookupWordOrNameWithMatchingReading`, used for a vocab with readings, isn't affected: it checks the name forms.

**The fix** is the one word, but it changes what JAStudio computes for some vocab. So it's a change to analysis: build it,
run a full rebuild, and review what the rebuild changed before committing it with the corpus's changes
(`.claude/rules/02-universal-local/045-committing-with-the-corpus-repository.md`).
