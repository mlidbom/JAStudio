# The notes as Compze Taggregates: what Compze.Teventive has to change

> **Read `../shared` first.** This folder is the fable team's own conclusions, as they stood before the two teams
> compared theirs. Where the two differ, `../shared` is what was agreed: a loaded note's first tevent carries a snapshot
> marker of its own, not the creation marker; an instance reaches its class's declaration through a virtual property;
> and JAStudio moves nothing onto Taggregates before routing for each class has landed. What changed, and why, is in
> `../shared/how-the-two-teams-compared.md`.

> **Status: exploration, started 2026-11-16. Nothing is built, in JAStudio or in Compze.** The findings and conclusions
> of the JAStudio session `taggregates-fable`, worked out in a dialog with the Compze session `jastudio-rep-fable`, and
> kept current as the exploration goes on. Another team explores the same question in `../opus`. These conclusions are
> independent of theirs: I have not read their documents or talked to them.

**The question (Magnus, 2026-11-16).** Should JAStudio's notes be Compze Taggregates, so that every change to a note is
a tevent, and whatever follows a change subscribes to exactly the tevents it concerns? The storage stays as it is: a
note's file holds its state, not its history.

**The scope of this folder.** What that needs of Compze.Teventive. How JAStudio would be built on it is left out, except
where it sets a requirement on Compze (Magnus, 2026-11-16).

## The headline

**The idea fits JAStudio, and Compze.Teventive's model of tevents already does what the notes need. What stands in the
way is what a Taggregate costs for each instance: as Compze.Teventive 0.6.0-alpha stands, the corpus's 80,000 notes
would add about 1 GB to a heap of 733 MB.** Everything else that is missing is small.

## Conclusions, the most important first

1. **Decide tevent routing once for each teventive class, not for each instance.** This decides whether JAStudio can
   use Taggregates at all. Measured: a Taggregate keeps 1.1 to 1.6 KB before it has a single part, each component about
   1.0 KB more, and each shared tomponent 0.55 to 0.97 KB more, against 350 bytes for comparable data as plain objects.
   Each instance builds its own tevent dispatchers, registrations and closures, though how a tevent is routed depends
   only on the class and the tevent's type. **A prototype of the change brings the corpus from about 1.1 GB to about
   26 MB over plain objects, and loads about fifteen times faster**
   ([what-the-prototype-of-routing-for-each-class-showed.md](what-the-prototype-of-routing-for-each-class-showed.md)).
2. **Let a taggregate load from tevents that its own code makes from stored state,** a compacted-history. It works
   today, but only through a member marked for Compze's internal use: without it the loaded taggregate refuses its
   next tevent (run and confirmed).
3. **Make publishing stand a publish from inside what it notifies.** Two flaws, both run and confirmed:
   - An observer of a taggregate's tevent stream that publishes on that taggregate is notified of the same tevent
     again. If it publishes once, the outer publish throws after the state has changed. If it publishes each time it
     is notified of the tevent it reacts to, which is the natural way to write it, **the process dies of a stack
     overflow.**
   - A tevent published from inside `ITaggregate.Commit`'s callback is applied, and never committed: nothing says so.
4. **Tell a taggregate's own class when it has published,** through a protected hook, so that a consumer needs no
   observer for each of 80,000 instances. What a throwing hook leaves behind has to be defined.
5. **Assert that one thread at a time publishes on a taggregate.** JAStudio has that race today, between a background
   reparse and an edit in the UI, and a Taggregate would be corrupted by it quietly.
6. **Make `ITaggregateTevent` a plain `ITevent`.** Today every taggregate's tevents declare themselves remotable,
   durable and transactional, which a note's are not. Nothing enforces it in memory, so it is a type that says more
   than the truth, not a blocker.
7. **Pin with specifications what the notes rely on.** Eight behaviors that the notes would stand on hold today (run and
   confirmed), and as far as the Compze session read, at least one has no specification.
8. **A source generator for the tevent implementation classes is a decision for later,** after one note type's tevents
   are written by hand.

