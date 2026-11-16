# Migrating the notes to Compze's state store

> **Status: started 2026-11-16.** Kept by `taggregates-opus`, which does the migration in JAStudio, with
> `jastudio-rep-opus`, which builds the state store as a spike in Compze. The plan as proposed to `jastudio-rep-opus`,
> and where it stands. It changes as the work does.

**The goal (Magnus, 2026-11-16):** all of JAStudio's notes kept in Compze's state store, every change to a note a
tevent, to the point where everything the sessions can test themselves works. Magnus looks once the basic structure is
there, such as how a note's tevent interface hierarchy is built, before the rest is built on it. Done when Magnus,
`taggregates-opus` and `jastudio-rep-opus` all sign off.

**Where:** JAStudio's worktree `C:\Dev\JAStudio.wt2`, branch `migrating-the-notes-to-the-state-store`, from `main`.
Ideas tried more than one way get branches of their own, kept as a record. Compze's side: branch
`taggregate-store-spike`, in Compze's main checkout. What Magnus decided, which the migration codes against, is in
Compze's `dev_docs/taggregate-improvements/shared/README.md`, "What Magnus decides", and `the-taggregate-store.md`, "The
programming model".

## How JAStudio builds against the spike

**Project references to the spike's projects, instead of Compze's packages, on the migration branch only.** A change
in Compze is then in JAStudio's next build, with no packing, and no locally built package reaches the global NuGet
cache.

- **Today:** central package management with transitive pinning, in `src/src_dotnet/Directory.Packages.props`.
  Sixteen projects reference fourteen Compze packages, and three more are pinned because they come through the others.
  Each pinned version is the version in Compze's source, so the switch brings no unreleased changes besides the spike's.
- **The switch, in one file:** `src/src_dotnet/Directory.Build.targets` turns each Compze package reference into a
  reference to the project of the same name, under a folder given by a property. None of the sixteen project files
  changes, and switching back is deleting that block. Every Compze package must switch at once: a project built from
  source beside the same library from a package gives two copies of one assembly.
- **The folder:** a Compze worktree of JAStudio's own, `C:\Dev\Compze.worktrees\taggregate-store-spike-for-jastudio`,
  created by `jastudio-rep-opus` and moved only to commits it names, never committed in. Building straight from
  Compze's main checkout would build in the folders `jastudio-rep-opus` builds in, at the same time, and would build its
  unfinished edits.
- **`JAStudio.slnx` lists the Compze projects too,** 21 of them, Teventive and Tessaging among them. Compze's
  FlexRef (`build/FlexRef.props` there) turns a Compze project's reference to another into a package reference when the
  solution being built doesn't list that other project, and the build then fails looking for the package. Listed, they
  are built from source, and Rider navigates into their source. A new Compze project the migration needs is added to the
  list too: a missing one fails the restore with NU1101, never silently.
- **The central pins of Compze's packages are removed** on this branch, in the same block: a pin collides with the
  project of the same name (NU1109).
- **Checked, 2026-11-16:** at Compze's `e29fd6673`, which is `main`, JAStudio builds clean and all 2,823 of its .NET
  specifications pass.
- **The same SDK** in both repositories' `global.json`, 11.0.100, and both are `net11.0`.
- **At the end:** Compze publishes the store's packages, on Magnus's approval, and the branch goes back to package
  references before it merges into `main`.

## The order

Each step is an increment that builds and passes JAStudio's whole suite, and is committed.

**Before Compze's step 2, needing nothing new from Compze:**

1. **A baseline:** a full rebuild of this worktree's corpus in the headless JAStudio, to compare every later rebuild
   with.
2. **The snapshot types round-trip:** specifications that a vocab, a kanji and a sentence come back unchanged through
   their DTOs, and a check of every note in the real corpus. Under the private copy every edit builds its copy through
   the DTO, so a property it misses would be lost at every edit.
3. **`NoteId` derives from `TaggregateId`,** which is in Compze.Abstractions, already referenced. `VocabId`, `KanjiId`
   and `SentenceId` then are taggregate ids.
4. **What Anki says of a note's cards leaves the note:** `_cardStatus` and `AnkiTags`. It isn't stored, so a copy
   through the DTO drops it.
