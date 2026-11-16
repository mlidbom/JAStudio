# What the two reviews changed

> **Status: 2026-11-16.** Two reviewers who took no part in the exploration, one Opus and one Fable, each reviewed
> the shared conclusions, the documents under them, the code and the prototypes, without contact with each other or
> with anyone else. Then the two of them and `taggregates-fable` discussed the findings and agreed on the changes
> below. The conclusions as changed are in [README.md](README.md).

## The verdict, which the two reached separately

**What was found about Compze holds. That the notes should become Taggregates is not established.** Both reproduced
every figure of bytes kept, the eight behaviors and both flaws. Both found that "the idea fits JAStudio" was asserted:
JAStudio's side was never designed, sized or compared with a smaller route, and the design has gaps where JAStudio
meets it.

## What they found, and what was changed

| Finding | Found by | The change |
|---|---|---|
| "The idea fits JAStudio" is asserted, not weighed. A much smaller route was never examined: tevents dispatched from today's change points on one dispatcher for the collection, with no change in Compze | Both | The first conclusion is now that Compze can be made to carry the notes. Three options are set out, the smallest first, and the next step is one real part written both ways |
| The size of the conversion is nowhere: 51 `NoteGuard.Update` sites in 22 files, 27 `WritableStringValue`s, about 21 slots | Both | In the conclusions |
| The order across notes isn't kept: a subscriber that changes another note makes later subscribers hear that note's tevents first | Both; run by Fable | Under what the option costs JAStudio: a queue for the whole collection |
| Today's check that nothing changed disappears: a forced reparse would publish, save and dispatch for every sentence | Both | Under the costs: publish only when the value differs |
| The assertion of one thread at a time covers the hook, so the save and every subscriber | Fable, run | Under the costs: one writer at a time in each note |
| The fixed `Commit` loses a tevent when its callback throws, where 0.6.0-alpha hands it on again | Fable, run | Under the costs: a failed save |
| A refused note can't be loaded again by itself | Opus | Under the costs |
| The costs are given against the wrong baseline: faster than 0.6.0-alpha, but about 20 times slower than plain objects to load | Both | Under the costs, with the Release figures |
| The Compze-side programs reference Debug builds of Compze | Fable; confirmed by Opus | Under how far the evidence goes |
| "Prototyped in Compze's base classes" over-claims: additive, one test model, the old loading API, subtractions given as results | Both | Under how far the evidence goes |
| Two choices were settled by bytes, against the simple version first | Opus; Fable agrees | Put back to Magnus as options |
| Wiring inside a note doesn't become subscriptions: it moves into the tevent types | Opus | Under the costs |
| A creation tevent for each way a note is built, and a third form of a stored note | Fable | Under the costs |
| The stale set in `VocabNoteForms` can be fixed today with a subscription to `NoteUpdated` | Opus | Under what was found along the way |

## Where the three of us differed, and how it was settled

- **Is the smaller route a step toward Taggregates, or the place to stop?** Both, said both reviewers: a step if it
  uses the Taggregate tevent types from the start, and possibly the end, to be decided after living with it. The Opus
  reviewer added what it gains, the Fable reviewer what is done twice.
- **The assertion covering the hook: a flaw, or the assertion doing its job?** Its job, all three. The queue isn't
  safe for two threads, so the assertion has to cover the hook. What was wrong was that the documents counted it as a
  gain only.
- **The heap against today's notes.** The Fable reviewer had it lower, the Opus reviewer unknown. Settled as: not
  measured, likely about even or lower.
- **The figures of time and of bytes allocated.** The Opus reviewer's were from a Debug build of Compze. Settled on the
  Fable reviewer's Release figures.
- **Whether entities are built in the per-class declarations.** The Opus reviewer had found none; the Fable reviewer
  showed them on the branch with slots of the class. Settled as built.

## What neither reviewer could check

The opus team's benchmarks, which weren't rerun; the full suite on the combined branch; two threads in a real race;
what `TaggregateTypeValidator` costs at a start; the Python stub generator; and JAStudio's load time today. The stale
set in `VocabNoteForms` is plausible by reading, and was not reproduced.

## What the reviewers ran

The three programs on the branch `taggregates-fable-experiments`; the measurement of slots of the class, as built and
against a Release build; Compze's unit tests on a copy of that branch, 496 of 496; and the Fable reviewer's own checks
of the four findings marked run above, kept on the same branch under `experiments/taggregates-fable/ReviewChecks`.
