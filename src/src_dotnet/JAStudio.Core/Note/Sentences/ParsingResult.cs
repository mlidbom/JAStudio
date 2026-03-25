using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.Note.Sentences;

public class ParsingResult
{
   public List<ParsedMatch> ParsedWords { get; }
   public string Sentence { get; }
   public string ParserVersion { get; }
   public string TokenizerVersion { get; }

   internal ParsingResult(List<ParsedMatch> words, string sentence, string parserVersion, string tokenizerVersion = "")
   {
      ParsedWords = words;
      Sentence = sentence.Replace(StringExtensions.InvisibleSpace, string.Empty);
      ParserVersion = parserVersion;
      TokenizerVersion = tokenizerVersion;
   }

   internal HashSet<NoteId> MatchedVocabIds
   {
      get
      {
         return ParsedWords
               .Where(p => p.VocabId != null)
               .Select(p => p.VocabId!)
               .ToHashSet();
      }
   }

   internal List<string> ParsedWordsStrings()
   {
      return ParsedWords.Select(p => p.ParsedForm).Distinct().ToList();
   }

   internal static ParsingResult FromAnalysis(TextAnalysis analysis) =>
      new(
         analysis.ValidMatches.Select(ParsedMatch.FromMatch).ToList(),
         analysis.Text,
         TextAnalysis.Version,
         JNTokenizer.Version
      );
}
