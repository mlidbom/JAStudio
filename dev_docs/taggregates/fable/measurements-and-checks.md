# Measurements and checks

> **Status: run 2026-11-16, against Compze.Teventive 0.6.0-alpha from nuget.org.** What was run, what it gave, and how
> to run it again. What follows from it is in [README.md](README.md).

## Where the programs are, and how to run them

On the branch `taggregates-fable-experiments`, in the worktree `C:\Dev\JAStudio.worktrees\taggregates-fable-experiments`,
under `experiments/taggregates-fable/`. Each is a console program of its own, outside JAStudio's solution, that
references the Compze.Teventive package.

| Program | What it does |
|---|---|
| `TaggregateCost` | What a Taggregate and its components cost for each instance, against plain objects |
| `TeventiveChecks` | The behaviors the notes rely on, loading from a compacted-history, the two flaws, and what a shared tomponent costs |
| `RoutingForEachClassPrototype` | What the same two models cost with tevent routing decided once for each class: see [what-the-prototype-of-routing-for-each-class-showed.md](what-the-prototype-of-routing-for-each-class-showed.md) |

```
dotnet build -c Release
dotnet bin/Release/net11.0/<program>.dll
```

**How the costs are measured:** 80,000 instances are created and held. Kept is the growth of the heap after a full
garbage collection, divided by the count. Allocated is the growth of the process's allocated bytes. Kept and allocated
don't depend on what else the machine does; the times do, and the machine's load was 3 to 7 percent during the runs.

## What a Taggregate and its components cost (`TaggregateCost`)

A root with four text values and four parts of two values each.

| For each instance | Kept | Allocated | Time |
|---|---|---|---|
| Plain objects | 350 B | 352 B | 0.2 µs |
| A Taggregate with 4 tevent appliers, loaded from one creation tevent | 1,632 B | 2,864 B | 3.0 µs |
| The same, created by publishing | 2,256 B | 3,069 B | 3.2 µs |
| With 4 components of 2 tevent appliers each, loaded | 5,629 B | 8,504 B | 11.1 µs |
| The same, created by publishing | 6,122 B | 8,736 B | 13.0 µs |
| The first change published on each loaded instance, one subscriber | 528 B more | 1,160 B | 0.9 µs |
| A second change of the same type | 109 B more | 384 B | 0.3 µs |

- **A component costs about 1.0 KB:** (5,629 - 1,632) / 4.
- **Created by publishing keeps more than loaded,** because the published tevents stay in the list of uncommitted
  tevents until `Commit` is called. The 109 bytes of a second change are that too.
- **The first change of a type keeps 528 bytes,** most of it the dictionaries of resolved handlers that each instance
  copies with one entry more.

## What a shared tomponent costs (`TeventiveChecks`)

A root with one tevent applier, and none or four shared tomponents, each holding an integer, each in a slot of its own.

| For each instance | Kept | Allocated | Time |
|---|---|---|---|
| No shared tomponent, loaded from its creation tevent | 1,111 B | 2,160 B | 2.3 µs |
| 4 shared tomponents, loaded from its creation tevent only | 3,320 B | 5,561 B | 7.5 µs |
| 4 shared tomponents, loaded from its creation tevent and one tevent adopted for each | 4,995 B | 10,443 B | 12.2 µs |

- **A shared tomponent that has seen no tevent keeps 552 B,** and one that has seen its first 971 B.
- **Loading one adopted tevent allocates 1,220 B and takes 1.2 µs.**

## The behaviors the notes rely on (`TeventiveChecks`)

The model is shaped as the notes are: a base class `Note` with a tevent applier of its own, under `Vocab` and
`Sentence`, each with its wrapper; components; two shared tomponents of one class in a component, one of them a
subclass; a generic shared tomponent in a slot of the root; and one dispatcher for the whole collection that every
note's tevents go to in their wrappers. All eight hold:

1. **One tevent that two components apply:** a rename, published by the question, is applied by the forms too, since
   its interface inherits an interface of the forms' tevents.
2. **A subscription to a meaning that several tevents share** gets each of them: the creation, the rename and an
   added form are each a change to what the vocab matches.
3. **A base class's tevent applier runs in a subclass,** and a subscriber tells its tevent apart by the subclass that
   published it: `ForWrapped<IVocabTevent<INoteTevent.TagAdded>>`.
4. **Two shared tomponents of one class, in a component, are told apart by their slots,** and a subclass of the shared
   tomponent works.
5. **A subscription to an interface of the owner's that a slot's adopting wrapper tevent implements** gets the tevents
   adopted through that slot, with `For`, and the handler receives the adopting wrapper tevent.
6. **A subscription to a shared tomponent's own tevent type** gets nothing of what is adopted, as Compze's routing
   document says.
7. **A generic shared tomponent, with generic tevents, in a slot of the root.**
8. **A generic tevent interface that isn't a wrapper is matched by covariance:** a subscription to
   `IMediaAdded<MediaRef>` gets a `MediaAdded<AudioRef>`.

## Loading from a compacted-history, with what Compze has today (`TeventiveChecks`)

- **A vocab loads** from a creation tevent and a tevent adopted for each word of each word set, through
  `ITaggregate.LoadFromHistory`: its state is right, and its version is the number of tevents.
- **Loading leaves nothing uncommitted,** and the loaded vocab publishes.
- **Without the versions stamped** through `IMutableTaggregateTevent.SetTaggregateVersionInternal`, the loaded vocab
  refuses its next tevent: "The first published tevent NoteTevent+TagAdded did not implement ITaggregateCreatedTevent".

## The two places that don't stand a publish from inside them (`TeventiveChecks`)

- **An observer that publishes once, on the taggregate it observes:** notified of `first, first, second`; the tags are
  `first, second`; the publish threw `InvalidOperationException`: "Collection was modified; enumeration operation may
  not execute."
- **An observer that publishes each time it is notified of the tevent it reacts to,** run in a process of its own:
  exit code -1073741571, "Stack overflow."
- **A tevent published from inside `Commit`'s callback:** the callback was handed 1 tevent; the tags are
  `before, during the commit`; the next `Commit` was handed 0; nothing was thrown.

## Not run

- **Routing for each class in Compze itself:** it is measured on a prototype only.
- **Two threads publishing on one taggregate.**
- **What `TaggregateTypeValidator` costs at start-up** for an assembly of JAStudio.Core's size, about a thousand types.
