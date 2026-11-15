using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki.AnkisFunctions;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>The changes JAStudio makes to Anki's tags (<see cref="ISRSTagOperations"/>), made in the collection Anki has open, each as an Anki operation.</summary>
public class AnkiTagOperations : ISRSTagOperations
{
   readonly StudyNoteIds _ankiNoteIds;
   readonly IAnkisTags _ankisTags;

   /// <param name="ankiNoteIds">Finds each note's study-note, its Anki note.</param>
   /// <param name="ankisTags">Anki's tags, which the changes are made in.</param>
   public AnkiTagOperations(StudyNoteIds ankiNoteIds, IAnkisTags ankisTags)
   {
      _ankiNoteIds = ankiNoteIds;
      _ankisTags = ankisTags;
   }

   public void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => _ankisTags.AddToNotes(StudyNotesOf(notes), tagInAnki);

   public void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => _ankisTags.RemoveFromNotes(StudyNotesOf(notes), tagInAnki);

   public void RenameTheBranchOnEveryNoteInTheSRS(string pathInAnki, string renamedPathInAnki) => _ankisTags.Rename(pathInAnki, renamedPathInAnki);

   public void DeleteTheBranchFromEveryNoteInTheSRS(string pathInAnki) => _ankisTags.Delete(pathInAnki);

   public int NumberOfNotesInTheSRSWithATagIn(string pathInAnki) => _ankisTags.NumberOfNotesWithATagIn(pathInAnki);

   List<long> StudyNotesOf(IEnumerable<NoteId> notes) => [..notes.Select(_ankiNoteIds.RequiredStudyNoteIdOf)];
}
