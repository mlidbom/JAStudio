# Taggregates for JAStudio's notes: what Compze needs

> **Status: design and exploration, 2026-11-16. Nothing is implemented.** Written by the session taggregates-opus, in
> JAStudio, in a dialogue with the session jastudio-rep-opus, in Compze. The Compze changes they agreed on are written
> up in Compze: `C:\Dev\Compze\dev_docs\taggregate-improvements\opus\`. Another pair of sessions explores the same
> question in parallel (taggregates-fable and jastudio-rep-fable), with documents of their own.

Magnus's idea, 2026-11-16: JAStudio's notes as Compze.Teventive taggregates, so that every change to a note is a
tevent and whatever follows a change subscribes to exactly the tevents it concerns. The storage stays as it is: a
note's file holds its state, not its history. Magnus then narrowed this exploration to what Compze needs: JAStudio's
own design counts only where it changes what Compze must do.

## Conclusions

1. **Taggregates fit JAStudio's notes well.** A note's stored state can be restored through the same tevent appliers
   that editing uses, and the hand-written wiring between a note's parts, and from editors to whatever must follow
   their changes, would become subscriptions. See [what taggregates would give the notes](what-taggregates-would-give-the-notes.md).
2. **Compze.Teventive 0.6.0-alpha can't hold JAStudio's 80,000 notes.** A vocab-shaped taggregate keeps 11.5 KB just
   to exist and 18.5 KB once each component has published, against 733 bytes for the same state as plain objects:
   about 0.6 to 0.9 GB for the corpus, on a heap of 733 MB. See [the measurements](measurements.md).
3. **Compze needs three changes first, in this order:** loading a taggregate from a snapshot, tevent routing decided
   once per class instead of per instance, and a hook called once per publish cascade. Then smaller changes later. See
   [what Compze needs](what-compze-needs.md).
4. **A snapshot is a short history, not one tevent:** one tevent can't restore a taggregate's entity collections or
   its shared tomponents. Raised by taggregates-opus, and decided with jastudio-rep-opus. Two smaller requirements
   are raised and not yet answered: committing from inside the per-cascade hook, and an assertion against two threads
   publishing on one taggregate. See [what Compze needs](what-compze-needs.md).
5. **Nothing in JAStudio moves onto taggregates before the snapshot and the per-class routing have landed.**

## The documents

- [What taggregates would give the notes](what-taggregates-would-give-the-notes.md): what JAStudio does today, and what
  listening to exactly what changed would clean up. Why this is worth Compze's changes.
- [What Compze needs](what-compze-needs.md): JAStudio's use, each change Compze needs and why, and what is open.
- [The measurements](measurements.md): how much a taggregate keeps in memory, how it was measured, and where the
  benchmark is kept.
- [Prototype of the snapshot and the per-cascade hook](prototype-of-the-snapshot-and-the-per-cascade-hook.md): changes
  1 and 3 built and specified in a Compze branch, and what was decided while building them.
- [Found along the way](found-along-the-way.md): bugs in JAStudio and in Compze, and a broken Compze solution.
