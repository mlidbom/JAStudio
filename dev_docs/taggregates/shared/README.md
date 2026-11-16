# The notes as Compze Taggregates: the two teams' shared conclusions, for JAStudio

> **Status: exploration, 2026-11-16. Nothing is built, in JAStudio or on any branch of Compze that anyone uses.** Agreed
> by the four sessions that explored the question as two teams: `taggregates-fable` and `taggregates-opus` in JAStudio,
> `jastudio-rep-fable` and `jastudio-rep-opus` in Compze. Each team first reached conclusions of its own, in `../fable`
> and `../opus`; this folder holds what they agreed on after comparing them. Kept by `taggregates-fable`, which
> represents JAStudio. Compze's side is in Compze's `dev_docs/taggregate-improvements/shared/`.

**The question (Magnus, 2026-11-16).** Should JAStudio's notes be Compze Taggregates, so that every change to a note is
a tevent, and whatever follows a change subscribes to exactly the tevents it concerns? The storage stays as it is: a
note's file holds its state, not its history. The exploration was narrowed to what that needs of Compze.

## The conclusions

1. **The idea fits JAStudio.** A note's stored state can be loaded through the same tevent appliers that editing uses,
   and the hand-written wiring between a note's parts, and from the editors to what must follow their changes, becomes
   subscriptions. Compze.Teventive's model of tevents already does what the notes need: the behaviors that the notes
   would stand on were run against Compze.Teventive 0.6.0-alpha, and all hold.
2. **Compze.Teventive 0.6.0-alpha can't hold the corpus.** Each instance builds its own tevent dispatchers, though how
   a tevent is routed depends only on the class and the tevent's type. Both teams measured it, with different models:
   the 80,000 notes would add 0.6 to 1.1 GB to a heap of 733 MB.
3. **Tevent routing decided once for each class takes that away.** Four prototypes, two of them inside Compze's own
   base classes with Compze's full suite passing, bring a note to within 120 to 300 bytes of the same data as plain
   objects: 20 to 30 MB for the corpus, and loading 15 to 40 times faster.
4. **Three changes in Compze come first,** all prototyped: routing for each class; loading a taggregate from a
   snapshot of its state; and a hook that tells a taggregate's class when it has published, with publishing made to
   stand a publish from inside what it notifies.
5. **Two flaws in Compze.Teventive were found and run,** and are wrong whatever JAStudio does: an observer that
   publishes on the taggregate it observes can kill the process with a stack overflow, and a tevent published from
   inside `ITaggregate.Commit`'s callback is applied and never committed.
6. **Nothing in JAStudio moves onto Taggregates before routing for each class, slots included, has landed in Compze.**

What the notes require of Compze, each with the fact about JAStudio behind it, is in
[what-the-notes-require-of-compze.md](what-the-notes-require-of-compze.md). How the teams' numbers compare, and what
each team gave up to the other, is in [how-the-two-teams-compared.md](how-the-two-teams-compared.md).

## What Magnus has to decide

All of it is in Compze, and Compze's shared folder lists it with the detail. In short:

- **Whether to do routing for each class.** It changes how every taggregate is written: 56 registrations in 41 files of
  Compze, its tests and its sample.
- **Appliers declared explicitly or found by convention.** All four sessions recommend the declaration. Both were
  prototyped and cost the same; the convention lets a misspelled tevent handler never run, without an error.
- **`ITaggregateTevent` as a plain `ITevent`,** a change to a public interface, which both Compze sessions recommend
  whatever JAStudio does.
- **The names:** of the snapshot marker, the hook, and the declaration's members.
- **A source generator for the tevent implementation classes:** now, later or never. Both teams say later, after one
  note type's tevents are written by hand.

## Where everything is

| What | Where |
|---|---|
| The shared conclusions for JAStudio | `dev_docs/taggregates/shared/`, here |
| The shared conclusions for Compze | Compze: `dev_docs/taggregate-improvements/shared/` |
| The fable team's documents | `dev_docs/taggregates/fable/`, and Compze: `dev_docs/taggregate-improvements/fable/` |
| The opus team's documents | `dev_docs/taggregates/opus/`, and Compze: `dev_docs/taggregate-improvements/opus/` |
| The prototypes inside Compze's base classes, combined, with the suite passing: routing for each class, the snapshot, the hook with its queue, both flaws fixed, the assertion | Compze: branch `taggregates-opus/combined`, at 604c00df4 |
| Static slots in Compze's base classes, built on the combined branch, measured | Compze: branch `taggregate-experiments-fable-static-slots`, at fb75d60a0 |
| The fable team's prototypes and checks beside Compze's classes | JAStudio: branch `taggregates-fable-experiments`; Compze: branch `taggregate-experiments-fable` |

No code is changed in this checkout, or on Compze's `main`.

## Left on JAStudio's side, deferred

- **One note type's tevent interfaces, written to be read.** It waits for nothing in Compze, and it is what a source
  generator's worth should be judged by. Deferred by Magnus on 2026-11-16 while Compze's changes are explored.
- **A round-trip specification:** load each note, save it, and compare the files. It is the guard against a snapshot
  tevent that misses a property, which would lose data at the next save without a word.
- **The Python stub generator** would meet a generic base class from Compze under `JPNote`. Not checked.

## Found along the way, in JAStudio

- **A vocab's forms that no other vocab owns go stale** (`VocabNoteForms`, `_notOwnedByOtherVocab`). The set is computed
  from other vocab's questions and forms, but only the vocab's own changes reset it. When another vocab adds or drops
  this vocab's question as a form, `SentenceCollection.WithVocabOwnedForm` lists sentences by the stale set until the
  vocab changes or the collection is loaded again. Found by the opus team by reading the code, not reproduced, and not
  fixed or marked at the code, since this exploration changes no code: see `../opus/found-along-the-way.md`. It is
  what tevents are for: data derived from other notes, which only hearing of their changes keeps current.
