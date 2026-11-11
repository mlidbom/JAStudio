using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// Which fields of a deck's note type hold the parts of a sentence: the Japanese, its translation, its audio and its image. Every deck names<br/>
/// them as it likes, such as Expression, source_question or Sentence for the Japanese, so a field is recognized by its name where the name is a<br/>
/// known one, and by what it holds in a sample of the notes otherwise.
/// </summary>
public sealed partial class DeckFieldRoles
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

   public string? TranslationField { get; }
   public string? AudioField { get; }
   public string? ImageField { get; }

   /// <summary>The roles of <paramref name="noteType"/>'s fields, recognized from their names and from <paramref name="sample"/>, some of its notes.</summary>
   public static DeckFieldRoles RecognizedIn(AnkiNoteType noteType, IReadOnlyList<RawNote> sample)
   {
      var fields = noteType.FieldNames.Select(name => FieldContents.Of(name, sample)).ToList();
      var japanese = TheJapaneseField();
      return new DeckFieldRoles(noteType, japanese?.Name, TheTranslationField()?.Name, TheMediaField(it => it.SoundShare)?.Name, TheMediaField(it => it.ImageShare)?.Name);

      FieldContents? TheJapaneseField()
      {
         var candidates = fields.Where(it => it is { JapaneseShare: >= 0.5, SoundShare: < 0.5, ImageShare: < 0.5, FuriganaShare: < 0.3 }).ToList();
         return candidates.Where(it => KnownJapaneseFieldNames.Contains(it.Name.ToLowerInvariant())).MinBy(it => Array.IndexOf(KnownJapaneseFieldNames, it.Name.ToLowerInvariant()))
             ?? candidates.Where(it => !NamePartsOfFieldsThatHoldNoSentence.Any(part => it.Name.Contains(part, StringComparison.OrdinalIgnoreCase)))
                          .MaxBy(it => it.JapaneseShare + Math.Min(it.AverageLength, 40) / 400.0);
      }

      FieldContents? TheTranslationField()
      {
         var candidates = fields.Where(it => it != japanese && it is { LatinShare: >= 0.5, SoundShare: < 0.5, ImageShare: < 0.5 }).ToList();
         return candidates.Where(it => KnownTranslationFieldNames.Contains(it.Name.ToLowerInvariant())).MinBy(it => Array.IndexOf(KnownTranslationFieldNames, it.Name.ToLowerInvariant()))
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
                                TranslationField is null ? null : CleanTranslation(note.Field(TranslationField)),
                                AudioField is null ? null : SoundFile().Match(note.Field(AudioField)) is { Success: true } sound ? sound.Groups[1].Value : null,
                                ImageField is null ? null : ImageFile().Match(note.Field(ImageField)) is { Success: true } image ? image.Groups[1].Value : null);

      static string? CleanTranslation(string field) => AnyTag().Replace(field, " ").Trim() is { Length: > 0 } translation ? System.Net.WebUtility.HtmlDecode(translation) : null;
   }

   public override string ToString() => $"{NoteType.Name}: Japanese {JapaneseField ?? "-"}, translation {TranslationField ?? "-"}, audio {AudioField ?? "-"}, image {ImageField ?? "-"}";

   [GeneratedRegex(@"\[sound:([^\]]+)\]")] internal static partial Regex SoundFile();
   [GeneratedRegex(@"<img[^>]*?src\s*=\s*[""']?([^""'>]+)", RegexOptions.IgnoreCase)] internal static partial Regex ImageFile();
   [GeneratedRegex(@"<[^>]*>")] private static partial Regex AnyTag();
}
