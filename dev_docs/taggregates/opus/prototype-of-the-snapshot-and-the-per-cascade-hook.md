# Prototype: loading a taggregate from a snapshot, and the hook once per publish cascade

Changes 1 and 3 of [what Compze needs](what-compze-needs.md), prototyped 2026-11-16 by taggregates-opus. Change 2, tevent
routing decided once per class, is jastudio-rep-opus's prototype: see Compze's
`dev_docs/taggregate-improvements/opus/5-prototype-of-per-class-routing.md`.

- **Where:** Compze, branch `taggregates-opus/snapshot-and-hook`, commit b1be4854c, in the worktree
  `C:\Dev\Compze.worktrees\taggregates-opus-snapshot-and-hook`. Branched from Compze's `main` at a6317da47.
- **Status:** an experiment, not for merging as it stands. Compze's 417 unit tests pass, 38 of them new.

## What it adds to Compze.Teventive

### `ITaggregate.LoadFromSnapshot(IEnumerable<ITaggregateTevent> snapshot, int version)`

```csharp
var note = new Note();
((ITaggregate)note).LoadFromSnapshot([
   new NoteTevent.Snapshot(id, "restored"),          //ITaggregateSnapshotTevent: the note's own state, and its identity
   new NoteTevent.Sense.Created(firstSenseId, "first"),  //one created tevent per entity
   new NoteTevent.Sense.Created(secondSenseId, "second")
], version: 1);
```

- **The first tevent is an `ITaggregateSnapshotTevent`,** a new marker beside `ITaggregateCreatedTevent`, and no later
  one is. `Taggregate.ApplyTevent` takes the identity from either marker.
- **Each tevent is stamped with the snapshot's identity and its one version,** wrapped in the taggregate's own wrapper,
  and applied by the tevent appliers only. No tevent handler or `TeventStream` observer sees it, and nothing is left
  uncommitted. Then the invariants are asserted.
- **Refused:** an empty snapshot, one that doesn't start with the marker, a marker on a later tevent, version 0, a
  tevent of another taggregate's type or identity, and a taggregate that isn't empty.
- **A shared tomponent's part of a snapshot is its own tevent in its slot's adopting wrapper,** written out by the
  caller, such as `new ShippingAddressTevent<PostalAddressTevent.Changed>(new PostalAddressTevent.Changed("221B Baker
  Street"))`. The slot knows its adopting wrapper's type; a convenience on the slot could write it. Not done.

### `Taggregate.OnPublishCascadeEnded(IReadOnlyList<ITaggregateTevent<TTaggregateTevent>> teventsOfTheCascade)`

- **A protected virtual method,** called once at the end of each publish cascade, after the invariants are asserted,
  with the cascade's tevents in the order published, each in its publisher's wrapper.
- **The base implementation emits them on `TeventStream`,** which is now created when first subscribed to, through
  `LazyInitializer.EnsureInitialized`.
- **Publishing on the taggregate from inside it starts a cascade of its own,** with its own call: the cascade's tevents
  are taken from the list before they're handed on. **So may `Commit` be called from inside it.**

### `Commit` takes the uncommitted tevents before handing them on

A publish from inside the commit callback is no longer cleared away uncommitted, or changes the list the callback is
enumerating: it stays uncommitted, for the next commit. The same shape as `Publish`'s fix.

### One thread at a time

`OneThreadAtATime`, a `ref struct` in `Compze.Teventive._private.Implementation`, entered by `Publish`, `Commit`,
`LoadFromHistory` and `LoadFromSnapshot`, and left when disposed:

- **A compare-exchange** puts the thread's managed id in an `int` field of the taggregate. Another thread entering
  meanwhile throws `InvalidOperationException`. The thread that holds it may enter again, as a tevent handler that
  publishes from inside a publish does, and only the entry that took it gives it back.
- **One thread at a time, not one thread for ever:** a full rebuild on a thread pool uses each note from whichever
  thread is free.
- **The cost:** a field, and one compare-exchange per entry. No allocation: the `ref struct` holds a `ref` to the
  field.

## Decisions made while building it

- **A cascade that fails takes its tevents with it.** `Publish` takes the cascade's tevents in a `finally`, so that
  when an applier, a handler or the invariants throw, the failed cascade's tevents don't go out with the next one.
  They stay applied and uncommitted: the taggregate is as corrupt as the failure left it, and the exception says so.
- **A commit callback that throws now loses the tevents it was handed.** Before, the list was cleared only after the
  callback returned, so a throwing callback left them uncommitted. Nothing relies on it: the tevent store's `Save`
  commits inside the caller's unit of work, which fails as a whole, and the taggregate instance with it. Put back in
  front on failure would double-commit what a nested commit committed meanwhile.
- **`InvalidOperationException` for another thread,** not a contract assertion: the assertion needs no message
  closure per call, and the failure is a misuse of the object's state, which is what that exception means.

## The specifications

In Compze's `test/Compze.Tests.Unit/CQRS/Taggregates/SnapshotsAndPublishCascades/`, with a fixture taggregate of their
own, `Note`: a text, senses as an entity collection, a tevent handler that can react to an edit by publishing, and an
override of `OnPublishCascadeEnded`.

- **Snapshots:** the state restored through the appliers; identity and version; no handler, cascade or uncommitted
  tevent; the next publish at version + 1; an entity collection of two; an order's shipping and billing addresses
  through their two slots (`OrderTaggregate`); a project's checklist of two shared tentities (`ProjectTaggregate`);
  and the refusals of an empty snapshot, a missing or misplaced marker, version 0, and a taggregate that isn't empty.
  The refusals of another taggregate's tevent type or identity have no specification.
- **Publish cascades:** a tevent handler's reaction is in the same cascade; an observer that publishes on the same
  note sees each tevent once, in order, and each ends a cascade of its own; committing from the hook, then reacting,
  commits each tevent once, in order; a commit callback that publishes while enumerating gets the earlier tevents and
  leaves the new one for the next commit.
- **One thread at a time:** while an edit on another thread waits inside its cascade, publishing or committing from
  this thread fails; once it's done, this thread publishes.

**Checked by mutation:** each specification group fails when the change it pins is undone. The old `Commit` fails
two; removing the thread check fails two; handing the live cascade list on makes the observer specification recurse
until the stack overflows, which crashes the test process. `dotnet test` reports that run as failed, but a summary line
for the filtered tests can still read "Passed!", with three fewer tests: read the run's result, not that line.

## Not in the prototype

- **Memory:** the lazy `TeventStream` saves one `SimpleObservable` per unobserved taggregate. It's measured together
  with jastudio-rep-opus's per-class routing, once the two branches are combined.
- **The convenience for a slot's part of a snapshot,** above.
- **Compze's documents and changelog,** which an implementation for merging would update.
