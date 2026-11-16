using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

namespace JAStudio.Core.Note.Sentences.ParsingCases;

public sealed partial record DisplayedVocab
{
   /// <summary>One word of the <see cref="DisplayedVocab"/>: where it starts in the sentence, and the vocab it's matched to.</summary>
   /// <param name="StartIndex">Where in the sentence the word starts, in characters.</param>
   /// <param name="VocabId">The id of the vocab the word is matched to. Null when it's matched to no vocab.</param>
   /// <param name="FormMatchedToNoVocab">The word as it's parsed, for a word matched to no vocab, which has nothing else to tell it by. Null for a word matched to a vocab.</param>
   public sealed record Word(int StartIndex, string? VocabId, string? FormMatchedToNoVocab)
   {
      /// <summary><paramref name="displayedWord"/> as a word of the displayed vocab.</summary>
      public static Word Of(BreakdownWord displayedWord) => new(displayedWord.StartIndex, displayedWord.VocabId, displayedWord.VocabId == null ? displayedWord.Form : null);
   }
}
