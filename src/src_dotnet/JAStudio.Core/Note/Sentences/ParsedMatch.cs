using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences.Serialization;

namespace JAStudio.Core.Note.Sentences;

public class ParsedMatch
{
   /// <summary>The <see cref="Variant"/> of a word matched by its surface form, as the sentence has it.</summary>
   public const string SurfaceVariant = "S";

   /// <summary>The <see cref="Variant"/> of a word matched by its base form, the dictionary form of the inflected word the sentence has.</summary>
   public const string BaseVariant = "B";

   public int StartIndex { get; set; }
   public bool IsDisplayed { get; set; }
   public string Variant { get; set; }
   public string ParsedForm { get; set; }
   public NoteId? VocabId { get; set; }

   public ParsedMatch(string variant, int startIndex, bool isDisplayed, string word, NoteId? vocabId)
   {
      StartIndex = startIndex;
      IsDisplayed = isDisplayed;
      Variant = variant;
      ParsedForm = word;
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
}
