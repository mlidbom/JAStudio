# Studying — Design Notes

> **Status: started 2026-11-07 at Magnus's request, to be designed.** It records the principle, what exists
> today and the features wanted. The design comes when the work reaches it.

## The principle (Magnus, 2026-11-07)

- **Studying is not Anki-specific.** JAStudio's domain has its own concept of studying.
- **Anki is one studying-add-on:** a spaced repetition system JAStudio integrates with. It's the only one for now,
  but it isn't the truth of studying. Others may come, for students to use instead of Anki.
- **So the domain speaks about studying.** Only the Anki integration speaks about Anki's notes, cards, card
  templates and suspension.

## Terms

- **studying-add-on** — a spaced repetition system that JAStudio integrates with, which schedules a student's
  reviews. Anki is the only one today.
- **reading** and **listening** — the two ways a note is studied today. Reading shows the text and asks for its
  meaning; listening plays the audio. In Anki they are card types (`CardTypes.Reading`, `CardTypes.Listening`,
  the card templates' names). What the domain calls them is to be designed.

## Today

- **The studying status is student-data held in memory, not stored in the corpus.**
  - At load, and whenever JAStudio reads the study-notes from Anki again (`JPCollection.ReadTheStudyNotesFromAnkiAgain`),
    such as after a sync, `AnkiBackendDataLoader` reads every card from Anki's database: its
    Anki note id, its card type, whether it's suspended, and its note type (`CardStudyingStatus`).
  - Each note keeps that in memory (`JPNote.ReadItsStudyNoteFrom`, and `JPNote.SetStudyingStatus` for a single card).
- **"Studying" means having a card that isn't suspended.** `JPNote.IsStudying(cardType)` answers it for one card
  type or for any card. `IsStudyingRead` and `IsStudyingListening` are shorthands.
- **`ICardOperations`** is Core's interface to the studying-add-on, implemented by `AnkiCardOperationsImpl`. It
  can suspend all of a note's cards, unsuspend all of them, and delete the note's Anki note. Its names and docs
  speak of cards and Anki notes.
- **The note context menu** has "Unsuspend all cards", "Suspend all cards", and "Delete note". "Delete note"
  removes the note from the corpus and from Anki, review history included, and its media files that no other note
  uses (`corpus-data-model.md`).
- **Some study state is stored in the corpus anyway,** as generated kanji tags such as
  `-::kanji::studying-vocab`. They're student-data and are to move out (`corpus-data-model.md`).
- **Every corpus note has an Anki note,** apart from one leftover sentence. Whether a note is in the
  studying-add-on and whether it's studied are separate questions, and "studied" means "unsuspended"
  (`corpus-data-model.md`, "Settled so far").

## Wanted (Magnus, 2026-11-07)

- **Suspending and unsuspending a note's reading and listening separately,** for one note or for a selection of
  notes.
- **Adding notes to the studying-add-on, and removing them from it entirely.**
- **Search keys** for the studying state, such as `studying-reading:true` and `studying-listening:false`
  ([note-search.md](note-search.md)).

## To design

1. **The domain's words** for reading and listening (today Anki's "card types"), and for suspending (today
   Anki's word).
2. **The states,** per note and per reading or listening:
   - not in the studying-add-on;
   - in it but suspended;
   - studied.

   Anki's other states (new, learning, due and so on) only if a workflow needs them.
3. **The boundary:** what a studying-add-on implements.
   - Reading the states of all notes.
   - Adding a note to it, and removing a note from it.
   - Suspending and unsuspending reading or listening.

   `ICardOperations` is the seed. It needs renaming in the domain's words, and widening.
4. **Removing a note from Anki deletes its review history.** How that is confirmed, and how it differs from
   suspending everything.
5. **Several students:** student-data is per student (`corpus-data-model.md`, target model principle 5). How a
   student relates to a studying-add-on's collection or profile.
6. **Student-data for notes that aren't in any studying-add-on,** such as priority. It overlaps the open points in
   `corpus-data-model.md`.