**What needs no change:** `IMutableTeventDispatcher` as the one dispatcher for the whole collection; shared tomponents
in components, subclassed and generic; taggregate inheritance; subscribing by a meaning that several tevents share.
**What the notes don't need:** tevent handlers in components, entities, a second base class beside `Taggregate`, the
tevent store, the bus.

The reasons, and the fact about JAStudio behind each, are in
[what-the-notes-require-of-compze-teventive.md](what-the-notes-require-of-compze-teventive.md). The numbers, and how to
run them again, are in [measurements-and-checks.md](measurements-and-checks.md).

## How the changes would look

That is Compze's to design, and the Compze session `jastudio-rep-fable` has written its proposals in Compze's
`dev_docs/taggregate-improvements/fable/`, starting at its `README.md`. I have read them and agree with them. In short:

- **Routing:** each class declares its tevent appliers once, in a static declaration whose lambdas take the instance.
  A component, a collection of entities or a shared tomponent is an entry in the same ordered declaration, got from
  the instance by a lambda. A slot is a static of its owner's class. An instance keeps one reference.
- **Loading:** a protected member of `Taggregate` that takes a compacted-history, stamps id and versions, applies it,
  and publishes nothing. A slot adopts a tevent for loading as it does for publishing.
- **Publishing:** the notification becomes a queue, so that tevents arrive in the order published and the hook is
  never called from inside itself. `Commit` takes its list out before it calls back.

One finding of the prototype changed those proposals: loading needs no wrapper object, since a taggregate's own tevent
appliers can be routed by the tevent's own type. The Compze session took it in.

## What is verified, and how

| What | How |
|---|---|
| What a Taggregate, a component and a shared tomponent cost for each instance | Measured, 80,000 instances, Compze.Teventive 0.6.0-alpha from nuget.org |
| The eight behaviors the notes rely on, loading through the internal member, the two flaws | Run against the same package |
| What the cost is made of, what the changes would break in Compze | Read in Compze's code by the Compze session, which ran nothing |
| That routing for each class takes the cost away | Measured on a prototype outside Compze, with the same models, and on a second prototype by the Compze session, which agrees |
| The assertion of one thread at a time, the start-up cost of `TaggregateTypeValidator` | Not run |
| How many components and shared tomponents a note would have | Counted from today's constructors, by judgment of which part becomes what |

## Where everything is

- **These documents:** `dev_docs/taggregates/fable/`, on the branch `compze-taggretates`.
- **The experiments:** the branch `taggregates-fable-experiments`, checked out in the worktree
  `C:\Dev\JAStudio.worktrees\taggregates-fable-experiments`, under `experiments/taggregates-fable/`. No code in this
  checkout is changed.
- **Compze's side:** `C:\Dev\Compze\dev_docs\taggregate-improvements\fable\`, written by the Compze session, on Compze's branch
  `taggregate-improvements`. Its own prototype and runs are on Compze's branch `taggregate-experiments-fable`, in the
  worktree `C:/Dev/Compze.worktrees/taggregate-experiments-fable`.

## What Magnus has to decide

All of it is in Compze, and listed in the Compze session's `README.md`: whether to do routing for each class, which
changes how every taggregate is written; `ITaggregateTevent` as a plain `ITevent`, a change to a public interface; the
small additions to `Taggregate` and their names; and the generator, now, later or never. The two flaws need no decision.
By the Compze session's search, of 24 taggregate hierarchies in Compze's tests and sample about five are delivered
between endpoints, and would each need one line to say so once `ITaggregateTevent` is a plain `ITevent`.

**Where the exploration stands (2026-11-16):** the two sessions have agreed that they have nothing more to add, and
have stopped.

## Left on JAStudio's side, deferred

- **One note type's tevent interfaces, written to be read:** it waits for nothing in Compze, and it is where a
  generator's worth shows. Deferred by Magnus on 2026-11-16 while Compze's changes are explored.
- **The Python stub generator** would meet a generic base class from Compze under `JPNote`. Not checked.
