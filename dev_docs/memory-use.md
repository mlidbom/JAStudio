# Memory use: what the notes take

What the collection's notes take in memory, and what could take less, so that more sentences fit. Measured 2026-11-12
on the headless JAStudio with the corpus loaded: 49,162 sentences, 28,004 vocab and 2,554 kanji.

## How it's measured

`describe_what_the_heap_holds` counts the heap by type, and the strings by value, after collecting all garbage: compare a
call before a change and one after, both on a JAStudio that has just started or both on one that has run a while. A
few thousand large `byte[]`s come and go: right after a start, they held 1.1 MB once and 119 MB, in 5,987 arrays, another
time. Pooled buffers, presumably, not notes: compare the types that are notes'.

The first measurement, below, came before that tool: `describe_memory` with `alsoAfterCollectingGarbage`, 1,108 MB on
the managed heap after the most thorough collection. Then a heap dump (`dotnet-dump collect -p <pid> --type Heap`, 1.4 GB) summed by type (`dotnet-dump analyze` with
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
   - **Gone (2026-11-17):** step 10 of the migration to the state store made every text field a plain string, with a
     tevent of its own, so `WritableStringValue` and its notifiers are gone (`57df9e5b2`).
2. **Keep a hash of the last saved snapshot instead of the snapshot: about 78 MB, 0.9 KB a sentence.** A 128-bit hash,
   such as `XxHash128`, is 16 bytes. `Flush` and `HasTheSameStoredDataAs` compare the hashes instead of the bytes. A
   collision would make a change go unsaved; at 128 bits, that won't happen.
   - **Done (2026-11-12):** `JPNote` keeps the hash. Right after a start, the `byte[]`s went from 178 MB, 85,702 of them,
     to a few thousand: the snapshots took 2.2 KB a note then, more than the 0.9 KB the dump taken an hour into a run
     showed.
   - **Gone (2026-11-17):** step 10 of the migration to the state store made every part of a note publish tevents of
     its own, so nothing compares a note's stored data any more, and the hash went with `Flush`.
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

## What the heap holds besides the notes

The rest of the 1.1 GB is the vocab, which take more each than a sentence, the kanji, the indexes that find notes by
word, and the dictionary's and the UI's caches. The `byte[]`s aren't MeCab's dictionary: only 9 arrays are over 200 KB,
2.3 MB in all.

## On the state store, against main (2026-11-17)

Step 12 of [the migration of the notes to Compze's state store](taggregates/shared/the-migration.md#step-12-as-measured)
compared the branch `migrating-the-notes-to-the-state-store` (`237b5bca6`) with `main` (`59cd15125`): each started warm
in the headless JAStudio, on the same corpus checkout, with 49,338 sentences, 28,004 vocab and 2,554 kanji.

| | main | the branch, Compze at `f0ed1ccf6` | the branch, Compze at `53d24b46c` |
|---|---|---|---|
| The managed heap after the most thorough collection (`describe_memory`) | 878 MB | 850 MB | 730 MB |
| The process's working set then | 958 MB | 941 MB | 818 MB |
| Its private memory then | 1,058 MB | 1,035 MB | 907 MB |
| The heap's objects (`describe_what_the_heap_holds`) | 12.70 million | 12.61 million | 10.47 million |
| The heap without the `byte[]`s that come and go | 727 MB | 712 MB | 574 MB |

**So the state store takes about 2% less at `f0ed1ccf6`, and about 15% less at `53d24b46c`,** where Compze's index keeps
no keys, as below. At `f0ed1ccf6`, what went and what came are of about the same size, by the types that take the most in
each heap:

| What | main | the branch |
|---|---|---|
| The text fields' `WritableStringValue`s and their notifiers, `SimpleObservable<string>` | 46 MB, 719,064 of each | gone |
| The sentence indexes' `HashSet<SentenceNote>`s | 41 MB, 109,500 | gone |
| The vocab indexes' `HashSet<VocabNote>`s, and the dictionaries holding them | 48 MB, 220,044 | gone |
| `int[]`s and `string[]`s | 35 and 19 MB | 15 and 6 MB |
| Each taggregate's keys in each index, an `ImmutableHashSet` each (Compze's `TaggregateIndex`, `_committedKeysById`) | | about 105 MB, for about 1.6 million keys |
| The taggregates by key, arrays of `ITaggregateReference`s | | 21 MB |
| The maps from each key to them, `ConcurrentDictionary` nodes | | about 15 MB |
| The store's taggregates by id, and its references to them | | about 13 MB |
| The strings, and the stored parsing results' `ParsedMatch`es | 146 and 31 MB | the same |

**Done: an index that keeps no keys** (Compze's `53d24b46c`, 2026-11-17). An `ImmutableHashSet` takes about 56 bytes a
key, and a sentence's words (`SentenceIndexes.Word`) and matched vocab (`SentenceIndexes.MatchedVocab`) took 33 MB each.
The index now computes a changed taggregate's old keys from the committed instance as the commit is prepared, so it
keeps none. That took 138 MB off the heap, more than the sets' 105 MB: the key objects only they held went with them,
2.1 million objects in all. What is left of the indexes is about 42 MB: the `ConcurrentDictionary` from each key, about
21 MB, and its arrays of `ITaggregateReference`s, 21 MB.
