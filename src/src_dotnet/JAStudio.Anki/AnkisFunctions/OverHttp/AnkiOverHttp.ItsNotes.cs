using System.Collections.Generic;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsNotes(AnkiOverHttp anki) : IAnkisNotes
   {
      readonly AnkiOverHttp _anki = anki;

      public List<long> Find(string search) => _anki.Call<List<long>>("notes_find", ("search", search));

      public void AddTag(IReadOnlyList<long> noteIds, string tag) => _anki.Call("notes_add_tag", ("note_ids", noteIds), ("tag", tag));

      public void Delete(IReadOnlyList<long> noteIds) => _anki.Call("notes_remove", ("note_ids", noteIds));

      public void WriteTheFields(long noteId, IReadOnlyDictionary<string, string> fields) => _anki.Call("study_note_write", ("anki_note_id", noteId), ("fields", fields));

      public void SuspendAllCardsOf(long noteId) => _anki.Call("note_suspend_all_cards", ("note_id", noteId));

      public void UnsuspendAllCardsOf(long noteId) => _anki.Call("note_unsuspend_all_cards", ("note_id", noteId));
   }
}
