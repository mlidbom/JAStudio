using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;

namespace JAStudio.HeadlessHost;

/// <summary>Refuses to change Anki's tags: the headless JAStudio has no Anki to change them in, only a copy of a collection it reads.</summary>
class HeadlessAnkiTagOperations : ISRSTagOperations
{
   public void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => throw NoAnki();
   public void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => throw NoAnki();
   public void RenameTheBranchOnEveryNoteInTheSRS(string pathInAnki, string renamedPathInAnki) => throw NoAnki();
   public void DeleteTheBranchFromEveryNoteInTheSRS(string pathInAnki) => throw NoAnki();
   public int NumberOfNotesInTheSRSWithATagIn(string pathInAnki) => throw NoAnki();

   static NotSupportedException NoAnki() => new("The headless JAStudio has no Anki to change Anki's tags in.");
}
