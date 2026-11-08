using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// Raw notes in a pretend Anki, for specifications: there's no Anki to import from in them. Specifications add the note types and the raw notes,<br/>
/// and their media files go in the test environment's Anki media folder, <see cref="IEnvironmentPaths.AnkiMediaDir"/>.
/// </summary>
/// <remarks>
/// Like Anki, it hands out a fresh <see cref="RawNote"/> each time notes are selected, with the tags they have then: compare notes by <see cref="RawNote.AnkiNoteId"/>.
/// </remarks>
public partial class InMemoryAnkiRawNotes : IAnkiRawNotes
{
   readonly List<AnkiNoteType> _noteTypes = [];
   readonly List<NoteInAnki> _notes = [];
   long _nextAnkiNoteId = 1;

   public void AddNoteType(AnkiNoteType noteType) => _noteTypes.Add(noteType);

   /// <summary>Adds a note of <paramref name="noteType"/> with the fields <paramref name="fieldsByName"/> and no tags, and returns it as a raw note.</summary>
   public RawNote Add(string noteType, IReadOnlyDictionary<string, string> fieldsByName)
   {
      var note = new NoteInAnki(noteType, _nextAnkiNoteId++, fieldsByName);
      _notes.Add(note);
      return note.AsItIsNow();
   }

   public bool IsTaggedAsImported(RawNote rawNote) => NoteWithTheIdOf(rawNote).Tags.Contains(RawNote.ImportedTag);
   public bool Contains(RawNote rawNote) => _notes.Any(it => it.AnkiNoteId == rawNote.AnkiNoteId);

   /// <summary>Removes the tag <see cref="RawNote.ImportedTag"/> from <paramref name="rawNote"/>, as one does in Anki to import a note again.</summary>
   public void RemoveTheImportedTag(RawNote rawNote) => NoteWithTheIdOf(rawNote).Tags.Remove(RawNote.ImportedTag);

   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes() => _noteTypes;

   /// <summary>Anki's search can't run here, so a profile with an <see cref="ImportProfile.AnkiSearch"/> is refused rather than silently given every note of its type.</summary>
   public IReadOnlyList<RawNote> SelectedBy(ImportProfile profile)
   {
      Contract.Argument.Assert(profile.AnkiSearch == "", () => "Anki's search can't run in specifications, so they can't use an import profile with a search.");
      return [.. _notes.Where(it => it.NoteType == profile.AnkiNoteType).Select(it => it.AsItIsNow())];
   }

   public void TagAsImported(IReadOnlyList<RawNote> rawNotes) => rawNotes.Select(NoteWithTheIdOf).ToList().ForEach(it => it.Tags.Add(RawNote.ImportedTag));

   public void Delete(IReadOnlyList<RawNote> rawNotes) => _notes.RemoveAll(note => rawNotes.Any(it => it.AnkiNoteId == note.AnkiNoteId));

   NoteInAnki NoteWithTheIdOf(RawNote rawNote) => _notes.Single(it => it.AnkiNoteId == rawNote.AnkiNoteId);
}
