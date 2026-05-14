using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.Note.Sentences;

public class SentenceConfiguration
{
   public List<string> HighlightedWords { get; }
   public WordExclusionSet IncorrectMatches { get; }
   public WordExclusionSet HiddenMatches { get; }

   public SentenceConfiguration(List<string> highlightedWords, WordExclusionSet incorrectMatches, WordExclusionSet hiddenMatches)
   {
      HighlightedWords = highlightedWords;
      IncorrectMatches = incorrectMatches;
      HiddenMatches = hiddenMatches;
   }

   public static SentenceConfiguration FromIncorrectMatches(List<WordExclusion> incorrectMatches) => FromValues([], incorrectMatches, []);

   public static SentenceConfiguration FromValues(
      List<string> highlighted,
      List<WordExclusion> incorrectMatches,
      List<WordExclusion> hiddenMatches)
   {
      return new SentenceConfiguration(
         highlighted,
         new WordExclusionSet(() => {}, incorrectMatches),
         new WordExclusionSet(() => {}, hiddenMatches));
   }

   public static SentenceConfiguration Empty() =>
      new(
         [],
         WordExclusionSet.Empty(),
         WordExclusionSet.Empty());

   public override string ToString()
   {
      var parts = new List<string>();
      if(HighlightedWords.Any())
      {
         parts.Add($"highlighted_words: [{string.Join(", ", HighlightedWords)}]");
      }

      if(!IncorrectMatches.IsEmpty())
      {
         parts.Add($"incorrect_matches: {IncorrectMatches}");
      }

      if(!HiddenMatches.IsEmpty())
      {
         parts.Add($"hidden_matches: {HiddenMatches}");
      }

      return string.Join(", ", parts);
   }
}
