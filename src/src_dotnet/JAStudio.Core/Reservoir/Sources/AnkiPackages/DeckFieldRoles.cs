using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// Which fields of a deck's note type hold the parts of a sentence: the Japanese, its translation, its audio and its image. Every deck names<br/>
/// them as it likes, such as Expression, source_question or Sentence for the Japanese, so a field is recognized by its name where the name is a<br/>
/// known one, and by what it holds in a sample of the notes otherwise.
/// </summary>
/// <remarks>
/// The Japanese field is the first of these that holds Japanese: one with a known name and no furigana; one with a known name and furigana, as<br/>
/// Migaku's decks give their sentences; one with no furigana whose name isn't a name of fields holding no sentence, such as Reading; the same<br/>
/// with furigana; and last, one named as a translation, as a deck with its fields swapped has its Japanese in Meaning.
/// </remarks>
public sealed class DeckFieldRoles
{
   static readonly string[] KnownJapaneseFieldNames = ["expression", "source_question", "sentence", "japanese", "jp", "sentence-jp", "japanese sentence", "sentkanji", "text", "front"];
   static readonly string[] KnownTranslationFieldNames = ["meaning", "english", "source_answer", "translation", "englishtl", "sentence-en", "english translation", "senteng", "back"];
   static readonly string[] NamePartsOfTranslationFields = ["meaning", "english", "translation"];
   static readonly string[] NamePartsOfFieldsThatHoldNoSentence =
      [.. NamePartsOfTranslationFields, "reading", "furigana", "kana", "morph", "focus", "vocab", "word", "target", "notes", "definition", "hint", "kanji", "parsed", "id", "sequence", "tags"];

   readonly FieldContents? _japanese;

   DeckFieldRoles(AnkiNoteType noteType, FieldContents? japanese, string? translationField, string? audioField, string? imageField)
   {
      NoteType = noteType;
      _japanese = japanese;
      TranslationField = translationField;
      AudioField = audioField;
      ImageField = imageField;
   }

   public AnkiNoteType NoteType { get; }

   /// <summary>The field holding the sentence in Japanese, or null when none was recognized, and the note type gives no sentences.</summary>
   public string? JapaneseField => _japanese?.Name;

   /// <summary>Whether the Japanese field gives its sentences with furigana, which are taken away (<see cref="AnkiFieldMarkup.TextWithoutFurigana"/>).</summary>
   public bool JapaneseFieldHoldsFurigana => _japanese?.HoldsFurigana ?? false;

   /// <summary>
   /// Whether the Japanese field holds words rather than sentences, as a vocabulary deck's does (<see cref="FieldContents.HoldsWordsRatherThanSentences"/>):<br/>
   /// the note type then gives no sentences.
   /// </summary>
   public bool HoldsWordsRatherThanSentences => _japanese?.HoldsWordsRatherThanSentences ?? false;

   /// <summary>The field holding the sentence's translation, or null when none was recognized.</summary>
   public string? TranslationField { get; }

   /// <summary>The field referring to the sentence's audio file, or null when none was recognized.</summary>
   public string? AudioField { get; }

   /// <summary>The field holding the sentence's image, such as a screenshot of the scene, or null when none was recognized.</summary>
   public string? ImageField { get; }

