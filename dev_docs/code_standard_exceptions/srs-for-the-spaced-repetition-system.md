# SRS for the spaced repetition system, in names

**The rule it breaks:** the code standard on naming says no abbreviations, unless it's certain both that the reader
knows the abbreviation and that it's obvious in context.

**The exception:** in the names of types, members, parameters and files, the spaced repetition system is `SRS`:
`ISRSWindows`, `SRSAvailability`, `StudyNoteInTheSRS`, `RenameTheBranchOnEveryNoteInTheSRS`. `SRS` is among the
solution's abbreviations in Rider (`JAStudio.slnx.DotSettings`), so that its naming inspection accepts the capitals.

**Why:** Magnus approved it on 2026-11-15, when the names of Core's seam to the spaced repetition system first spelled
the term out: "SpacedRepetitionSystem looks like a prime candidate for allowing an exception, an acronym. SRS Is a well
known acronym, and anyone dealing with this domain should know it already."

- Spelled out, the term is 22 characters in every name of the seam. The longest member was
  `RenameTheBranchOnEveryNoteInTheSpacedRepetitionSystem`.
- The acronym is the domain's own: those who use Anki write SRS, and the deck maker subs2srs has it in its name.

**What stays the same** (Claude's call, 2026-11-15, for Magnus to change if he doesn't share it): in prose, meaning the
documents and the comments, it's the spaced repetition system, the term decided on 2026-11-15
(`moving-jastudio-out-of-anki.md`, "Terms"). The doc comment of a type named with `SRS` says what it stands for where
it first mentions it.
