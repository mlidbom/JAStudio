# Memory use: what the notes take

What the collection's notes take in memory, and what could take less, so that more sentences fit. Measured 2026-11-12
on the headless JAStudio with the corpus loaded: 49,162 sentences, 28,004 vocab and 2,554 kanji.

## How it's measured

`describe_what_the_heap_holds` counts the heap by type, and the strings by value, after collecting all garbage: compare a
call before a change and one after, both on a JAStudio that has just started or both on one that has run a while. A
few thousand large `byte[]`s come and go: right after a start, they held 1.1 MB once and 119 MB, in 5,987 arrays, another
time. Pooled buffers, presumably, not notes: compare the types that are notes'.

The first measurement, below, came before that tool: `describe_memory` with `alsoAfterCollectingGarbage`, 1,108 MB on
the managed heap after the most thorough collection, in megabytes of 1024 × 1024 bytes, which that tool counted then:
1,162 MB as every tool counts now. Then a heap dump (`dotnet-dump collect -p <pid> --type Heap`, 1.4 GB) summed by type (`dotnet-dump analyze` with
`dumpheap -stat -live`) and by string value (`dumpheap -strings`), and `gcroot` for who holds an object. `objsize` on a
note is no use: from a sentence, the references reach nearly the whole heap.

The per-sentence figures below are estimates: each type's count divided out over the notes that own it, not a
measurement of one sentence. They come to about 10 KB a sentence.

## The heap, by what takes the most (1,143 MB live)

| What | MB | Instances | Per sentence |
|---|---|---|---|
| Each `WritableStringValue`'s change notifier, a Compze `SimpleObservable<string>`, and the lock it creates at once (`IMonitor.New()`: `MonitorCE`, its `ThinMonitorWrapper` and `object`, a `System.Threading.Lock`, two `LockDisposer`s with an `Action` each, a `List<TakeMonitorLockTimeoutException>`) | ~300 | 717,480, 9 a note | 9 fields: ~3.7 KB |
| Strings | 181 | 3.7 million, 1.8 million distinct values | ~1.2 KB |
| …of which copies of a string held elsewhere too | 63 | | |
| Each note's last saved snapshot, `JPNote._lastPersistedSnapshot`, its whole MemoryPack serialization, kept to tell whether `Flush` must save | ~78 | ~80,000 | ~0.9 KB |
| The stored parsing result's matches, `ParsedMatch` | 31 | 635,966, 13 a sentence | 0.6 KB |
| A `VocabId` per match, made anew for each as it's loaded | 22 | 691,877 | 0.35 KB |
| `"S"` and `"B"`, a new string for each match's variant as it's loaded | 15 | 635,996 | 0.3 KB |
| The sentence collection's indexes: `HashSet<SentenceNote>`s | 41 | 109,325 | 0.8 KB |
| A note's card statuses, a `Dictionary<string, bool>` each | 14 | 79,720 | 0.18 KB |
| The media reference lists, mostly empty | 20 | ~310,000 | 0.3 KB |

## The easy wins, largest first

1. **No lock per change notifier: about 300 MB, 3.7 KB a sentence, a third of what a sentence takes.** Only
   `PerfectSynonyms` subscribes to a field, to two of each vocab's. No sentence field has a subscriber, yet each of a
   sentence's nine (`SourceName`, `Reading`, `CorpusQuestion`, `CorpusAnswer`, `Comments` and the source data's four)
   carries a notifier with its lock.
   - **In Compze:** `SimpleObservable` needs no lock at all: `ImmutableInterlocked.Update` swaps its immutable list of
     subscribers. That alone saves 384 bytes of the 416. And `MonitorCE` itself could be lighter, which helps every
     lock: the list of timeout exceptions could start as a shared empty one, and the wrapper could lock on itself.
   - **In JAStudio:** `WritableStringValue` could make its notifier on the first `Subscribe`, the rest of the 416.
   - **Done (2026-11-12), in Compze:** Compze.Internals.SystemCE 0.6.1-internal's `SimpleObservable` takes no lock and is
     24 bytes, and Compze.Threading 0.10.1-alpha's monitor 144 bytes instead of 384. Right after a start, the heap went
     from 1,017 MB to 733.5 MB, its objects from 19.2 million to 12.7 million, and the `System.Action`s from 1.6 million,
     101.9 MB, to 157,226, 10.1 MB. The JAStudio half isn't worth it now: the 717,480 notifiers take 17.2 MB.
2. **Keep a hash of the last saved snapshot instead of the snapshot: about 78 MB, 0.9 KB a sentence.** A 128-bit hash,
   such as `XxHash128`, is 16 bytes. `Flush` and `HasTheSameStoredDataAs` compare the hashes instead of the bytes. A
   collision would make a change go unsaved; at 128 bits, that won't happen.
   - **Done (2026-11-12):** `JPNote` keeps the hash. Right after a start, the `byte[]`s went from 178 MB, 85,702 of them,
     to a few thousand: the snapshots took 2.2 KB a note then, more than the 0.9 KB the dump taken an hour into a run
     showed.
