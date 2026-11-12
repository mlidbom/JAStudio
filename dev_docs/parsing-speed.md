# Parsing speed

How fast JAStudio parses sentences in bulk, as the full rebuild and the reservoir's analysis do, on many threads; what
limited it, and what was done about it (Magnus and Claude, 2026-11-12).

## How to measure it

- **`time_parsing_every_sentence`,** a tool of JAStudio's MCP server, times tokenizing and then analyzing every
  sentence of the corpus on a given number of threads. Each pass reports the processor time it used and the machine's
  other processes used meanwhile, how long the garbage collector paused the threads, how much memory it allocated, and
  the peak memory. Calls made at the same time run one after the other. Discard the first run after a start, which is
  several times slower than the next ones. The memory allocated is the same in every run, within 0.1%, whatever else
  runs: it shows a change to allocation that the times can't.
  By default it first collects all garbage, so every run starts from the same memory.
- **`describe_memory`** reports the garbage collector's mode and settings and the process's memory, optionally also
  after the most thorough collection there is.
- **`count_the_candidate_words`** counts the candidate words analyzing every sentence builds, by how many tokens each
  spans, and how many of them could get a match, and did.
- **`sample_what_analyzing_every_sentence_allocates`** analyzes every sentence while sampling what the process
  allocates, by type, from the runtime's own allocation events: what the bytes allocated go to, which no profile here
  shows. dotTrace's timeline profile records no managed allocations here.
- **A profile of the headless JAStudio:** attach dotTrace's profiler to its process while the timing tool runs, and read
  the snapshot with the Rider MCP's dotTrace tools. A timeline profile's "Lock contention" subsystem shows which locks
  the threads wait for; a sampling profile, summed up by function with `~/.claude/tools/dottrace_hotspots.py`, shows
  where the processor time goes.
  `ConsoleProfiler.exe attach <process id> --profiling-type=Timeline --save-to=<file>.dtt --timeout=40s`
  - **Export the call tree with `minTotalTimePercent` 0.002, not 0.02:** the threshold is a share of the whole snapshot,
    every thread's idle time included, so 0.02% of a 60-second snapshot cut off every call under 270 ms.
  - **A profile compares two versions under any load** by the time of code neither changed: the analysis took 8.7
    times MeCab's `Viterbi.DoViterbi` before the change of step 5 below, and 7.0 after.
- **Other development work often runs on this machine** (Magnus, 2026-11-12), such as other sessions' builds and tests.
  It takes cores from the runs, and made the same code's times vary by up to a factor of 2.5 that day. A run during which
  the other processes used more than a little processor time was disturbed: repeat it when they're quiet. Check that
  before reading anything into a difference. The process's own processor time doesn't escape it: a thread sharing a
  physical core with another process's runs slower, and its time counts on.
- **Compare in one sitting:** start the old code and the new one after each other, rather than comparing with numbers
  from earlier.
- **The machine these numbers come from** has 16 cores, 32 with hyperthreading, and the corpus 49,162 sentences.

## What limited parsing on many threads, in the order found

1. **One MeCab tagger, under a lock.** Every parse took the lock around MeCab, so tokenizing took the same time on any
   number of threads. Now `MeCabTaggerPool` lends each parse a tagger of its own: tokenizing on 32 threads went from
   1.0 s to 0.1 s.
2. **The garbage collector in workstation mode.** Its collections pause every thread that allocates, and parsing
   allocates fast: on 32 threads, the pauses took almost half the time, and analyzing on 16 and 32 threads was slower
   than on 8. JAStudio now runs the garbage collector in server mode, with DATAS off, wherever it parses: in Anki
   (`src/jastudio_src/jastudio/dotnet/runtimeconfig.json`), in the headless JAStudio and in the desktop host (their
   project files).

   Analyzing every sentence, with the garbage collector's pauses in brackets and the peak private memory, each run
   after a full collection:

   | Threads | Workstation | Server, DATAS on | Server, DATAS off (chosen) |
   |---|---|---|---|
   | 4 | 2.73 s (0.32) · 1.35 GB | 3.42 s (1.13) · 1.87 GB | 2.25 s (0.04) · 3.38 GB |
   | 8 | 2.15 s (0.50) · 1.35 GB | 1.63-1.82 s (0.2-0.4) · 1.98 GB | 1.39-1.53 s (0.02-0.06) · 3.40 GB |
   | 16 | 2.97-3.10 s (1.1) · 1.3 GB | 2.13-2.73 s (0.4-0.9) · 2.31 GB | 1.84 s (0.03-0.05) · 3.40 GB |
   | 32 | 3.80 s (1.9) · 1.26 GB | 2.47-2.61 s (0.7-0.8) · 2.3-2.4 GB | 1.80 s (0.02-0.03) · 3.42 GB |

   - **The memory is held, not needed.** After the most thorough collection, every mode keeps the same managed heap,
     about 1.2 GB. Server mode sizes its allocation budgets for speed, one heap per logical core, and keeps what it
     committed until something makes it collect; on a machine with plenty of memory, nothing does. Its first run after
     a start peaked at 5.6 GB. Inside Anki, Python doesn't feel the memory .NET uses (Magnus).
   - **DATAS** adapts the number of heaps and the budgets to the load, starting from one heap after a full collection.
     It was in between on both speed and memory, and erratic.
   - **One thread is slower in server mode:** analyzing on 1 thread, with the caches' lock of step 3 still in place,
     took 11.6 s, against 8.0 s in workstation mode, and tokenizing 1.4 s against 0.9 (in different sittings). Its
     allocation budget is far larger than the processor's cache, which is likely why: allocating touches memory the
     cache doesn't hold. From 4 threads up, server mode is faster, and parsing in bulk runs on many threads.
