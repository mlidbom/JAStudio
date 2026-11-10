using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>
/// The raw notes in the open Anki profile, for importing: note types and notes are read from the database of the collection Anki has open<br/>
/// (<see cref="AnkiCollectionDatabase"/>), the notes an import profile selects are found with Anki's own search, and tagging and deleting go<br/>
/// through Anki.
/// </summary>
public partial class AnkiRawNotes : IAnkiRawNotes
{
   readonly AnkiCollectionDatabase _database;

   public AnkiRawNotes(AnkiCollectionDatabase database) => _database = database;

   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes() => NoteTypesInAnki.ReadFrom(_database).ForRawNotes();

   public IReadOnlyList<RawNote> SelectedBy(ImportProfile profile)
   {
      var ankiNoteIds = AnkiFacade.Notes.Find(SearchFor(profile));
      var noteType = NoteTypesInAnki.ReadFrom(_database).Named(profile.AnkiNoteType);
      return [.. _database.Read<RawNoteRow>(RawNoteRow.SelectThoseWith(ankiNoteIds)).OrderBy(it => it.AnkiNoteId).Select(it => it.AsARawNoteOf(noteType))];
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

   static List<long> AnkiNoteIdsOf(IReadOnlyList<RawNote> rawNotes) => [.. rawNotes.Select(it => it.AnkiNoteId)];
}
