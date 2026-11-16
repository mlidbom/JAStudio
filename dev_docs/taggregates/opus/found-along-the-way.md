# Found along the way

Found 2026-11-16 while exploring taggregates. None is fixed on a branch anyone uses, and none is marked at its code:
this exploration makes no code changes (Magnus, 2026-11-16). The two Compze reentrancy bugs are fixed in an experiment
branch only.

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

## Compze: `Commit` has the same reentrancy bug as `Publish`

Found by jastudio-rep-opus while checking that committing from the per-cascade hook works, and reproduced with
0.6.0-alpha. `Commit` hands its callback the live list of uncommitted tevents and clears it after the callback returns.
A publish on the same taggregate from inside the callback either throws, when the callback is enumerating the tevents,
or is cleared away and never committed. The tevent store's own `Save` could meet it: its commit callback publishes the
tevents on the bus, and an in-process handler that changes the same taggregate would publish from inside it. Fixed in
the [prototype](prototype-of-the-snapshot-and-the-per-cascade-hook.md) the same way as `Publish`.

## Compze: `Compze.Core.slnx` doesn't build

It still lists `src/SolutionStructure/Solution.dev_docs.csproj`, which commit 048eb6719 (2026-07-23) deleted and
replaced with `Solution.docs.csproj` in `Compze.AllProjects.slnx` only. `dotnet build Compze.Core.slnx` fails at once
with MSB3202, project file not found. No other solution lists it. The prototype was built with
`Compze.AllProjects.slnx` instead.
