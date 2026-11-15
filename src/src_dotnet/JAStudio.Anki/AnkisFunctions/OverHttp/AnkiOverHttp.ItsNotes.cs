using System.Collections.Generic;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsNotes(AnkisHttpServer server) : IAnkisNotes
   {
      readonly AnkisHttpServer _server = server;

      public List<long> Find(string search) => _server.Call<List<long>>("notes_find", ("search", search));

      public void AddTag(IReadOnlyList<long> noteIds, string tag) => _server.Call("notes_add_tag", ("note_ids", noteIds), ("tag", tag));

      public void Delete(IReadOnlyList<long> noteIds) => _server.Call("notes_remove", ("note_ids", noteIds));

      public void Delete(long noteId) => _server.Call("note_remove", ("note_id", noteId));

      public void WriteTheFields(long noteId, IReadOnlyDictionary<string, string> fields) => _server.Call("study_note_write", ("anki_note_id", noteId), ("fields", fields));

      public void SuspendAllCardsOf(long noteId) => _server.Call("note_suspend_all_cards", ("note_id", noteId));

      public void UnsuspendAllCardsOf(long noteId) => _server.Call("note_unsuspend_all_cards", ("note_id", noteId));
   }
}
