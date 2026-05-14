using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using LinqToDB;

namespace JAStudio.Anki;

/// <summary>
/// Result from bulk loading AnkiNoteData from Anki's SQLite database.
/// </summary>
public class AnkiBulkLoadResult
{
   public List<AnkiNoteData> Notes { get; }
   public Dictionary<long, NoteId> AnkiIdMap { get; }

   public AnkiBulkLoadResult(List<AnkiNoteData> notes, Dictionary<long, NoteId> ankiIdMap)
   {
      Notes = notes;
      AnkiIdMap = ankiIdMap;
   }
}

/// <summary>
/// Loads all notes of a given note type directly from Anki's SQLite database.
/// Port of jastudio.anki_extentions.note_bulk_loader.NoteBulkLoader.
/// </summary>
public static class NoteBulkLoader
{
   const char FieldSeparator = '\x1f';

   /// <summary>
   /// Load all notes of the specified note type from the Anki database.
   /// Opens and closes its own connection so it's safe to call from any thread.
   /// </summary>
   public static AnkiBulkLoadResult LoadAllNotesOfType(string dbFilePath, string noteTypeName, Func<Guid, NoteId> idFactory)
   {
      using var db = AnkiDb.OpenReadOnly(dbFilePath);
      var (noteTypeId, fieldMap) = GetNoteTypeInfo(db, noteTypeName);
      return LoadNotes(db, noteTypeId, fieldMap, idFactory);
   }

   /// <summary>
   /// Build a lightweight AnkiNoteId → domain NoteId mapping for all known note types.
   /// Only reads the jas_note_id field — does not load full note data.
   /// </summary>
   public static Dictionary<long, NoteId> LoadAnkiIdMaps(string dbFilePath)
   {
      using var db = AnkiDb.OpenReadOnly(dbFilePath);
      var result = new Dictionary<long, NoteId>();

      foreach(var noteTypeName in NoteTypes.AllList)
      {
         var idFactory = NoteTypes.IdFactoryFromName(noteTypeName);
         var (noteTypeId, fieldMap) = GetNoteTypeInfo(db, noteTypeName, throwIfMissing: false);

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

   static (long? noteTypeId, Dictionary<string, int> fieldMap) GetNoteTypeInfo(AnkiDb db, string noteTypeName, bool throwIfMissing = true)
   {
      // Fetch all note types in C# to avoid depending on Anki's custom unicase collation
      var allNoteTypes = db.NoteTypes.ToList();
      var match = allNoteTypes.FirstOrDefault(nt => string.Equals(nt.Name, noteTypeName, StringComparison.Ordinal));

      if(match == null)
      {
         if(throwIfMissing)
            throw new KeyNotFoundException($"Note type '{noteTypeName}' not found in Anki database.");
         return (null, new Dictionary<string, int>());
      }

      var fieldMap = db.Fields
                       .Where(f => f.NoteTypeId == match.Id)
                       .OrderBy(f => f.Ordinal)
                       .ToList()
                       .ToDictionary(f => f.Name, f => f.Ordinal);

      return (match.Id, fieldMap);
   }

   static AnkiBulkLoadResult LoadNotes(AnkiDb db, long? noteTypeId, Dictionary<string, int> fieldMap, Func<Guid, NoteId> idFactory)
   {
      var notes = db.Notes
                    .Where(n => n.NoteTypeId == noteTypeId!.Value)
                    .Select(n => new { n.Id, n.Tags, n.Fields })
                    .ToList();

      var results = new List<AnkiNoteData>();
      var ankiIdMap = new Dictionary<long, NoteId>();

      foreach(var note in notes)
      {
         var tags = string.IsNullOrEmpty(note.Tags)
                       ? new List<string>()
                       : new List<string>(note.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries));

         var fieldValues = string.IsNullOrEmpty(note.Fields) ? [] : note.Fields.Split(FieldSeparator);

         var fields = new Dictionary<string, string>(fieldMap.Count);
         foreach(var (name, ordinal) in fieldMap)
         {
            fields[name] = ordinal < fieldValues.Length ? fieldValues[ordinal] : "";
         }

         // Read persisted jas_note_id from the note's fields, or generate a new one.
         // A new GUID is written back to the jas_note_id field on next flush.
         var jasNoteIdStr = fields.GetValueOrDefault(AnkiFieldNames.JasNoteId, "");
         var noteId = Guid.TryParse(jasNoteIdStr, out var guid)
                         ? idFactory(guid)
                         : idFactory(Guid.NewGuid());

         ankiIdMap[note.Id] = noteId;
         results.Add(new AnkiNoteData(noteId, fields, tags));
      }

      return new AnkiBulkLoadResult(results, ankiIdMap);
   }
}
