using System.Collections.Generic;
using JAStudio.PythonInterop;

namespace JAStudio.Anki.AnkisFunctions.ThroughPythonnet;

public sealed partial class AnkiThroughPythonnet
{
   sealed class ItsNotes(AnkiThroughPythonnet anki) : IAnkisNotes
   {
      readonly AnkiThroughPythonnet _anki = anki;

      public List<long> Find(string search) => _anki.Backend.Use(it => PythonDotNetShim.LongList.ToDotNet(it.notes_find(search)));

      public void AddTag(IReadOnlyList<long> noteIds, string tag) => _anki.Backend.Use(it => it.notes_add_tag(PythonDotNetShim.LongList.ToPython(noteIds), tag));

      public void Delete(IReadOnlyList<long> noteIds) => _anki.Backend.Use(it => it.notes_remove(PythonDotNetShim.LongList.ToPython(noteIds)));

      public void WriteTheFields(long noteId, IReadOnlyDictionary<string, string> fields) =>
         _anki.Backend.Use(it => it.study_note_write(noteId, PythonDotNetShim.StringStringDict.ToPython(fields)));

      public void SuspendAllCardsOf(long noteId) => _anki.Backend.Use(it => it.note_suspend_all_cards(noteId));

      public void UnsuspendAllCardsOf(long noteId) => _anki.Backend.Use(it => it.note_unsuspend_all_cards(noteId));
   }
}