5. **What a note computes from other notes and keeps leaves it:** the set of forms that other vocab don't own, in
   `VocabNoteForms`, which goes stale.

**After Compze's step 1, `ITaggregateTevent` as a plain `ITevent`:**

6. **The vocab's tevent interface hierarchy,** `VocabNoteForms` first, written to be read. **Magnus looks at it here.**

**After Compze's step 2, the in-memory state store:**

7. **The vocab moves into the store,** with `VocabNoteForms` the first part that publishes real tevents. The parts not
   converted yet go on changing state as today, and `NoteGuard.Update` publishes an interim tevent for them, so that the
   store counts the copy as changed. It publishes only when the note's DTO changed, as the hash decides today. Each later part converted removes one use of it, and the last removes it.
8. **The edit sites get units of work,** the holders of a note get taggregate-references, and the vocab caches' nine
   indexes become `ITaggregateIndex`es.
9. **The kanji, then the sentences,** the same way.
10. **The remaining parts converted,** one at a time, until the interim tevent is gone.
11. **What reaches other notes becomes participation handlers:** the perfect synonyms' answer, renaming
    word-references, and the other side of the mutual relationships and of `VocabNoteForms.Add` and `Remove`. What
    can't be undone, such as telling Anki to delete a study-note, moves to after the commit.

**After Compze's step 3, routing per class:**

12. **Memory and load time measured** with the whole corpus loaded, against `main`.

**After Compze's step 4, snapshots and the JSON store:**

13. **The notes stored through the store,** each file written whole, and several as all or nothing.

**Last, what the store's design doesn't have yet:** loading the whole corpus again while running, as around a git
fast-forward; a wait until observers have caught up, which JAStudio's specifications need; and a short way for a
specification to run an edit.

## What proves each step

- **JAStudio's whole suite,** .NET and Python, and `full-build.ps1`, since the API that Python sees changes.
- **The round-trip check over the real corpus,** from step 2 on.
- **A full rebuild compared with the baseline:** the migration should change nothing the corpus stores, so no file
  should differ. If the store needs the files changed, that is a change to the corpus, which the main checkout's
  session makes.
- **The MCP server's tools against the headless JAStudio,** which edit notes as the UI does.
- **What only Magnus can test:** JAStudio inside Anki, live.

## Put to `jastudio-rep-opus`, 2026-11-16, and agreed by it

**Its answers:**

- **Typed ids:** yes to all three, as requirements of steps 2 and 4. `Taggregate.Id` isn't sealed, and stays so.
- **Storage:** the seam, `ITaggregateStorage` (`LoadAll`, and `Prepare` returning a write to commit or discard), comes
  in step 2. JAStudio keeps its own storage behind it, its DTOs, settings and file layout byte for byte. Compze's JSON
  store is the default implementation for others, and JAStudio may borrow its atomic write. Step 4 brings JAStudio the
  snapshots and the round-trip helper, not a new file format.
- **Loading:** the store fills itself from `ITaggregateStorage.LoadAll` when it starts, outside any unit of work, in
  step 2. Loading everything again while running stays last.
- **The interim tevent:** works. A copy counts as changed when it has published, and that is what is written.

**The points as put:**

1. **Typed ids.** A note's id is a `VocabId`, `KanjiId` or `SentenceId`, each a `NoteId`. With `NoteId` derived from
   `TaggregateId`, JAStudio needs the store, the tevents and `RestoreIdentity` to keep the id instance they're given,
   never to make a new `TaggregateId` from its `Guid`, and `Taggregate.Id` to stay overridable, so that `VocabNote.Id`
   can be a `VocabId` by a covariant override.
2. **Storage before step 4.** The real JAStudio can't run on the store until its notes are written. A storage seam in
   step 2 would let JAStudio's repository write them until step 4. And at step 4: the corpus's files must stay exactly
   as they are, so either the JSON store lets its consumer decide the layout and the serialization, or JAStudio keeps
   its own storage behind the seam.
3. **Loading.** JAStudio loads all 80,000 notes at startup, from their DTOs, and again around a git fast-forward. The
   store needs a way to be filled with committed taggregates from their snapshots, outside any unit of work.
4. **The interim tevent of step 7** needs nothing from Compze, as far as I can see: a tevent whose applier changes
   nothing. Its only purpose is that the store counts the copy as changed.
