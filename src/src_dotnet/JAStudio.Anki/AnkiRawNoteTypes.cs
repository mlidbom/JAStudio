using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>
/// The note types of an Anki collection whose notes are raw notes to import, read straight from the collection's database: every note type but<br/>
/// JAStudio's own. JAStudio inside Anki reads them from the collection Anki has open, the headless JAStudio from the one it was started with.
/// </summary>
public static class AnkiRawNoteTypes
{
   /// <summary>The note types for raw notes in the collection whose database is <paramref name="collectionFilePath"/>, <c>collection.anki2</c>, by name.</summary>
   public static IReadOnlyList<AnkiNoteType> In(string collectionFilePath)
   {
      using var db = AnkiDb.OpenReadOnly(collectionFilePath);
      var fields = db.Fields.ToList();
      return
      [
         .. db.NoteTypes.ToList()
              .Where(it => !NoteTypes.All.Contains(it.Name))
              .OrderBy(it => it.Name, StringComparer.CurrentCultureIgnoreCase)
              .Select(noteType => new AnkiNoteType(noteType.Name, FieldNamesOf(noteType.Id, fields)))
      ];
   }

   /// <summary>The names of the fields of the note type with the id <paramref name="noteTypeId"/>, in order, among <paramref name="fields"/>.</summary>
   internal static List<string> FieldNamesOf(long noteTypeId, IEnumerable<FieldRow> fields) =>
      [.. fields.Where(it => it.NoteTypeId == noteTypeId).OrderBy(it => it.Ordinal).Select(it => it.Name)];
}
