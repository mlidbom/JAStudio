using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Reservoir.Keeping.Media;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// A note of a kept deck, as a line of <c>notes.jsonl</c>: every field as the deck has it, its tags, and the sentence it gives, if any, shaped as<br/>
/// a sentence note's source data.
/// </summary>
/// <param name="Id">The id the sentence takes into the corpus: made from the package's hash and the note's id in it, so the same every time it's kept.</param>
/// <param name="AnkiNoteId">The note's id in the deck's collection database.</param>
/// <param name="NoteType">The name of the note's note type (<see cref="KeptNoteTypeMapping.Name"/>).</param>
/// <param name="Tags">The note's tags, as the deck has them.</param>
/// <param name="Fields">Every field of the note, by name, in its note type's order, as the deck has it.</param>
/// <param name="SourceData">The sentence the note gives, by its note type's mapping, or null when it gives none.</param>
public sealed record KeptNoteLine(Guid Id, long AnkiNoteId, string NoteType, IReadOnlyList<string> Tags, IReadOnlyDictionary<string, string> Fields, KeptSentenceSourceData? SourceData)
{
   /// <summary>The namespace of the notes' ids (<see cref="NameBasedGuid"/>), JAStudio's own: a random GUID, made once.</summary>
   static readonly Guid NoteIdNamespace = new("ceb6a19e-c9a4-4b30-83a6-2f1795b719cc");

   /// <summary>
   /// The line of <paramref name="note"/>, of the note type whose fields' roles are <paramref name="roles"/>, its sentence's media referred to by the<br/>
   /// hashes they're kept under in <paramref name="media"/>, its id made from <paramref name="package"/>'s hash and the note's id in it.
   /// </summary>
   public static KeptNoteLine Of(RawNote note, DeckFieldRoles roles, StoredDeckMedia media, KeptPackage package)
   {
      return new KeptNoteLine(NameBasedGuid.Of(NoteIdNamespace, $"{package.Sha256}:{note.AnkiNoteId}"),
                              note.AnkiNoteId,
                              roles.NoteType.Name,
                              note.Tags,
                              roles.NoteType.FieldNames.ToDictionary(name => name, note.Field),
                              roles.SentenceOf(note) is {} sentence ? KeptSentenceSourceData.Of(sentence, media) : null);
   }
}
