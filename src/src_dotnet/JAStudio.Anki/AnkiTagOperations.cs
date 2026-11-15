using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>The changes JAStudio makes to Anki's tags (<see cref="ISpacedRepetitionSystemTagOperations"/>), made in the collection Anki has open, each as an Anki operation.</summary>
public class AnkiTagOperations : ISpacedRepetitionSystemTagOperations
{
   readonly StudyNoteIds _ankiNoteIds;

   /// <param name="ankiNoteIds">Finds each note's study-note, its Anki note.</param>
   public AnkiTagOperations(StudyNoteIds ankiNoteIds) => _ankiNoteIds = ankiNoteIds;

   public void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => AnkiFacade.Tags.AddToNotes(StudyNotesOf(notes), tagInAnki);

   public void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => AnkiFacade.Tags.RemoveFromNotes(StudyNotesOf(notes), tagInAnki);

   public void RenameTheBranchOnEveryNoteInTheSpacedRepetitionSystem(string pathInAnki, string renamedPathInAnki) => AnkiFacade.Tags.Rename(pathInAnki, renamedPathInAnki);

   public void DeleteTheBranchFromEveryNoteInTheSpacedRepetitionSystem(string pathInAnki) => AnkiFacade.Tags.Delete(pathInAnki);

   public int NumberOfNotesInTheSpacedRepetitionSystemWithATagIn(string pathInAnki) => AnkiFacade.Tags.NumberOfNotesWithATagIn(pathInAnki);

   List<long> StudyNotesOf(IEnumerable<NoteId> notes) => [..notes.Select(_ankiNoteIds.RequiredStudyNoteIdOf)];
}
