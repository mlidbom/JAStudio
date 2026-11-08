using System.Collections.Generic;
using JAStudio.Core.Note;

namespace JAStudio.Core.Importing.RawNotes;

/// <summary>
/// Anki's side of importing: its note types, the <see cref="RawNote"/>s an <see cref="ImportProfile"/> selects, and tagging or deleting them once imported.<br/>
/// Anki knows nothing of particular imports: everything specific to one, Immersion Kit's included, is an <see cref="ImportProfile"/>.
/// </summary>
public interface IAnkiRawNotes
{
   /// <summary>The note types in Anki that raw notes can have: every one but JAStudio's own <see cref="NoteTypes"/>, by name.</summary>
   IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes();

   /// <summary>
   /// The notes of <paramref name="profile"/>'s <see cref="ImportProfile.AnkiNoteType"/> that its <see cref="ImportProfile.AnkiSearch"/> finds,<br/>
   /// those an earlier run imported and tagged <see cref="RawNote.ImportedTag"/> included.
   /// </summary>
   IReadOnlyList<RawNote> SelectedBy(ImportProfile profile);

   /// <summary>Tags <paramref name="rawNotes"/> <see cref="RawNote.ImportedTag"/> in Anki.</summary>
   void TagAsImported(IReadOnlyList<RawNote> rawNotes);

   /// <summary>Deletes <paramref name="rawNotes"/> from Anki.</summary>
   void Delete(IReadOnlyList<RawNote> rawNotes);
}
