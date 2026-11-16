# What a taggregate keeps in memory: measured with Compze.Teventive 0.6.0-alpha

Measured 2026-11-16 by taggregates-opus, on .NET 11.

## The results

Bytes retained per note, over 10,000 notes. Each component has 3 string properties and one tevent applier per
property. The note is restored through `ITaggregate.LoadFromHistory` from one creation tevent that carries its state.

| Shape | Plain objects | Taggregate, loaded | More after one publish on each component |
|---|---|---|---|
| 15 components (vocab-like) | 733 B | 11,457 B | +7,094 B |
| 6 components (sentence-like) | 303 B | 5,240 B | +2,944 B |
| 1 component, 1 property | 47 B | 1,592 B | +721 B |

**For the corpus:** 28,004 vocab at 11.5 to 18.5 KB is about 320 to 520 MB; 49,162 sentences at 5.2 to 8.2 KB is
about 255 to 400 MB. Together about 0.6 to 0.9 GB more, on a managed heap of about 733 MB right after a start
(`dev_docs/memory-use.md`). A full rebuild publishes on every note, so the higher figures. The kanji are left out:
2,554 of them add little. For comparison, the largest item `memory-use.md` removed was 300 MB of field notifiers.

## Where it goes

Read from the code, and agreed by jastudio-rep-opus:

- **Each taggregate creates two tevent dispatchers,** one for appliers and one for handlers, and **each component one
  more**, which it registers on its parent's.
- **Each dispatcher has its own subscriptions:** three lists, and a set built from its configuration.
- **Each registration adds objects:** a subscriber with a list per `RegisterTeventAppliers()` call, and a registered
  handler, a delegate and a closure over `this` per applier.
- **The growth after publishing** is each dispatcher's cache of the handlers a wrapper tevent type routes to, built per
  instance on its first dispatch of each type, and kept.

## Not measured

- **Startup time:** 80,000 notes with some 50 registrations each, every one taking its dispatcher's lock.
- **The real notes' shapes:** the benchmark's components are minimal. Their own state is the same in both columns, so
  the difference holds, but the totals for real notes are larger.
- **The redesign's figure,** about 200 B per note over plain objects, is jastudio-rep-opus's estimate. The redesign is
  to add a Compze specification of the bytes retained per instance, with a bound.

## How it was measured, and where the benchmark is kept

A Python script writes a C# program for a given number of components and properties per component. The program
creates the notes, keeps them, and compares `GC.GetTotalMemory(true)` before and after, then publishes once on each
component and commits, and measures again. A plain object graph of the same shape is the baseline.

The benchmark is kept in Compze, beside the write-up of the changes:
`C:\Dev\Compze\dev_docs\taggregate-improvements\opus\benchmark\`, with a README on how to run it. It must run outside
both repositories: their `Directory.Build.props` would apply to it.
