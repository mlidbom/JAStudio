using System;
using System.Collections.Generic;
using System.IO;
using JAStudio.Anki;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The study-notes in the collection the headless JAStudio was started with, read straight from its database, as JAStudio inside Anki reads them.<br/>
/// While Anki runs, its latest changes may still be in the database's write-ahead log, which reading the file doesn't see. Writing it refuses,<br/>
/// since there is no Anki to write a study-note in.
/// </summary>
class HeadlessAnkiStudyNotes : IAnkiStudyNotes
{
   readonly FileInfo _ankiCollectionFile;

   public HeadlessAnkiStudyNotes(FileInfo ankiCollectionFile) => _ankiCollectionFile = ankiCollectionFile;

   public IReadOnlyList<StudyNoteInAnki> ReadAll() => StudyNotesInAnkisDatabase.ReadAll(_ankiCollectionFile.FullName);

   public void Write(JPNote note) => throw new NotSupportedException("The headless JAStudio has no Anki to write a study-note in.");
}
