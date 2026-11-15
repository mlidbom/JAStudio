using System.Collections.Generic;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsTags(AnkisHttpServer server) : IAnkisTags
   {
      readonly AnkisHttpServer _server = server;

      public void AddToNotes(IReadOnlyList<long> noteIds, string tag) => _server.Call("tags_add_to_notes", ("note_ids", noteIds), ("tag", tag));

      public void RemoveFromNotes(IReadOnlyList<long> noteIds, string tag) => _server.Call("tags_remove_from_notes", ("note_ids", noteIds), ("tag", tag));

      public void Rename(string path, string renamedPath) => _server.Call("tags_rename", ("path", path), ("renamed_path", renamedPath));

      public void Delete(string path) => _server.Call("tags_delete", ("path", path));

      public int NumberOfNotesWithATagIn(string path) => _server.Call<int>("tags_number_of_notes_with_a_tag_in", ("path", path));
   }
}
