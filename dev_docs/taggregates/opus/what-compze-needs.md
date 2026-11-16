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
- **The change:** `ITaggregate.LoadFromSnapshot(snapshot, version)`. It wraps each tevent in the taggregate's own
  wrapper, stamps the version, runs the tevent appliers only, and asserts the invariants.
- **A snapshot is a short history, not necessarily one tevent:** the shortest history that produces the state. One
  tevent can restore a taggregate and its owned components, whose tevents are subtypes of the owner's, but not the
  rest. `Tentity`'s collection manager creates one entity per entity-created tevent, with the id that tevent carries.
  A shared tomponent receives only tevents wrapped in its slot's adopting wrapper, and two members of the same type,
  such as a shipping and a billing address, can't both be fed from one tevent. So the snapshot is:
  - first, the tevent with the snapshot marker, which establishes identity, and no later one has it;
  - one tevent per entity that implements that entity's created tevent, nested entities alike;
  - for each shared-tomponent slot, the tomponent's own snapshot tevent wrapped in the slot's adopting wrapper, and for
    a collection of shared tentities in a slot, one wrapped created tevent per tentity.

  Every part restores through its own appliers. All the snapshot's tevents get the one version: the snapshot as a whole
  stands for the first N tevents, and how many tevents it takes depends on the shape of the state, not on its history.
  Raised by taggregates-opus, decided by jastudio-rep-opus (2026-11-16). Restoring each part from a state object, outside
  the appliers, is rejected: it brings back two code paths for state.
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
- **A convenience method for subscribing to a shared tomponent's tevents across every owner.** No new routing is
  needed: routing unwraps one wrapper level, and a shared tomponent's tevent is wrapped twice, but
  `ForWrappedGeneric<IPublisherTevent<IPublisherTevent<IPostalAddressTevent>>>` matches every slot of every owner by
  covariance, with nothing declared on the owners, and naming an inner tevent type selects chosen tevents. Checked
  with 0.6.0-alpha by jastudio-rep-opus. A shorter way to write it would be ergonomics only. (taggregates-opus had
  proposed that each owner make its adopting wrappers implement a meaning of its own; it works, but needs a
  declaration on every owner.)
- **A marker for property-updated tevents,** only if it earns its place for other reasons too. A snapshot tevent that
  doesn't implement a newly added property-updated tevent leaves that property at its default on load, and the next
  save loses the data. The per-class tables could check it once per class if property-updated tevents were marked.
  Until then the consumer's round-trip specification (load, save, compare) is the safety net: JAStudio needs one.

## Two requirements of the per-cascade hook

Raised by taggregates-opus and agreed by jastudio-rep-opus on 2026-11-16, as part of change 3. Both are in the
[prototype](prototype-of-the-snapshot-and-the-per-cascade-hook.md).

- **Committing from inside the per-cascade hook must be allowed, and specified.** JAStudio's override of the hook
  commits there. The hook runs inside `Publish`, so Compze must say that `Commit` may be called from it, and that a
  publish the hook makes afterwards, which starts a cascade of its own, stays uncommitted until its own hook call
  commits it. A specification: commit in the hook, publish again in the hook, and every tevent is committed once, in
  order. Checking it, jastudio-rep-opus found that `Commit` has the same reentrancy bug as `Publish` (see
  [found along the way](found-along-the-way.md)).
- **A cheap assertion that one taggregate isn't published on by two threads at once.** `Taggregate`'s reentrancy level
  and its lists are plain fields, so two threads in `Publish` on one instance corrupt them silently. JAStudio could meet
  it: a full rebuild on background threads updates a vocab's generated data while the UI thread may edit that vocab;
  today the editors are disabled during a reparse, by hand, for this reason. Recording the publishing thread when a
  cascade starts, and failing when another thread enters while one is under way, costs a field and a comparison per
  `Publish`. It doesn't make taggregates thread-safe: it turns a silent corruption into a loud failure. A compare-exchange,
  so that two threads starting at the same moment can't both pass; `Commit`, `LoadFromHistory` and `LoadFromSnapshot`
  take it too.
