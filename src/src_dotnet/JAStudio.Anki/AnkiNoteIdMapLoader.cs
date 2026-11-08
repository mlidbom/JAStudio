using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using LinqToDB;

namespace JAStudio.Anki;

/// <summary>
/// Reads, straight from Anki's database, which corpus note each Anki note of JAStudio's note types is: the <see cref="NoteId"/> in its<br/>
/// <c>jas_note_id</c> field. That's what the reload fills <see cref="ExternalNoteIdMap"/> with, so that what Anki reports about an Anki note,<br/>
/// such as its cards' studying status or its deletion, reaches the corpus note.
/// </summary>
public static class AnkiNoteIdMapLoader
{
   const char FieldSeparator = '\x1f';

   /// <summary>
   /// The corpus note id of every Anki note of JAStudio's note types that has one, by the Anki note's id. Only the <c>jas_note_id</c> field is read.<br/>
   /// Opens and closes its own connection, so it's safe to call from any thread.
   /// </summary>
   public static Dictionary<long, NoteId> Load(string dbFilePath)
   {
      using var db = AnkiDb.OpenReadOnly(dbFilePath);
      var result = new Dictionary<long, NoteId>();

      foreach(var noteTypeName in NoteTypes.AllList)
      {
         var idFactory = NoteTypes.IdFactoryFromName(noteTypeName);
         var (noteTypeId, fieldMap) = GetNoteTypeInfo(db, noteTypeName);

         if(noteTypeId == null || !fieldMap.TryGetValue(AnkiFieldNames.JasNoteId, out var jasNoteIdOrdinal))
            continue;

         var notes = db.Notes
                       .Where(n => n.NoteTypeId == noteTypeId.Value)
                       .Select(n => new { n.Id, n.Fields })
                       .ToList();

         foreach(var note in notes)
         {
            var fieldValues = string.IsNullOrEmpty(note.Fields) ? [] : note.Fields.Split(FieldSeparator);
            var jasNoteIdStr = jasNoteIdOrdinal < fieldValues.Length ? fieldValues[jasNoteIdOrdinal] : "";
            if(Guid.TryParse(jasNoteIdStr, out var guid))
            {
               result[note.Id] = idFactory(guid);
            }
         }
      }

      return result;
   }

   /// <summary>The id of the note type named <paramref name="noteTypeName"/> and the ordinals of its fields by name, or a null id when Anki hasn't the note type.</summary>
   static (long? noteTypeId, Dictionary<string, int> fieldMap) GetNoteTypeInfo(AnkiDb db, string noteTypeName)
   {
      // Fetch all note types in C# to avoid depending on Anki's custom unicase collation
      var allNoteTypes = db.NoteTypes.ToList();
      var match = allNoteTypes.FirstOrDefault(nt => string.Equals(nt.Name, noteTypeName, StringComparison.Ordinal));
      if(match == null)
         return (null, new Dictionary<string, int>());

      var fieldMap = db.Fields
                       .Where(f => f.NoteTypeId == match.Id)
                       .OrderBy(f => f.Ordinal)
                       .ToList()
                       .ToDictionary(f => f.Name, f => f.Ordinal);

      return (match.Id, fieldMap);
   }
}
