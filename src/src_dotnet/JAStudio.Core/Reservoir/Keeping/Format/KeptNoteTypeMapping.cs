using System.Collections.Generic;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// How a kept deck's note type maps to a sentence: which of its fields holds the Japanese, the translation, the audio and the image, as<br/>
/// recognized when it was kept (<see cref="Sources.AnkiPackages.DeckFieldRoles"/>). The note lines hold every field as it is, so the mapping can be<br/>
/// made anew from them.
/// </summary>
public sealed record KeptNoteTypeMapping(
   string Name,
   IReadOnlyList<string> Fields,
   string? JapaneseField,
   bool JapaneseFieldHoldsFurigana,
   bool JapaneseFieldHoldsWordsRatherThanSentences,
   string? TranslationField,
   string? AudioField,
   string? ImageField);
