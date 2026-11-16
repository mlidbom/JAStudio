# How the two teams compared

> **Status: 2026-11-16.** What the two teams measured, where their conclusions differed, and what each took from the
> other. The conclusions they share are in [README.md](README.md).

## The teams

- **The fable team:** `taggregates-fable` in JAStudio and `jastudio-rep-fable` in Compze. Documents in `../fable` and
  in Compze's `dev_docs/taggregate-improvements/fable/`.
- **The opus team:** `taggregates-opus` in JAStudio and `jastudio-rep-opus` in Compze. Documents in `../opus` and in
  Compze's `dev_docs/taggregate-improvements/opus/`.

Each worked without reading the other's documents until Magnus asked them to compare.

## The measurements agree

Different models, measured separately, against Compze.Teventive 0.6.0-alpha. Bytes kept for each note.

| | Plain objects | 0.6.0-alpha | With routing for each class |
|---|---|---|---|
| Opus: 15 components of three text values each | 733 B | 11,457 B, and 18,551 B once each component has published | 1,080 B on Compze's own base classes, about 936 B in a final design |
| Opus: 6 components | 303 B | 5,240 B | 504 B |
| Fable: a root with four text values and four components | 350 B | 5,629 B | 472 B on a prototype outside Compze |
| Fable's Compze session: four components and four shared tomponents in one of them | 350 B | 10,204 B | 584 B |

- **A shared tomponent, measured by the fable team only:** 552 B before it has seen a tevent and 971 B after its first,
  against 48 to 56 B with a slot that is a static of its owner's class.
- **For the corpus:** the opus team's 0.6 to 0.9 GB counts components only. With the shared tomponents the notes need,
  the fable team comes to about 1.1 GB. With routing for each class both come to tens of MB over plain objects.
- **Loading:** 15 times faster on the fable prototype, 40 times on the opus one, which loads 80,000 notes of 15
  components in under a tenth of a second on one thread.

## What the opus team had that the fable team didn't

- **Prototypes inside Compze's own base classes,** on branches of Compze, combined, with Compze's full suite passing:
  routing for each class, loading from a snapshot, the hook, the fixes of both flaws, and the assertion of one thread
  at a time. The fable team's prototypes stand beside Compze's classes.
- **The lists of tevents created on first use and handed on whole,** which took what an edited note keeps from 216 B to
  8 B.
- **Routing by convention, prototyped and measured:** `Apply` and `Handle` methods found by reflection. The same cost,
  and a tevent handler whose name is misspelled never runs, without an error.
- **Components typed on the subclass that declares them:** Compze's nested `Component<…>` gives a `VocabNote`'s
  component a `JPNote` parent; the top-level base classes avoid it, checked.
- **That a snapshot tevent can miss a property,** and lose data at the next save, so JAStudio needs a round-trip
  specification.

## What the fable team had that the opus team didn't

- **That shared tomponents are the notes' normal case,** what they cost today, and a slot as a static of its owner's
  class.
- **The order of a note's tevents:** see below.
- **That the flaw in publishing ends in a stack overflow** when the observer publishes each time it is notified of the
  tevent it reacts to. The opus team had met the overflow only by breaking its own fix.
- **Eight behaviors the notes stand on, run against 0.6.0-alpha,** as candidates for specifications.
- **That a loaded tevent needs no wrapper object.**
- **What `ITaggregateTevent` as a plain `ITevent` would change in Compze:** about five of 24 taggregate hierarchies in
  its tests and sample need one line each.

## Where they differed, and how it was settled

| The difference | Settled as | Why |
|---|---|---|
| A publish from inside the hook: a call of the hook from inside itself (opus), or a queue (fable) | The queue | With the nested call a note's tevents published as A, B, C reach subscribers as A, C, B. The opus team wrote the specification, and its prototype failed it. It then built the queue into its prototype, where the specification and Compze's full suite pass |
| The loaded tevent: a snapshot marker of its own (opus), or the creation tevent (fable) | The marker | A restored note wasn't created; a note has several kinds of creation tevent; and only a snapshot tevent must set every property. Each team had first conceded this to the other |
| Loading: public on `ITaggregate` (opus), or protected and typed on the class (fable) | Protected, typed on the class | Only the note class's own factory loads, and the type is checked when compiling |
| Versions of the loaded tevents: one given by the caller (opus), or 1 to k (fable) | One for the whole snapshot: version 1, with no parameter until a tevent store has snapshots | With 1 to k the version would depend on how many slots a class has |
| A slot: an object for each instance (opus), or a static of the owner's class (fable) | The static | About 48 B for each slot, ten slots in a vocab |
| Tevent handlers in components: needed (opus), or not (fable) | Not needed by JAStudio | Reactions that reach other notes belong after the commit; inside a note it is one tevent that both parts apply, or the root's handler |
| When JAStudio starts: after routing has landed (opus), or one note type earlier (fable) | After routing with slots has landed | No note type is written twice |
| How much of a note is shared tomponents: "eventually" (opus), or from the first note type (fable) | From the first note type | Every note type has parts of one class several times, and media reference lists that all three share |

Settled between the two Compze sessions, with no requirement of JAStudio's:

- **How an instance reaches its class's declaration:** a virtual property that returns the class's static table (opus),
  not a constructor argument kept in every teventive (fable). It costs an instance nothing, and an assertion that the
  table is the instance's own class's makes a forgotten one fail loudly.
- **Loading without a wrapper object:** an optimization to offer later, after measuring.
- **The order in Compze:** the two flaws first, then the small additions, then routing in increments, with slots before
  anything of JAStudio's moves; `ITaggregateTevent` as a plain `ITevent` is independent of the rest.
- **Whether a taggregate refuses further use after an applier, a handler or an invariant threw:** for Magnus to
  decide. It is built and specified on the combined branch, where later `Publish` and `Commit` then refuse.

See Compze's shared folder.
