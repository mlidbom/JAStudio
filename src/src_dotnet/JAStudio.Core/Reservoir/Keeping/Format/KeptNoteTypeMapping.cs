using System.Collections.Generic;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// How a kept deck's note type maps to a sentence: which of its fields holds the Japanese, the translation, the audio and the image, as<br/>
/// recognized when it was kept (<see cref="DeckFieldRoles"/>). The note lines hold every field as it is, so the mapping can be made anew from them.
/// </summary>
/// <param name="Name">The note type's name.</param>
/// <param name="Fields">The names of the note type's fields, in their order.</param>
public sealed record KeptNoteTypeMapping(
   string Name,
   IReadOnlyList<string> Fields,
   string? JapaneseField,
   bool JapaneseFieldHoldsFurigana,
   bool JapaneseFieldHoldsWordsRatherThanSentences,
   string? TranslationField,
   string? AudioField,
   string? ImageField)
{
   /// <summary>The mapping of the note type whose fields' roles are <paramref name="roles"/>.</summary>
   public static KeptNoteTypeMapping Of(DeckFieldRoles roles) =>
      new(roles.NoteType.Name,
          roles.NoteType.FieldNames,
          roles.JapaneseField,
          roles.JapaneseFieldHoldsFurigana,
          roles.JapaneseFieldHoldsWordsRatherThanSentences,
          roles.TranslationField,
          roles.AudioField,
          roles.ImageField);
}
