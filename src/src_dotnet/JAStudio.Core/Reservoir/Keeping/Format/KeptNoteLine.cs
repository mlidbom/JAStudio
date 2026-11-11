using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Reservoir.Keeping.Media;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// A note of a kept deck, as a line of <c>notes.jsonl</c>: every field as the deck has it, its tags, and the sentence it gives, if any, shaped as<br/>
/// a sentence note's source data.
/// </summary>
/// <param name="Id">The id the sentence takes into the corpus.</param>
/// <param name="AnkiNoteId">The note's id in the deck's collection database.</param>
/// <param name="NoteType">The name of the note's note type (<see cref="KeptNoteTypeMapping.Name"/>).</param>
/// <param name="Tags">The note's tags, as the deck has them.</param>
/// <param name="Fields">Every field of the note, by name, in its note type's order, as the deck has it.</param>
/// <param name="SourceData">The sentence the note gives, by its note type's mapping, or null when it gives none.</param>
public sealed record KeptNoteLine(Guid Id, long AnkiNoteId, string NoteType, IReadOnlyList<string> Tags, IReadOnlyDictionary<string, string> Fields, KeptSentenceSourceData? SourceData)
{
   /// <summary>
   /// The line of <paramref name="note"/>, of the note type whose fields' roles are <paramref name="roles"/>, its sentence's media referred to by the<br/>
   /// hashes they're kept under in <paramref name="media"/>.
   /// </summary>
   public static KeptNoteLine Of(RawNote note, DeckFieldRoles roles, StoredDeckMedia media)
   {
      //review: each note's id is a new GUID each time its deck is kept, so keeping a deck twice gives different ids. An id made from the
      //   package's hash and the note's id, a name-based UUID, would stay the same (expanding-the-sentence-corpus.md, "Keeping the reservoir safe").
      return new KeptNoteLine(Guid.NewGuid(),
                              note.AnkiNoteId,
                              roles.NoteType.Name,
                              note.Tags,
                              roles.NoteType.FieldNames.ToDictionary(name => name, note.Field),
                              roles.SentenceOf(note) is {} sentence ? KeptSentenceSourceData.Of(sentence, media) : null);
   }
}
