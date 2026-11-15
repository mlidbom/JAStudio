using System.Collections.Generic;
using JAStudio.PythonInterop;

namespace JAStudio.Anki.AnkisFunctions.ThroughPythonnet;

public sealed partial class AnkiThroughPythonnet
{
   sealed class ItsTags(AnkiThroughPythonnet anki) : IAnkisTags
   {
      readonly AnkiThroughPythonnet _anki = anki;

      public void AddToNotes(IReadOnlyList<long> noteIds, string tag) => _anki.Backend.Use(it => it.tags_add_to_notes(PythonDotNetShim.LongList.ToPython(noteIds), tag));

      public void RemoveFromNotes(IReadOnlyList<long> noteIds, string tag) => _anki.Backend.Use(it => it.tags_remove_from_notes(PythonDotNetShim.LongList.ToPython(noteIds), tag));

      public void Rename(string path, string renamedPath) => _anki.Backend.Use(it => it.tags_rename(path, renamedPath));

      public void Delete(string path) => _anki.Backend.Use(it => it.tags_delete(path));

      public int NumberOfNotesWithATagIn(string path) => _anki.Backend.Use(it => (int)it.tags_number_of_notes_with_a_tag_in(path));
   }
}
