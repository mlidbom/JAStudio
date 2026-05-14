using System.Collections.Generic;

namespace JAStudio.Core.Note;

public class AnkiNoteData
{
   public NoteId? Id { get; set; }
   public Dictionary<string, string> Fields { get; set; }
   public List<string> Tags { get; set; }

   public AnkiNoteData(NoteId? id, Dictionary<string, string> fields, List<string> tags)
   {
      Id = id;
      Fields = fields;
      Tags = tags;
   }
}
