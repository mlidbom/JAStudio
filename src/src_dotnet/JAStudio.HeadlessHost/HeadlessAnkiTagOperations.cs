using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.HeadlessHost;

/// <summary>Refuses to change Anki's tags: the headless JAStudio has no Anki to change them in, only a copy of a collection it reads.</summary>
class HeadlessAnkiTagOperations : IAnkiTagOperations
{
   public void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => throw NoAnki();
   public void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) => throw NoAnki();
   public void RenameTheBranchOnEveryAnkiNote(string pathInAnki, string renamedPathInAnki) => throw NoAnki();
   public void DeleteTheBranchFromEveryAnkiNote(string pathInAnki) => throw NoAnki();
   public int NumberOfAnkiNotesWithATagIn(string pathInAnki) => throw NoAnki();

   static NotSupportedException NoAnki() => new("The headless JAStudio has no Anki to change Anki's tags in.");
}