3. **The note caches' lock.** Every lookup in a note cache, such as `VocabCache.WithForm` for each candidate word, took
   the cache's exclusive lock, and copied the notes it found while holding it. On 16 threads, the threads spent about a
   third of their time waiting for it (dotTrace timeline). Now lookups take no lock: each index, a `NoteIndex`, and the
   notes by id, `NotesById`, are changed on copies under the cache's lock, and published when the change is done
   (`NoteCacheBase.Change`). A lookup sees the cache before or after a change, never in the middle of one.

   Analyzing every sentence, both in server mode with DATAS off, measured in one sitting:

   | Threads | With the lock | Lookups without a lock |
   |---|---|---|
   | 1 | 11.6 s | 9.2 s |
   | 8 | 1.87 s | 1.63 s |
   | 16 | 1.87 s | 0.98 s |
   | 32 | 1.87 s | 0.68-0.70 s |

   Even one thread got faster: the lock cost something on every lookup, contended or not. Parsing now scales past the
   16 cores into the hyperthreads.

4. **Candidate words that can't match anything.** With the three limits above removed, a timeline profile of analyzing
   on 32 threads found no lock contention, and the garbage collector paused the threads for 0.035 s of the pass's 0.8 s.
   The threads kept 24 to 28 of the 32 logical cores busy, and finished within about 50 ms of each other: parsing was
   limited by the processor, so it could get faster only by doing less work. Of the 17.4 s of processor time the
   threads spent analyzing, building every candidate word took 4.3 s, and the validity of the compounds 3.3 s.

   At each token, the analysis built a candidate word for each number of tokens it could span, up to 12, and analyzed
   each. `count_the_candidate_words` on the corpus:

   | Tokens spanned | Built | With a form a vocab or a dictionary entry has | With a valid match |
   |---|---|---|---|
   | 1 | 523,090 | 464,621 | 463,340 |
   | 2 | 473,928 | 137,252 | 126,875 |
   | 3 | 425,916 | 35,179 | 31,319 |
   | 4 | 379,882 | 5,581 | 5,455 |
   | 5 to 9 | 1,300,948 | 1,160 | 1,149 |
   | 10 to 12 | 417,641 | 0 | 0 |
   | All | 3,521,405 | 643,793 | 628,138 |

   A candidate word gets matches only from the vocab and the dictionary words that have one of its forms, so the 2.82
   million compounds without one, 80% of all candidate words built, were analyzed for nothing. Each form of a candidate
   word starts with the surface of all its tokens but the last, so once no vocab form or dictionary word form starts with
   the surface of the tokens so far, no longer candidate word can match. Now the analysis builds no longer ones
   (`TextAnalysisLocation`, with `VocabCache`'s index of form prefixes and the dictionary's word forms in order,
   `FormsInOrder`). It builds 1,162,917 candidate words instead of 3,521,405, with exactly as many known and valid as
   before, and a full rebuild changes no notes.

   Analyzing every sentence, measured in one sitting:

   | Threads | Every candidate word | Only those that can match |
   |---|---|---|
   | 1 | 8.41 s | 6.53 s |
   | 8 | 1.18 s | 0.83 s |
   | 16 | 0.87 s | 0.63 s |
   | 32 | 0.69-0.70 s | 0.41-0.43 s |

   The garbage collector collects 4 times a pass instead of 12.