   /// <summary>The roles of <paramref name="noteType"/>'s fields, recognized from their names and from <paramref name="sample"/>, some of its notes.</summary>
   public static DeckFieldRoles RecognizedIn(AnkiNoteType noteType, IReadOnlyList<RawNote> sample)
   {
      var fields = noteType.FieldNames.Select(name => FieldContents.Of(name, sample)).ToList();
      var japanese = TheJapaneseField();
      return new DeckFieldRoles(noteType, japanese, TheTranslationField()?.Name, TheMediaField(it => it.SoundShare)?.Name, TheMediaField(it => it.ImageShare)?.Name);

      FieldContents? TheJapaneseField()
      {
         var holdingJapanese = fields.Where(it => it.HoldsJapanese).ToList();
         var plain = holdingJapanese.Where(it => !it.HoldsFurigana).ToList();
         var withFurigana = holdingJapanese.Where(it => it.HoldsFurigana).ToList();
         return TheFirstOfTheKnownNames(plain, KnownJapaneseFieldNames)
             ?? TheFirstOfTheKnownNames(withFurigana, KnownJapaneseFieldNames)
             ?? TheLikeliestToHoldTheSentences(plain.Where(it => !IsNamedAsAFieldThatHoldsNoSentence(it)))
             ?? TheLikeliestToHoldTheSentences(withFurigana.Where(it => !IsNamedAsAFieldThatHoldsNoSentence(it)))
             ?? TheLikeliestToHoldTheSentences(holdingJapanese.Where(IsNamedAsATranslationField));
      }

      FieldContents? TheTranslationField()
      {
         var candidates = fields.Where(it => it != japanese && it.HoldsTranslations).ToList();
         return TheFirstOfTheKnownNames(candidates, KnownTranslationFieldNames)
             ?? candidates.Where(it => it.LatinShare >= 0.6 && it.HoldsProse).MaxBy(it => it.LatinShare);
      }

      // Several fields of one kind of media, such as a sentence's audio and a word's, are told apart by "sentence" in the name.
      FieldContents? TheMediaField(Func<FieldContents, double> share) =>
         fields.Where(it => share(it) >= 0.5).OrderByDescending(it => it.Name.Contains("sentence", StringComparison.OrdinalIgnoreCase)).ThenByDescending(share).FirstOrDefault();
   }

   /// <summary>
   /// The Japanese <paramref name="note"/>'s Japanese field holds, words or a sentence, cleaned as the reservoir keeps it, or null when it holds<br/>
   /// none, or the note type has no Japanese field.
   /// </summary>
   public ReservoirSentenceText? JapaneseTextOf(RawNote note) => _japanese?.TextOf(note.Field(_japanese.Name));

   /// <summary>The sentence <paramref name="note"/> holds, or null when it holds none, or the note type holds words rather than sentences, or has no Japanese field.</summary>
   public SourceSentence? SentenceOf(RawNote note)
   {
      if(HoldsWordsRatherThanSentences || JapaneseTextOf(note) is not {} text) return null;

      return new SourceSentence(text,
                                $"note {note.AnkiNoteId}",
                                TranslationField is null ? null : AnkiFieldMarkup.TextIn(note.Field(TranslationField)),
                                AudioField is null ? null : AnkiFieldMarkup.SoundFileIn(note.Field(AudioField)),
                                ImageField is null ? null : AnkiFieldMarkup.ImageFileIn(note.Field(ImageField)));
   }

   public override string ToString() =>
      $"{NoteType.Name}: Japanese {JapaneseField ?? "-"}{(JapaneseFieldHoldsFurigana ? " (furigana taken away)" : "")}{(HoldsWordsRatherThanSentences ? " (words, not sentences)" : "")}, "
    + $"translation {TranslationField ?? "-"}, audio {AudioField ?? "-"}, image {ImageField ?? "-"}";

   /// <summary>The one of <paramref name="candidates"/> named first in <paramref name="knownNames"/>, whatever the case, or none when none is named so.</summary>
   static FieldContents? TheFirstOfTheKnownNames(IEnumerable<FieldContents> candidates, string[] knownNames) =>
      candidates.Where(it => knownNames.Contains(it.Name.ToLowerInvariant())).MinBy(it => Array.IndexOf(knownNames, it.Name.ToLowerInvariant()));

   /// <summary>The one of <paramref name="candidates"/> likeliest to hold the sentences: the more Japanese, and the longer up to a sentence's length, the likelier.</summary>
   static FieldContents? TheLikeliestToHoldTheSentences(IEnumerable<FieldContents> candidates) => candidates.MaxBy(it => it.JapaneseShare + Math.Min(it.AverageTextLength, 40) / 400.0);

   static bool IsNamedAsAFieldThatHoldsNoSentence(FieldContents field) => IsNamedWithAnyOf(field, NamePartsOfFieldsThatHoldNoSentence);

   static bool IsNamedAsATranslationField(FieldContents field) => IsNamedWithAnyOf(field, NamePartsOfTranslationFields);

   static bool IsNamedWithAnyOf(FieldContents field, string[] nameParts) => nameParts.Any(part => field.Name.Contains(part, StringComparison.OrdinalIgnoreCase));
}
