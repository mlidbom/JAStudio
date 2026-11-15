# Studying — Design Notes

> **Status: started 2026-11-07 at Magnus's request, to be designed.** It records the principle, what exists
> today and the features wanted. The design comes when the work reaches it.
>
> **Since 2026-11-14:** JAStudio is to become a program of its own, which talks HTTP with Anki, and a Sync button in a
> Studying view is to replace Anki's notifications of what changed: see `moving-jastudio-out-of-anki.md`. Its first
> increment, built 2026-11-15, renamed Core's seam to the spaced repetition system in studying's words, and gave it the
> namespace `JAStudio.Core.Studying`: see "Increment 1 as built" there.

## The principle (Magnus, 2026-11-07)

- **Studying is not Anki-specific.** JAStudio's domain has its own concept of studying.
- **Anki is one spaced repetition system** that JAStudio integrates with. It's the only one for now, but it isn't the
  truth of studying. Others may come, for students to use instead of Anki.
- **So the domain speaks about studying.** Only the Anki integration speaks about Anki's notes, cards, card
  templates and suspension.

## Terms

- **spaced repetition system** — a system that schedules a student's reviews, which JAStudio integrates with. Anki is
  the only one today. Until 2026-11-15 this document said *studying-add-on*.
- **the Anki integration** — JAStudio's code that works with Anki: `JAStudio.Anki`.
- **the Anki add-on** — the Python code that Anki loads. Nothing in JAStudio is called an add-on (Magnus, 2026-11-15):
  with a studying-add-on in JAStudio, "the Anki add-on" read both as the add-on in JAStudio for Anki and as the add-on
  loaded in Anki. See `moving-jastudio-out-of-anki.md`, "Terms".
- **reading** and **listening** — the two ways a note is studied today. Reading shows the text and asks for its
  meaning; listening plays the audio. In Anki they are card types (`CardTypes.Reading`, `CardTypes.Listening`,
  the card templates' names). What the domain calls them is to be designed.

## Today

- **The studying status is student-data held in memory, not stored in the corpus.**
  - At load, and whenever JAStudio reads the study-notes from Anki again (`JPCollection.ReadTheStudyNotesFromAnkiAgain`),
    such as after a sync, `AnkiDataLoader` reads every card from Anki's database: its
    Anki note id, its card type, whether it's suspended, and its note type (`CardStudyingStatus`).
  - Each note keeps that in memory (`JPNote.ReadItsStudyNoteFrom`, and `JPNote.SetStudyingStatus` for a single card).
- **"Studying" means having a card that isn't suspended.** `JPNote.IsStudying(cardType)` answers it for one card
  type or for any card. `IsStudyingRead` and `IsStudyingListening` are shorthands.
- **`ISpacedRepetitionSystemCardOperations`** (`ICardOperations` until 2026-11-15) is Core's interface for changing a
  note's cards in the spaced repetition system, implemented by `AnkiCardOperations`. It can suspend all of a note's
  cards, unsuspend all of them, and delete the note's study-note. It still speaks of cards and suspending, which are
  Anki's words.
- **The note context menu** has "Unsuspend all cards", "Suspend all cards", and "Delete note". "Delete note"
  removes the note from the corpus and from Anki, review history included, and its media files that no other note
  uses (`corpus-data-model.md`).
- **Some study state is stored in the corpus anyway,** as generated kanji tags such as
  `-::kanji::studying-vocab`. They're student-data and are to move out (`corpus-data-model.md`).
- **Every corpus note has an Anki note,** apart from one leftover sentence. Whether a note is in the
  spaced repetition system and whether it's studied are separate questions, and "studied" means "unsuspended"
  (`corpus-data-model.md`, "Settled so far").

## Wanted (Magnus, 2026-11-07)

- **Suspending and unsuspending a note's reading and listening separately,** for one note or for a selection of
  notes.
- **Adding notes to the spaced repetition system, and removing them from it entirely.**
- **Search keys** for the studying state, such as `studying-reading:true` and `studying-listening:false`
  ([note-search.md](note-search.md)).

## To design

1. **The domain's words** for reading and listening (today Anki's "card types"), and for suspending (today
   Anki's word).
2. **The states,** per note and per reading or listening:
   - not in the spaced repetition system;
   - in it but suspended;
   - studied.

   Anki's other states (new, learning, due and so on) only if a workflow needs them.
3. **The boundary:** what the integration with a spaced repetition system implements.
   - Reading the states of all notes.
   - Adding a note to it, and removing a note from it.
   - Suspending and unsuspending reading or listening.

   `ISpacedRepetitionSystemCardOperations` is the seed. It needs the domain's words for cards and suspending, and
   widening.
4. **Removing a note from Anki deletes its review history.** How that is confirmed, and how it differs from
   suspending everything.
5. **Several students:** student-data is per student (`corpus-data-model.md`, target model principle 5). How a
   student relates to a spaced repetition system's collection or profile.
6. **Student-data for notes that aren't in any spaced repetition system,** such as priority. It overlaps the open points in
   `corpus-data-model.md`.
