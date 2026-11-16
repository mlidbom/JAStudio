# What the prototype of routing for each class showed

> **Status: an experiment, run 2026-11-16.** A prototype outside Compze, written to measure what the proposal would
> cost for each instance. It is not Compze's code, and not its design in detail. The conclusions are in
> [README.md](README.md).

## The headline

**Routing decided once for each class takes the cost away: the corpus's notes would cost a few tens of MB more than
plain objects, where Compze.Teventive 0.6.0-alpha costs about 1.1 GB, and they load about fifteen times faster.**

## What was built

The program `RoutingForEachClassPrototype`, on the branch `taggregates-fable-experiments`, under
`experiments/taggregates-fable/`. About 150 lines of base classes, which have the shape the Compze session proposes in
Compze's `dev_docs/taggregate-improvements/fable/tevent-routing-per-class.md`:

- **Each class declares its tevent appliers once,** in a static declaration whose lambdas take the instance.
- **A component and a shared tomponent are entries in that declaration,** got from the instance by a lambda.
- **A slot is a static of its owner's class,** and is the one place that wraps a tevent in its adopting wrapper tevent.
- **A subclass's declaration starts from its base class's.**
- **An instance keeps a reference to its class's declaration.** A component also keeps its parent, a shared tomponent
  its owner and its slot.
- **The entries that match a tevent's type are resolved once for each class,** into an array kept in a dictionary that
  is replaced, never changed, so it is read without a lock.

The tevents are Compze's own types (`TaggregateTevent`, the wrappers, `IPublisherTevent`), and the two models are the
ones measured with Compze.Teventive 0.6.0-alpha, so the numbers compare. The program checks that the loaded and
published state is right before it reports.

## The numbers, for each instance

80,000 instances. Bytes don't depend on what else the machine does. The times do: other work ran on the machine during
the prototype's runs, at 12 to 73 percent load, and two runs gave the same times to within a tenth.

| | 0.6.0-alpha kept | Prototype kept | 0.6.0-alpha time | Prototype time |
|---|---|---|---|---|
| A root with 4 tevent appliers, loaded | 1,632 B | 119 B | 3.0 µs | 0.5 µs |
| With 4 components of 2 tevent appliers each, loaded | 5,629 B | 472 B | 11.1 µs | 0.6 µs |
| The same, created by publishing | 6,122 B | 728 B | 13.0 µs | 0.7 µs |
| The first change published on each loaded instance | 528 B more | 200 B more | 0.9 µs | 0.4 µs |
| A second change of the same type | 109 B more | 112 B more | 0.3 µs | 0.3 µs |
| A root with 1 tevent applier, loaded | 1,111 B | 88 B | 2.3 µs | 0.3 µs |
| With 4 shared tomponents, loaded from its creation tevent | 3,320 B | 312 B | 7.5 µs | 0.3 µs |
| The same, and one tevent adopted for each | 4,995 B | 312 B | 12.2 µs | 1.1 µs |

- **The same data as plain objects keeps 350 B,** against the prototype's 472 B for a root with four components. The
  122 bytes are the taggregate's id object and the references that the root and each component keep.
- **A shared tomponent keeps 56 B,** whether or not it has seen a tevent, against 552 B and 971 B.
- **Loading one adopted tevent allocates 232 B and takes 0.2 µs,** against 1,220 B and 1.2 µs. The 232 bytes are the
  tevent and its adopting wrapper tevent themselves.
- **What a change keeps is the uncommitted tevent** in its wrapper, and the first time the list that holds it. A commit
  releases it.

**For the corpus,** with the parts counted as in
[what-the-notes-require-of-compze-teventive.md](what-the-notes-require-of-compze-teventive.md): about 500 bytes more
than plain objects for a vocab, 230 for a sentence, 280 for a kanji. About 26 MB, where 0.6.0-alpha gives about 1.1 GB.

## What building it showed about the design

1. **Loading needs no wrapper.** A taggregate's own tevent appliers are routed by the tevent's own type, and a slot's
   entry matches the adopting wrapper tevent, which is the tevent itself at the root. So the prototype's
   `LoadFromCompactedHistory` wraps nothing. The Compze session's proposal says that loading wraps each tevent as
   `Publish` would. For the 570,000 tevents JAStudio loads at each start, leaving that out saves an object and two
   lookups for each. A wrapper is needed at loading only if an entry asks for one.
2. **A subclass's declaration can reuse its base class's entries as they are.** An entry made for the base class casts
   the instance to the base class, which a subclass's instance is. `ThenFor<TSubclass>()` copies the list.
3. **A slot handed to a shared tomponent as a struct of the owner and the slot allocates nothing,** and the shared
   tomponent doesn't know its owner's type.
4. **`Commit` can hand over the list itself and start a new one,** in place of copying: the taggregate forgets the
   list before it calls back, so a tevent published from inside the callback goes into a new list.
5. **Static fields are initialized in the order written,** so a slot is declared above the declaration that enters it.
   Written the other way round, the entry is null: the declaration should assert that, with a message.
6. **A component's entry has to know the component's root tevent interface.** The prototype takes it as a type
   argument. In Compze it can come from the component class's own type arguments.
7. **The lambdas capture nothing when written `static`,** which the compiler checks.

## What the prototype doesn't have

Tevent handlers and the queue of notifications; `ForWrapped`, `BeforeHandlers`, `AfterHandlers` and letting a tevent go
unhandled; entities and their collections; the assertion of one thread at a time; the tevent stream; anything of the
tevent store. None of these needs anything for an instance that the prototype doesn't already keep, except the tevent
stream, which the Compze session proposes to create on first use.
