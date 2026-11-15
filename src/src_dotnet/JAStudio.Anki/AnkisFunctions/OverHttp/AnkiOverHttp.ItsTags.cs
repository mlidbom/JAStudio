using System.Collections.Generic;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsTags(AnkiOverHttp anki) : IAnkisTags
   {
      readonly AnkiOverHttp _anki = anki;

      public void AddToNotes(IReadOnlyList<long> noteIds, string tag) => _anki.Call("tags_add_to_notes", ("note_ids", noteIds), ("tag", tag));

      public void RemoveFromNotes(IReadOnlyList<long> noteIds, string tag) => _anki.Call("tags_remove_from_notes", ("note_ids", noteIds), ("tag", tag));

      public void Rename(string path, string renamedPath) => _anki.Call("tags_rename", ("path", path), ("renamed_path", renamedPath));

      public void Delete(string path) => _anki.Call("tags_delete", ("path", path));

      public int NumberOfNotesWithATagIn(string path) => _anki.Call<int>("tags_number_of_notes_with_a_tag_in", ("path", path));
   }
}
