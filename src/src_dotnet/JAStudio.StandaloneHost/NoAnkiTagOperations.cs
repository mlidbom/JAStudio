using System.Collections.Generic;
using Compze.Contracts;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.StandaloneHost;

/// <summary>
/// The standalone JAStudio's changes to Anki's tags: none, since it has no Anki, and so no Anki notes. A change to the Anki tags of no notes, or of<br/>
/// every Anki note, changes nothing.
/// </summary>
sealed class NoAnkiTagOperations : IAnkiTagOperations
{
   public void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => AssertNoneOf(notes);
   public void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => AssertNoneOf(notes);
   public void RenameTheBranchOnEveryAnkiNote(string pathInAnki, string renamedPathInAnki) {}
   public void DeleteTheBranchFromEveryAnkiNote(string pathInAnki) {}
   public int NumberOfAnkiNotesWithATagIn(string pathInAnki) => 0;

   /// <summary>Asserts that <paramref name="notes"/>, notes each with a study-note, are none: without Anki, no note has one.</summary>
   static void AssertNoneOf(IReadOnlyCollection<NoteId> notes) =>
      Contract.Argument.Assert(notes.Count == 0, () => $"The standalone JAStudio has no Anki, so no note has a study-note, but {notes.Count} were given.");
}
