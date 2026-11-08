using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>
/// The raw notes in the open Anki profile, for importing: note types and notes are read straight from Anki's database, the notes an import profile<br/>
/// selects are found with Anki's own search, and tagging and deleting go through Anki.
/// </summary>
public class AnkiRawNotes : IAnkiRawNotes
{
   const char FieldSeparator = '\x1f';

   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes()
   {
      using var db = AnkiDb.OpenReadOnly(DbFilePath());
      var fields = db.Fields.ToList();
      return
      [
         .. db.NoteTypes.ToList()
              .Where(it => !NoteTypes.All.Contains(it.Name))
              .OrderBy(it => it.Name, StringComparer.CurrentCultureIgnoreCase)
              .Select(noteType => new AnkiNoteType(noteType.Name, FieldNamesOf(noteType.Id, fields)))
      ];
   }

   public IReadOnlyList<RawNote> SelectedBy(ImportProfile profile)
   {
      var ankiNoteIds = AnkiFacade.Notes.Find(SearchFor(profile));

      using var db = AnkiDb.OpenReadOnly(DbFilePath());
      var noteType = db.NoteTypes.ToList().Single(it => it.Name == profile.AnkiNoteType);
      var fieldNames = FieldNamesOf(noteType.Id, [.. db.Fields]);

      return
      [
         .. db.Notes.Where(it => ankiNoteIds.Contains(it.Id))
              .ToList()
              .OrderBy(it => it.Id)
              .Select(it => RawNoteFrom(it, fieldNames))
      ];
   }

   public void TagAsImported(IReadOnlyList<RawNote> rawNotes) => AnkiFacade.Notes.AddTag(AnkiNoteIdsOf(rawNotes), RawNote.ImportedTag);

   public void Delete(IReadOnlyList<RawNote> rawNotes) => AnkiFacade.Notes.Delete(AnkiNoteIdsOf(rawNotes));

   /// <summary>
   /// The Anki search for <paramref name="profile"/>'s raw notes: the notes of its note type that its own search finds.<br/>
   /// In a quoted Anki search term, <c>*</c> and <c>_</c> are wildcards, so a note type's name has them escaped, with its backslashes and quotes.
   /// </summary>
   static string SearchFor(ImportProfile profile)
   {
      var notesOfTheNoteType = $"\"{Builtin.Note}:{EscapedForAQuotedSearchTerm(profile.AnkiNoteType)}\"";
      return profile.AnkiSearch == "" ? notesOfTheNoteType : $"{notesOfTheNoteType} ({profile.AnkiSearch})";

      static string EscapedForAQuotedSearchTerm(string text) => text.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("*", @"\*").Replace("_", @"\_");
   }

   static RawNote RawNoteFrom(NoteRow row, IReadOnlyList<string> fieldNames)
   {
      var fieldValues = row.Fields.Split(FieldSeparator);
      var fieldsByName = fieldNames.Select((name, ordinal) => (name, value: ordinal < fieldValues.Length ? fieldValues[ordinal] : ""))
                                   .ToDictionary(it => it.name, it => it.value);
      return new RawNote(row.Id, fieldsByName, row.Tags.Split(' ', StringSplitOptions.RemoveEmptyEntries));
   }

   static List<string> FieldNamesOf(long noteTypeId, IEnumerable<FieldRow> fields) =>
      [.. fields.Where(it => it.NoteTypeId == noteTypeId).OrderBy(it => it.Ordinal).Select(it => it.Name)];

   static List<long> AnkiNoteIdsOf(IReadOnlyList<RawNote> rawNotes) => [.. rawNotes.Select(it => it.AnkiNoteId)];

   static string DbFilePath() => AnkiFacade.Col.DbFilePath() ?? throw new InvalidOperationException("Anki has no collection open to import from.");
}
