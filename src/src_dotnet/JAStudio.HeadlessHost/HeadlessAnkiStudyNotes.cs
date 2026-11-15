using System;
using System.Collections.Generic;
using JAStudio.Anki;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Note;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The study-notes in the collection the headless JAStudio was started with, read from its file (<see cref="AnkiCollectionFile"/>), with the same<br/>
/// SELECTs as JAStudio inside Anki reads them. Changing one is refused, since there is no Anki to change a study-note or its cards in.
/// </summary>
class HeadlessAnkiStudyNotes : ISRSStudyNotes
{
   readonly AnkiCollectionDatabase _database;

   /// <param name="database">The database of the collection the headless JAStudio was started with, its file (<see cref="AnkiCollectionFile"/>).</param>
   public HeadlessAnkiStudyNotes(AnkiCollectionDatabase database) => _database = database;

   public IReadOnlyList<StudyNoteInTheSRS> ReadAll() => StudyNotesInAnkisDatabase.ReadAll(_database);

   public void Write(JPNote note) => throw new NotSupportedException("The headless JAStudio has no Anki to write a study-note in.");
   public void SuspendAllCardsForNote(NoteId noteId) => throw NoAnkiToChangeCardsIn();
   public void UnsuspendAllCardsForNote(NoteId noteId) => throw NoAnkiToChangeCardsIn();
   public void DeleteTheStudyNoteOf(NoteId noteId) => throw new NotSupportedException("The headless JAStudio has no Anki to delete a study-note in.");

   static NotSupportedException NoAnkiToChangeCardsIn() => new("The headless JAStudio has no Anki to change a note's cards in.");
}
