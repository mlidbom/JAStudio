using System.Linq;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Studying;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Anki;

/// <summary>
/// Loads what JAStudio needs from Anki of the study-notes, the Anki notes of its own note types (<see cref="SRSData"/>): which<br/>
/// corpus note each is, the studying statuses of its cards, and its tags. They're read from the collection's database (<see cref="AnkiCollectionDatabase"/>): inside<br/>
/// Anki, that of the collection Anki has open, through Anki's own connection; in the headless JAStudio, the collection file of an Anki profile.
/// </summary>
public sealed partial class AnkiDataLoader : ISRSDataLoader
{
   readonly AnkiCollectionDatabase _database;

   public AnkiDataLoader(AnkiCollectionDatabase database) => _database = database;

   public SRSData Load(TaskRunner taskRunner)
   {
      using var runner = taskRunner.Current("Loading user data from Anki");
      return runner.RunIndeterminate("Reading the study-notes from Anki", Read);
   }

   /// <summary>
   /// Reads <see cref="SRSData"/> from the database, with a SELECT each of the note types, the study-notes and the cards, one<br/>
   /// after another: inside Anki, Anki's backend runs one at a time anyway.
   /// </summary>
   SRSData Read()
   {
      var studyNoteTypes = NoteTypesInAnki.ReadFrom(_database).OfStudyNotesById();
      var studyNotes = _database.Read<StudyNoteRow>(StudyNoteRow.SelectEveryOf(studyNoteTypes.Keys));
      var noteTypeOfEachStudyNote = studyNotes.ToDictionary(it => it.AnkiNoteId, it => studyNoteTypes[it.NoteTypeId]);
      var cardsOfTheStudyNotes = _database.Read<CardRow>(CardRow.SelectEvery).Where(it => noteTypeOfEachStudyNote.ContainsKey(it.AnkiNoteId));

      return new SRSData(
         noteIdOfEachStudyNote: studyNotes.Where(it => it.HoldsACorpusNoteId).ToDictionary(it => it.AnkiNoteId, it => it.CorpusNoteIdAsANoteOf(noteTypeOfEachStudyNote[it.AnkiNoteId])),
         studyingStatuses: [.. cardsOfTheStudyNotes.Select(it => it.StudyingStatusAsACardOf(noteTypeOfEachStudyNote[it.AnkiNoteId]))],
         studyNoteTags: studyNotes.ToDictionary(it => it.AnkiNoteId, it => new NoteTagsAsAnkiStoresThem(it.Tags).Names));
   }
}
