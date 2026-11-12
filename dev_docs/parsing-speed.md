# Parsing speed

How fast JAStudio parses sentences in bulk, as the full rebuild and the reservoir's analysis do, on many threads; what
limited it, and what was done about it (Magnus and Claude, 2026-11-12).

## How to measure it

- **`time_parsing_every_sentence`,** a tool of JAStudio's MCP server, times tokenizing and then analyzing every
  sentence of the corpus on a given number of threads. Each pass reports how long the garbage collector paused the
  threads, how much memory it allocated, and the peak memory. Calls made at the same time run one after the other.
  Discard the first run after a start, which is several times slower than the next ones. The times of the same code
  vary by about a third from one start to the next (2026-11-12), so compare several starts. The memory allocated is the
  same in every start, within 0.1%: it shows a change to allocation that the times can't.
  By default it first collects all garbage, so every run starts from the same memory.
- **`describe_memory`** reports the garbage collector's mode and settings and the process's memory, optionally also
  after the most thorough collection there is.
- **`count_the_candidate_words`** counts the candidate words analyzing every sentence builds, by how many tokens each
  spans, and how many of them could get a match, and did.
- **A profile of the headless JAStudio:** attach dotTrace's timeline profiler to its process while the timing tool
  runs, and read the snapshot with the Rider MCP's dotTrace tools. Its "Lock contention" subsystem shows which locks
  the threads wait for.
  `ConsoleProfiler.exe attach <process id> --profiling-type=Timeline --save-to=<file>.dtt --timeout=40s`
- **Compare in one sitting:** the same code at the same thread count varies by 10-20% between one start of the process
  and the next. Start the old code and the new one after each other, rather than comparing with numbers from earlier.
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

## What limits parsing now (2026-11-12)

Still the processor. The profile made before building only the candidate words that can match also showed, across the
analysis's steps:

- **Tokenizing, in MeCab:** 2.2 s of the 17.4, which the change above doesn't touch, so a larger share now.
- **Allocating:** at least 1.8 s, and likely most of 0.9 s of native code the profiler couldn't name, directly under
  `TextAnalysis`'s constructor. Parsing allocated about 13 GB in the profiled run's two passes, around 270 KB a sentence.
- **`LazyCE` in `CandidateWordVariant`,** for the lookup in the dictionary: about 1 s, with three times the variants
  there are now. Each one creates a monitor of its own, and the first read takes the lock and publishes the value with an
  `Interlocked.Exchange`. Magnus is making `LazyCE` cheap in Compze (2026-11-12).
- **`string.Join` in `CandidateWord`'s constructor:** 0.9 s, most of it on candidate words no longer built.
- **Looking vocab up by form** (`NoteIndex.NotesWith`): 0.5 s; **the dictionary's `MightBeWord`:** 0.55 s.

A new profile would show the shares as they are now.