3. **Share what every match repeats: about 50 MB, 0.9 KB a sentence.** `ParsedWordSerializer` and
   `SentenceDto.FromParsedMatchDto` could:
   - use the constants `ParsedMatch.SurfaceVariant` and `BaseVariant` instead of the strings read, or make the variant a
     bool or an enum: 15 MB;
   - give each match the vocab's own `VocabId` instance, or one id instance per vocab: up to 22 MB;
   - share one string for each matched word's form: likely 10–15 MB, part of the 63 MB of copies.
   - **Done (2026-11-12):** the `ParsedMatch` constructor takes the constant for the variant and interns the form, and
     loading gives each match `VocabId.Shared`. Right after a start, the strings went from 180.9 MB to 149.7 MB, their
     copies from 63.1 MB to 31.9 MB, and the `VocabId`s from 691,877, 22.1 MB, to 81,343, 2.6 MB.
4. **Smaller:** one string for the parser version every sentence repeats (`text_analysis_0.1`, 2.8 MB); the audio's
   voice names (`Azure ja-JP-DaichiNeural` 18,640 times, the voices together about 4 MB); the card statuses and the
   empty media lists made only when needed (about 30 MB).

Together, the first three would take a sentence from about 10 KB to about 4.5 KB.

**The three took the heap, right after a start, from 1,243 MB to 733.5 MB** (2026-11-12), the large `byte[]`s that come
and go aside: items 2 and 3 to 1,017 MB, then the Compze upgrade of item 1. A sentence takes about 5 KB now, an estimate
as the 10 KB was.

## The next wins (2026-11-12, after the first three)

Right after a start, the heap held 818.8 MB: 106 MB of it the shared `ArrayPool`'s arrays (below), 713 MB the rest.
`describe_what_holds_the_objects_of_a_type`, built for this, told what holds the strings and the `byte[]`s. The figures
are estimates from the counts by type, unless they say measured. Largest first:

1. **The note caches' indexes keep each key's notes as a `HashSet`: about 70 MB, 0.8 KB a sentence.** 329,369 sets,
   220,044 of vocab and 109,325 of sentences, most of them holding a few notes. A set of one note takes about 180 bytes,
   an array of one 32. `NoteIndex` never changes a set it has published: a change copies the set, changes the copy and
   publishes it. So the published sets could be arrays: `PublishChanges` would turn each changed copy into one, and the
   changes would go on working on `HashSet`s. `NotesWith` copies the notes into a list either way. A full rebuild would
   check that the notes still come in the order they did.
   - **What it does to adding and removing (Magnus asked):** loading adds every note in one change, publishing once, so
     each set is built once either way. But every later change to a note publishes on its own (`JpNoteUpdated`, and
     creating a note, `AddToCache`), and copies the whole set of each key whose notes it changed. Today that copy is a
     `HashSet`'s clone of its arrays; from an array, it would be a `HashSet` built anew, hashing every note, and an array
     again when publishing: the same order of work, a few times more of it per copy (not measured).
   - **At a million sentences, the copying is the problem either way:** a common form's key would hold hundreds of
     thousands of sentences, so every sentence created, or changed in what it matches there, would copy megabytes,
     on the large object heap. Adding 100,000 sentences one at a time would copy such a set 100,000 times. The sentence
     indexes have to be designed for that scale, for time and memory together.
2. **The names dictionary's every form, kept in memory: about 49 MB, not per sentence.** `DictionaryProvider.AllNameForms`
   holds 952,653 forms, so that `DictLookup.MightBeName` can answer "no such name" without a SQLite query. Names are
   looked up only to set a vocab's parts of speech from the dictionary
   (`VocabNotePartsOfSpeech.SetAutomaticallyFromDictionary`) and by the MCP tool `look_up_in_the_dictionary`, never in
   parsing. Without the set, each name looked up the first time would cost a query; `LookupNameRaw` caches the results.
   Loading the forms is also part of the 1.7 seconds the dictionary's forms take at a start.
   - The word forms are another matter: parsing asks `MightBeWord` millions of times a pass. `FormsInOrder` holds them
     sorted as well, so a binary search could replace their set, saving about 8 MB and costing parsing time.
3. **A `FlagField` object for each flag: about 23 MB, not per sentence.** Each vocab has 18, of 40 bytes, in two arrays:
   504,072 objects and 5.6 MB of arrays. The flags of one kind could be the bits of one number in their `FlagFieldList`,
   with a `FlagField` made when one is asked for.
