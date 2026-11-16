# The prototypes combined: what a taggregate note costs with changes 1 to 3

Measured 2026-11-16 by taggregates-opus. The two prototypes, each built on its own branch from Compze's `main`, merged
into a third:

- **Change 2, tevent routing decided once per class:** jastudio-rep-opus's branch `taggregate-experiments-opus`, at
  529a5170e. Written up in Compze's `dev_docs/taggregate-improvements/opus/5-prototype-of-per-class-routing.md`.
- **Changes 1 and 3, the snapshot and the per-cascade hook:** taggregates-opus's branch
  `taggregates-opus/snapshot-and-hook`, at a5defb7eb. Written up in
  [the prototype of the snapshot and the per-cascade hook](prototype-of-the-snapshot-and-the-per-cascade-hook.md).
- **Combined:** branch `taggregates-opus/combined`, at 7dbfa74c0, in the worktree
  `C:\Dev\Compze.worktrees\taggregates-opus-combined`.

## They combine

- **Two conflicts, both small:** the fields of `Taggregate`, where one prototype made the dispatchers lazy and the other
  the lists of tevents; and `Publish`'s dispatch to the tevent handlers, where one added the per-class handlers and the
  other renamed the cascade's list. Nothing in either prototype had to change for the other.
- **Compze's full suite passes on the combination:** all 33 test assemblies, with 474 unit tests, 436 of them
  jastudio-rep-opus's and 38 taggregates-opus's.

## What a note costs

Bytes per note, over 10,000 notes, on .NET 11 with non-concurrent workstation garbage collection. Each component has 3
string properties with one tevent applier each. "Registered" is today's per-instance registration, as built in the
combined branch, where its dispatchers are created when first used.

| Components | Style | Retained, loaded | Allocated by a load | Retained more after an edit of each component and a commit | Allocated by those edits |
|---:|---|---:|---:|---:|---:|
| 15 | plain objects | 733 | | | |
| 15 | registered per instance | 11,168 | 19,216 | +6,280 | 26,672 |
| 15 | declared per class, from history | 1,080 | 1,784 | +8 | 11,608 |
| 15 | **declared per class, from a snapshot** | **1,080** | **2,040** | **+8** | **11,608** |
| 6 | plain objects | 303 | | | |
| 6 | **declared per class, from a snapshot** | **504** | **1,464** | **+8** | **4,688** |
| 1 | plain objects | 47 | | | |
| 1 | **declared per class, from a snapshot** | **168** | **1,128** | **+8** | **840** |

- **A 15-component note keeps 1,080 bytes, and nothing more once edited:** against 11,457 bytes, and 18,551 once edited,
  with 0.6.0-alpha ([the measurements](measurements.md)). The 144 bytes of jastudio-rep-opus's fallback to per-instance
  dispatchers would go in a final design: about 936 bytes, some 200 bytes over plain objects. What's left is the
  taggregate's id, its fields, and each component's reference to its parent.
- **For the corpus,** 80,000 notes at 936 to 1,080 bytes is about 75 to 86 MB, against about 59 MB as plain objects and
  0.6 to 0.9 GB with 0.6.0-alpha. The benchmark's notes are minimal: real notes hold more state, the same in every
  style.
- **Each change's part:** per-class routing took a loaded note from 11,256 to 1,168 bytes (jastudio-rep-opus's
  measurement); the lazy `TeventStream` took 24 bytes more; creating the lists of tevents only when needed, and handing
  them on whole, took 64 bytes, and the 208 bytes their grown backing arrays kept after an edit.

## What the measurements also show

- **A load from a snapshot allocates 256 bytes more than one from history:** `LoadFromSnapshot` copies the tevents into
  a list and wraps them itself, where the caller of `LoadFromHistory` hands them in wrapped. Garbage, not retained:
  about 20 MB over a load of 80,000 notes. Iterating the snapshot once instead of copying it would take most of it.
- **An edit allocates about 770 bytes,** a publish and its share of a commit: the tevent with its `TessageId` and
  timestamp, its wrappers, and now a new list for each cascade, which the old code reused. Garbage, not retained. For
  JAStudio, where edits are rare and a full rebuild publishes once or twice per note, it's of no consequence.