5. **Culture-sensitive comparisons and allocating LINQ in the hot paths (2026-11-12).** A sampling profile of analyzing
   every sentence on 16 threads, and `sample_what_analyzing_every_sentence_allocates`, found two kinds of waste:
   - **`string.EndsWith` took 5% of the analysis,** nearly all of it in `VocabNotePartsOfSpeech.IsCompleteNaAdjective`.
     Given a string and no `StringComparison`, it compares culture-sensitively, through ICU. Every such call in the
     solution, 30 of them, now compares ordinally, with Compze's `StartsWithOrdinal` and `EndsWithOrdinal`, and the
     analyzer rule CA1310 warns at a new one (`.editorconfig`).
   - **LINQ that allocated a delegate and an iterator at every call,** in code the analysis calls millions of times a
     pass: checking a match's requirements (now `Match.NoneFail`), telling whether a candidate word's display variants
     changed (now `SequenceEqual`), `WordExclusionSet.ExcludesAtIndex`, and `Where` followed by `ToList` on a list (now
     `FindAll`). Analyzing every sentence allocates 4,881 MB instead of 5,894.
   - **A candidate word, and a location, has one display variant or none** (`CandidateWord.DisplayVariant`,
     `TextAnalysisLocation.DisplayVariant`), as it always had, instead of a list made anew at every display pass
     (Magnus: "if there can't be, by all means change it"). Analyzing every sentence allocates 4,621 MB.

   The display analysis's steps take half the time they did. A full rebuild changes no notes. Analyzing every sentence,
   measured in one sitting, with little other work on the machine:

   | Threads | Before | After |
   |---|---|---|
   | 1 | 6.65-6.66 s | 5.24 s (one run; the others were disturbed) |
   | 16 | 0.62 s, 9.5 processor-seconds | 0.51 s, 7.4-7.6 processor-seconds |

## The first full rebuild after a start (2026-11-12)

A full rebuild reparses every sentence, then updates every note's generated data and tags, each step on
`ReanalysisThreads` threads (Magnus's configuration: 16). `full_rebuild` reports its time and the processor time other
processes used meanwhile, and the headless log has each step's time (`TaskLogEntry`'s tree, written when the rebuild ends).

- **Reparsing is most of it:** of a warm rebuild's 1.4 s, reparsing took 0.9 s and the rest 0.5 s, on one thread, 0.3 s
  of it updating the vocab.
- **The updates run on many threads now (2026-11-12): 56 ms instead of 433,** the vocab's 30 ms instead of 336. A warm
  rebuild takes 0.96 s instead of 1.18, measured minutes apart under similar load, and the first after a start 1.66 s.
  It can, since `UpdateGeneratedData` changes its own note only: the one update that changed other notes, pushing a
  vocab's answer to its perfect synonyms, now happens when the answer changes. The steps still run one after another:
  the kanji's tagging reads the vocab, for one.
- **The first rebuild after a start took 3.7 s, against 1.4 s warm.** A timeline profile showed why, and it was
  mostly the dictionary, not the JIT:
  - **Its forms loaded at the first lookup,** 1.6 s on one thread inside the reparse, the other threads waiting. Now the
    collection's load starts it at startup, alongside the notes, which take longer, so startup is no slower.
  - **LinqToDB looked for its SQLite provider at every connection,** and the dictionary opens one per lookup: it tried
    System.Data.SQLite, which JAStudio doesn't use, and caught the `FileNotFoundException`, 2,032 times. The
    connection options now name Microsoft.Data.Sqlite, and are made once.
  - **Every word's first lookup goes to SQLite,** five queries through LinqToDB, until `DictLookup`'s cache holds it:
    3.6 of the reparse's 15.8 processor-seconds. Half of it is LinqToDB's own work. Still so.
  - **The JIT:** about 1.5 processor-seconds compiling.
- **Now the first rebuild takes 2.2 s,** at comparable load from other work: about 0.8 s more than a warm one.
- **The headless JAStudio on a worktree reads the worktree's copy of `meta.json`,** which can be stale: one from May
  made it reparse on 4 threads. See the `review:` comment in the gateway's `HeadlessJAStudioSetup`.

## What limits parsing now (2026-11-12)

Still the processor, and nothing big is left that a small change would remove. The sampling profile after step 5, of the
analysis's 13.1 processor-seconds in two passes on 16 threads:

- **Tokenizing, in MeCab:** 2.8 s, a fifth, in MeCab.DotNet's own code: the Viterbi search, the dictionary's lookups,
  and decoding each node's features. Its lattice nodes, `MeCabNode`, are 15% of all that analyzing allocates. Only a
  faster tokenizer would change that, and a different one might tokenize differently.
- **Looking vocab up by form** (`NoteIndex.NotesWith` and `HasNotesWith`): about 1 s. Of it, 0.67 s is the concurrent
  dictionary's lookups, and 0.2 s copying each lookup's notes into a new list. Every note cache lookup returns a list;
  returning the published set itself would save the copy, at the cost of changing what every lookup returns.
- **The analysis's lists,** of matches above all, are about a sixth of what it allocates: each candidate word variant's
  matches, valid matches and display matches, the last made anew at every display pass.
- **The rest is the analysis's own logic,** spread over the matches' requirements and the display passes.
