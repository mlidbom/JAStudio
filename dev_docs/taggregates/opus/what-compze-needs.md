# What Compze needs for JAStudio's notes as taggregates

Agreed on 2026-11-16 by taggregates-opus (in JAStudio) and jastudio-rep-opus (in Compze), except where marked open.
The full write-up of each change, with its design, what it touches and what was rejected, is in Compze:
`C:\Dev\Compze\dev_docs\taggregate-improvements\opus\`. This document says what JAStudio requires and why, and points
there for the rest.

A term from those documents, used here as they define it: a **publish cascade** is one call to `Taggregate.Publish`
together with every `Publish` that tevent handlers make from inside it.

## JAStudio's use: what the changes must serve

- **About 80,000 notes held in memory at once:** 28,004 vocab, 49,162 sentences and 2,554 kanji, on a managed heap of
  about 733 MB (`dev_docs/memory-use.md`).
- **Each note stored as its state, in a JSON file,** never as a history. There is no tevent store, and won't be.
- **Every note restored at startup.**
- **Tevents for listening in process:** inside a note, one component following another's changes; across the corpus,
  the indexes, the UI, and reparsing sentences after a vocab changes.
- **Shape:** `JPNote` is the root of an inheritance hierarchy, with `VocabNote`, `SentenceNote` and `KanjiNote` below it.
  A vocab has about 15 components, some nested. Some parts recur across note types, such as audio lists and media
  lists, so shared tomponents eventually.
- **Edits are rare and one at a time, from the UI.** A full rebuild recomputes the generated data of every note, in
  parallel on many threads, with each note touched by one thread only.
- **Several JAStudios run in one process** in the specifications, so nothing may rest on static state.
- **JAStudio consumes Compze as NuGet packages.** Nothing in JAStudio uses Compze.Teventive yet.

## The changes, in order

### 1. Loading a taggregate from a snapshot

Compze document: `1-loading-a-taggregate-from-a-snapshot.md`.

- **Why:** a taggregate can come into being only by publishing its creation tevent, or from a tevent store's history.
  Restoring from stored state works through `LoadFromHistory` only after setting the tevent's version through
  `IMutableTaggregateTevent.SetTaggregateVersionInternal`, which is `[Obsolete]` for Compze's internal use.
- **The change:** `ITaggregate.LoadFromSnapshot(snapshot, version)`. It wraps the tevent in the taggregate's own
  wrapper, stamps the version, runs the tevent appliers only, and asserts the invariants.
- **A snapshot marker of its own,** not `ITaggregateCreatedTevent`: a restored note wasn't created, and Compze reads
  `ITaggregateCreatedTevent` as creation. Both markers establish the taggregate's identity. The name is Magnus's call:
  `ITaggregateSnapshotTevent` or `ITaggregateRestoredTevent`.
- **JAStudio passes 1 as the version,** and doesn't store a version in a note's file. Its "changed since saved" is
  already in the taggregate, as the tevents not yet committed. A stored version would change with every edit, which is
  noise in the corpus diffs Magnus reviews, and would change the corpus format, which is his decision.

### 2. Tevent routing decided once per class

Compze document: `2-tevent-routing-decided-once-per-class.md`.

- **Why:** every taggregate and every component builds its own tevent dispatchers, subscriptions and resolved-handler
  caches, though everything in them is decided by the class. That's the memory in [the measurements](measurements.md),
  and the wrong abstraction whatever it costs: `IMutableTeventDispatcher` is for subscriptions that come and go at run
  time, and an applier never goes.
- **The change:** a table of tevent appliers per class, declared once and shared by every instance, with lambdas that
  take the instance as a parameter instead of capturing `this`. A parent declares the route to each component, entity
  collection and shared-tomponent slot. The handlers each wrapper tevent type routes to are resolved once per class.
- **Tevent handlers in components too, which is new:** today only `Taggregate` can register tevent handlers.
  JAStudio's reactions to a note's own changes live in its components, such as `PerfectSynonyms` pushing a vocab's
  answer to its synonyms, which are other notes. Handler tables are composed through components as applier tables are.
  A handler runs inside the publish cascade, before the invariants are asserted; the hook of change 3 runs after.
- **Components typed on the class that declares them:** Compze's nested `Component<…>`, `Entity<…>` and
  `RemovableEntity<…>` give every component of `VocabNote` a `JPNote` parent, the class that closes `Taggregate<…>`.
  The top-level base classes already avoid it (checked with 0.6.0-alpha by jastudio-rep-opus); in the redesign that
  should be the normal path.
- **Safe with many threads at once** on different instances of one class, as JAStudio's full rebuild needs: resolved
  under a lock once, then read without one.
- **What's left per instance:** the id, the version, two lists and the lazily created `TeventStream`, and a parent
  reference per component. Estimated at about 200 B per note over plain objects, to be confirmed by the benchmark.
- **The one taste call for Magnus:** appliers declared explicitly in the table, as both sessions recommend, or found
  by convention as `Apply(…)` instance methods.

### 3. Told once per publish cascade

Compze document: `3-told-once-per-publish-cascade.md`.

- **Why:** JAStudio refiles a note, commits, saves its file and dispatches its tevents once per change. `TeventStream`
  emits a publish cascade's tevents one at a time, so an observer can't tell where the cascade ends; and subscribing to
  80,000 streams costs a subscription each.
- **The change:** a protected virtual hook in `Taggregate`, called once at the end of each publish cascade, after the
  invariants are asserted, with a copy of the cascade's tevents. Its base implementation feeds `TeventStream`, created
  lazily. `JPNote` overrides it.
- **It fixes a bug in `Taggregate.Publish`:** see [found along the way](found-along-the-way.md).

### Later

Compze document: `4-later.md`. None of these is needed for JAStudio's first slice.

- **`ITaggregateTevent` extends `IExactlyOnceTevent`,** which claims remote, transactional, exactly-once delivery that a
  note's in-memory tevents don't have. Each tevent also gets a `TessageId`. Consistent with Compze's own decision D6:
  a tevent's interfaces choose its delivery guarantees.
- **Compze's README calls the in-memory overhead "entirely negligible":** true per dispatch, not per instance held.
- **A source generator for tevent classes:** the class hierarchy mirrors the interface hierarchy mechanically.
- **Subscribing to a shared tomponent's tevents across every owner:** see the next section.

## Open: a snapshot of entity collections and shared tomponents

Raised with jastudio-rep-opus on 2026-11-16, not yet answered. A Compze requirement whatever JAStudio's design: any
snapshot feature, a tevent store's included, will meet it.

One snapshot tevent can restore a taggregate and its owned components, whose tevents are subtypes of the owner's, but
not the rest:

- **Entity collections:** `Tentity`'s collection manager creates one entity per entity-created tevent, with the id that
  tevent carries. One tevent creates at most one entity.
- **Shared tomponents:** a slot routes to its tomponent only tevents wrapped in the slot's adopting wrapper tevent. Two
  members of the same tomponent type, such as a shipping and a billing address, both close `IPublisherTevent<T>` over
  the same inner type, so one snapshot tevent can't hand each slot a different inner tevent.

Read from the code of 0.6.0-alpha (`Tentity.CollectionManager`, `SharedTomponentSlot`), not run.

**Proposed:** a snapshot as the shortest history that produces the state, not necessarily one tevent:
`LoadFromSnapshot(IEnumerable<ITaggregateTevent> snapshot, int version)`. First the tevent that establishes identity,
then one created tevent per entity carrying its state, then for each slot the tomponent's own snapshot tevent wrapped
in that slot's adopting wrapper. Every part still restores through its own appliers. How the snapshot's tevents are
numbered is left open. The alternative, restoring each part from a state object outside the appliers, brings back two
code paths for state.

## Considerations raised, for the Compze session to weigh

- **Subscribing to a shared tomponent's tevents across every owner may need no new subscription kind.** Routing
  unwraps one wrapper level, and a shared tomponent's tevent is wrapped twice, so Compze's documents list a new kind
  of subscription for it, later. But a slot's adopting wrapper tevent type is itself an owner tevent, so the owner can
  make it implement a meaning of its own hierarchy, such as "an address changed". A subscriber across every owner then
  subscribes to that meaning, one wrapper level in. It costs a declaration per owner, and covers every tevent of the
  slot.
- **A snapshot that misses a property loses it silently.** A snapshot tevent that doesn't implement a newly added
  property-updated tevent leaves that property at its default on load, and the next save loses the data. A consumer's
  round-trip specification (load, save, compare) catches it. Compze could check it only if property-updated tevents
  were recognizable as such.
