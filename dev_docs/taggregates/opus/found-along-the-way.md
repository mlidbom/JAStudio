# Found along the way

Two bugs, found 2026-11-16 while exploring taggregates. Neither is fixed, and neither is marked at its code: this
exploration makes no code changes (Magnus, 2026-11-16).

## JAStudio: a vocab's forms not owned by other vocab go stale

`VocabNoteForms` caches the set of its matched forms that no other vocab owns (`_notOwnedByOtherVocab`, computed by
`ComputeNotOwnedByOtherVocab`). It's computed from other vocab's questions and forms, but only this vocab's own
changes reset it (`InvalidateCaches`). When another vocab named by one of these forms adds or drops this vocab's
question as a form, the cached set stays as it was until this vocab changes or the collection is reloaded.
`SentenceCollection.WithVocabOwnedForm` then lists sentences by the stale set.

Found by reading the code, not reproduced. It's an example of what taggregates would fix: data derived from other
notes, which only a subscription to their changes can keep current. When code changes are allowed again, it gets a
`bug:` comment at `InvalidateCaches`, or a fix.

## Compze: `Taggregate.Publish` breaks when an observer publishes on the same taggregate

At the end of a publish cascade, `Publish` enumerates the cascade's tevents with a `foreach` and emits each on
`TeventStream`. An observer that publishes on the same taggregate from inside the emission adds to the list being
enumerated, and the inner `Publish` emits the outer cascade's tevents a second time. The outer `foreach` then throws
`InvalidOperationException`, out of the domain method that published, after the state has changed.

Found by reading the code, and reproduced with 0.6.0-alpha by jastudio-rep-opus: the observer of a rename saw
`Renamed, Renamed, Updated`, and the rename threw. JAStudio would meet it: a subscriber across the corpus that reacts
to a vocab's change may update that vocab's generated data.

The fix is part of the hook called once per publish cascade: copy the cascade's tevents, clear the list, then call the
hook with the copy, so a publish from inside the hook starts a cascade of its own. Recorded in Compze's
`3-told-once-per-publish-cascade.md`, with a specification to pin it.