4. **Each field's change notifier made when it gets its first subscriber: 17.2 MB measured, 0.2 KB a sentence.** The
   JAStudio half of the first win: 717,480 `SimpleObservable<string>`s of 24 bytes, nine a sentence. Only
   `PerfectSynonyms` subscribes, to two fields of each vocab.
5. **`VocabNoteForms`' seven lazy values per vocab: about 14 MB, not per sentence.** Each is a `LazyCE` and a delegate, 96
   bytes, made anew whenever the forms change: 18.8 MB. The six derived from the vocab's own forms could be one lazy
   object. The forms no other vocab owns depend on the other vocab, so they'd stay apart.
6. **Empty collections made only when needed: about 25 MB, part of it per sentence.** Each vocab's six `WordSet`s, and its
   compound parts' entered alternatives, have a `HashSet<WordReference>` each, nearly all empty (12.5 MB). Each sentence's two `WordExclusionSet`s have a
   `HashSet<WordExclusion>` each, nearly all empty, and share a delegate (9.4 MB). And there are 311,218 media reference
   lists for 83,797 audio references and about 36,000 image references (20 MB, about half of it saved).
7. **A note's card statuses as a `Dictionary<string, bool>`: about 12 MB of its 17, 0.15 KB a note.** A note has a few
   cards; an array of them, or a number with a bit for each, would do.
8. **A delegate in each `NoteGuard`: 6.9 MB measured.** 107,724 `System.Action`s of 64 bytes, each saving its note: the
   guard could hold the note instead.
9. **Strings repeated as they're loaded: about 5 MB.** The parser version every sentence repeats (`text_analysis_0.1`,
   2.75 MB) and the audio's voice names (2.3 MB) could be constants or interned, as the parsed forms are. The audio
   attachments' absolute paths (`FileInfo.FullName`), about 5 MB more, could be made from the media folder's path when
   asked for.

Together about 230 MB of the 713. Of a sentence's estimated 5 KB, items 1, 4, 6, 7, 8 and 9 would take about 1.5 KB.

**Status: enough for now (Magnus, 2026-11-12).** At about 5 KB a sentence note, a million sentences would take about 5 GB
of live objects: "5GB is no problem at all, I'd start feeling pain at 20GB maybe", and the working set will likely stay
far below a million. So none of the wins below is being made. One thing stays true at any size: every change to a note
copies the whole set of each index key it touches, so importing many sentences into the working set has to add them in
one change (`AddAllToCache`), not one at a time.

**What scales with the sentences (Magnus, 2026-11-12):** only what each sentence note costs matters for a working set of
hundreds of thousands of sentences, or a million; the vocab's, the kanji's and the dictionary's costs stay as they are.
What a sentence note costs, by part, estimated from the counts by type over the 49,162 sentences:

| Part | Bytes a sentence |
|---|---|
| The sentence indexes' sets (`NoteIndex`) | ~1,040 |
| The snapshot they're compared against (`SentenceSnapshot`, its `NoteId[]` and string) | ~240 |
| The parsed matches (13 `ParsedMatch`es of 48 bytes, and their array) | ~750 |
| The nine fields (`WritableStringValue`s and their notifiers) | ~580 |
| The note's bookkeeping: tags (a `HashSet` and a `List`), card statuses, guards, the Anki id maps, two `SentenceId`s, the source data | ~860 |
| The word exclusions (two `WordExclusionSet`s, their `HashSet`s and a delegate) | ~270 |
| The media lists and audio references | ~450 |
| The `SentenceNote` itself, its `ParsingResult` and the parser version string | ~300 |
| The text | a few hundred |

The easy wins above that are per sentence (4, 6, 7, 8 and 9) come to about 0.8 KB of the 5; the indexes to about 0.8 KB
more, but they need the redesign above.

**Not a win:**
- **The 106 MB of `byte[]`s are the shared `ArrayPool`'s arrays:** 60 arrays in the per-thread caches of 7 threads,
  which presumably rented them while loading. They aren't notes, and their size varies with which threads did the
  loading: 1.1 MB once, 119 MB another time. As far as I know, the pool lets a thread's arrays go only when memory runs
  low or the thread ends. What rents them I haven't looked into: if it's code of ours reading large files into a growing
  buffer, renting less would make the 106 MB go away.
- **The fields' strings,** 28 MB that `WritableStringValue`s hold, are the notes' own text.

## What the heap holds besides the notes

The rest of the 1.1 GB is the vocab, which take more each than a sentence, the kanji, the indexes that find notes by
word, and the dictionary's and the UI's caches. The dictionary's sets of every word form and name form hold 1.36 million
strings, 42.5 MB, and their entry arrays 23 MB more. The `byte[]`s aren't MeCab's dictionary: only 9 arrays are over
200 KB, 2.3 MB in all.
