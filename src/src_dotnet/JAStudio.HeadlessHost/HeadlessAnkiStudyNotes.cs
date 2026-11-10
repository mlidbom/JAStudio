using System;
using System.Collections.Generic;
using JAStudio.Anki;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The study-notes in the collection the headless JAStudio was started with, read from its file (<see cref="AnkiCollectionFile"/>), with the same<br/>
/// SELECTs as JAStudio inside Anki reads them. Writing one is refused, since there is no Anki to write a study-note in.
/// </summary>
class HeadlessAnkiStudyNotes : IAnkiStudyNotes
{
   readonly AnkiCollectionDatabase _database;

   /// <param name="database">The database of the collection the headless JAStudio was started with, its file (<see cref="AnkiCollectionFile"/>).</param>
   public HeadlessAnkiStudyNotes(AnkiCollectionDatabase database) => _database = database;

   public IReadOnlyList<StudyNoteInAnki> ReadAll() => StudyNotesInAnkisDatabase.ReadAll(_database);

   public void Write(JPNote note) => throw new NotSupportedException("The headless JAStudio has no Anki to write a study-note in.");
}
