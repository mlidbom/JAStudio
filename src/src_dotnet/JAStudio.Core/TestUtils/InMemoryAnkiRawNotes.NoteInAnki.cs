using System.Collections.Generic;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.TestUtils;

public partial class InMemoryAnkiRawNotes
{
   /// <summary>A note in the pretend Anki: its fields, and its tags as they are now.</summary>
   class NoteInAnki(string noteType, long ankiNoteId, IReadOnlyDictionary<string, string> fieldsByName)
   {
      public string NoteType { get; } = noteType;
      public long AnkiNoteId { get; } = ankiNoteId;
      public List<string> Tags { get; } = [];

      public RawNote AsItIsNow() => new(AnkiNoteId, fieldsByName, [.. Tags]);
   }
}
