using System;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences.Serialization;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// A match a sentence's stored parsing result holds: a word of the sentence, where it starts, by which variant it matched, whether the sentence<br/>
/// breakdown displays it, and the vocab it matched, if any.
/// </summary>
/// <remarks>
/// A sentence holds about 13, and the corpus over 600,000, so what they repeat is shared: <see cref="Variant"/> is one of the two constants,<br/>
/// and <see cref="ParsedForm"/> an interned string, one for each form. Loaded, each held its own copies: 31 MB of strings (2026-11-12).
/// </remarks>
public class ParsedMatch
{
   /// <summary>The <see cref="Variant"/> of a word matched by its surface form, as the sentence has it.</summary>
   public const string SurfaceVariant = "S";

   /// <summary>The <see cref="Variant"/> of a word matched by its base form, the dictionary form of the inflected word the sentence has.</summary>
   public const string BaseVariant = "B";

   public int StartIndex { get; }
   public bool IsDisplayed { get; }
   public string Variant { get; }
   public string ParsedForm { get; }
   public NoteId? VocabId { get; }

   public ParsedMatch(string variant, int startIndex, bool isDisplayed, string word, NoteId? vocabId)
   {
      StartIndex = startIndex;
      IsDisplayed = isDisplayed;
      Variant = TheConstantFor(variant);
      ParsedForm = string.Intern(word);
      VocabId = vocabId;
   }

   public int EndIndex => StartIndex + ParsedForm.Length;

   public static ParsedMatch FromMatch(Match match) =>
      new(
         match.Variant.IsSurface ? SurfaceVariant : BaseVariant,
         match.StartIndex,
         match.IsValidForDisplay,
         match.ParsedForm,
         match is VocabMatch vocabMatch ? vocabMatch.Vocab.GetId() : null
      );

   public override string ToString() => ParsedWordSerializer.ToRow(this);

   static string TheConstantFor(string variant) => variant switch
   {
      SurfaceVariant => SurfaceVariant,
      BaseVariant => BaseVariant,
      _ => throw new ArgumentException($"A parsed match's variant is {SurfaceVariant} or {BaseVariant}, not '{variant}'.", nameof(variant))
   };
}
