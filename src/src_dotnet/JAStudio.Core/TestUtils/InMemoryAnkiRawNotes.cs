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
public class InMemoryAnkiRawNotes : IAnkiRawNotes
{
   readonly List<AnkiNoteType> _noteTypes = [];
   readonly List<(string noteType, RawNote rawNote)> _rawNotes = [];
   readonly HashSet<RawNote> _taggedAsImported = [];

   public void AddNoteType(AnkiNoteType noteType) => _noteTypes.Add(noteType);

   public void Add(string noteType, RawNote rawNote) => _rawNotes.Add((noteType, rawNote));

   public bool IsTaggedAsImported(RawNote rawNote) => _taggedAsImported.Contains(rawNote);
   public bool Contains(RawNote rawNote) => _rawNotes.Any(it => it.rawNote == rawNote);

   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes() => _noteTypes;

   /// <summary>Anki's search can't run here, so a profile with an <see cref="ImportProfile.AnkiSearch"/> is refused rather than silently given every note of its type.</summary>
   public IReadOnlyList<RawNote> NotYetImported(ImportProfile profile)
   {
      Contract.Argument.Assert(profile.AnkiSearch == "", () => "Anki's search can't run in specifications, so they can't use an import profile with a search.");
      return [.. _rawNotes.Where(it => it.noteType == profile.AnkiNoteType && !_taggedAsImported.Contains(it.rawNote)).Select(it => it.rawNote)];
   }

   public void TagAsImported(IReadOnlyList<RawNote> rawNotes) => _taggedAsImported.UnionWith(rawNotes);

   public void Delete(IReadOnlyList<RawNote> rawNotes) => _rawNotes.RemoveAll(it => rawNotes.Contains(it.rawNote));
}
