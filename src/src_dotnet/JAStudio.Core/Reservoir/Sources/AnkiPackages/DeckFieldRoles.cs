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
public sealed class DeckFieldRoles
{
   static readonly string[] KnownJapaneseFieldNames = ["expression", "source_question", "sentence", "japanese", "jp", "sentence-jp", "japanese sentence", "text", "front"];
   static readonly string[] KnownTranslationFieldNames = ["meaning", "english", "source_answer", "translation", "englishtl", "sentence-en", "english translation", "back"];
   static readonly string[] NamePartsOfFieldsThatHoldNoSentence = ["reading", "furigana", "kana", "meaning", "english", "translation", "morph", "focus", "vocab", "word", "notes", "definition", "hint", "kanji", "parsed", "id", "sequence", "tags"];

   DeckFieldRoles(AnkiNoteType noteType, string? japaneseField, string? translationField, string? audioField, string? imageField)
   {
      NoteType = noteType;
      JapaneseField = japaneseField;
      TranslationField = translationField;
      AudioField = audioField;
      ImageField = imageField;
   }

   public AnkiNoteType NoteType { get; }

   /// <summary>The field holding the sentence in Japanese, or null when none was recognized, and the note type gives no sentences.</summary>
   public string? JapaneseField { get; }

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
      return new DeckFieldRoles(noteType, japanese?.Name, TheTranslationField()?.Name, TheMediaField(it => it.SoundShare)?.Name, TheMediaField(it => it.ImageShare)?.Name);

      FieldContents? TheJapaneseField()
      {
         var candidates = fields.Where(it => it.HoldsJapaneseSentences).ToList();
         return TheFirstOfTheKnownNames(candidates, KnownJapaneseFieldNames)
             ?? candidates.Where(it => !IsNamedAsAFieldThatHoldsNoSentence(it)).MaxBy(HowLikelyItHoldsTheSentences);
      }

      FieldContents? TheTranslationField()
      {
         var candidates = fields.Where(it => it != japanese && it.HoldsTranslations).ToList();
         return TheFirstOfTheKnownNames(candidates, KnownTranslationFieldNames)
             ?? candidates.Where(it => it.LatinShare >= 0.6).MaxBy(it => it.LatinShare);
      }

      // Several fields of one kind of media, such as a sentence's audio and a word's, are told apart by "sentence" in the name.
      FieldContents? TheMediaField(Func<FieldContents, double> share) =>
         fields.Where(it => share(it) >= 0.5).OrderByDescending(it => it.Name.Contains("sentence", StringComparison.OrdinalIgnoreCase)).ThenByDescending(share).FirstOrDefault();
   }

   /// <summary>The sentence <paramref name="note"/> holds, or null when its Japanese field holds none, or the note type has no Japanese field.</summary>
   public SourceSentence? SentenceOf(RawNote note)
   {
      if(JapaneseField is null || ReservoirSentenceText.CleanedFrom(note.Field(JapaneseField)) is not {} text) return null;

      return new SourceSentence(text,
                                $"note {note.AnkiNoteId}",
                                TranslationField is null ? null : AnkiFieldMarkup.TextIn(note.Field(TranslationField)),
                                AudioField is null ? null : AnkiFieldMarkup.SoundFileIn(note.Field(AudioField)),
                                ImageField is null ? null : AnkiFieldMarkup.ImageFileIn(note.Field(ImageField)));
   }

   public override string ToString() => $"{NoteType.Name}: Japanese {JapaneseField ?? "-"}, translation {TranslationField ?? "-"}, audio {AudioField ?? "-"}, image {ImageField ?? "-"}";

   /// <summary>The one of <paramref name="candidates"/> named first in <paramref name="knownNames"/>, whatever the case, or none when none is named so.</summary>
   static FieldContents? TheFirstOfTheKnownNames(IEnumerable<FieldContents> candidates, string[] knownNames) =>
      candidates.Where(it => knownNames.Contains(it.Name.ToLowerInvariant())).MinBy(it => Array.IndexOf(knownNames, it.Name.ToLowerInvariant()));

   static bool IsNamedAsAFieldThatHoldsNoSentence(FieldContents field) =>
      NamePartsOfFieldsThatHoldNoSentence.Any(part => field.Name.Contains(part, StringComparison.OrdinalIgnoreCase));

   /// <summary>How likely <paramref name="field"/>, of those holding Japanese, holds the sentences: the more Japanese, and the longer up to a sentence's length, the likelier.</summary>
   static double HowLikelyItHoldsTheSentences(FieldContents field) => field.JapaneseShare + Math.Min(field.AverageLength, 40) / 400.0;
}
