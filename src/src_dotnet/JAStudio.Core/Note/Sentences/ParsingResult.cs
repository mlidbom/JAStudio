using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// What parsing a sentence came to, as the sentence stores it: the text parsed, the parser's version, and the matches, in their order. A value:<br/>
/// two with the same text, version and matches are equal, so that a reparse that comes to the same result changes nothing.
/// </summary>
public sealed class ParsingResult : IEquatable<ParsingResult>
{
   public List<ParsedMatch> ParsedWords { get; }
   public string Sentence { get; }
   public string ParserVersion { get; }

   internal ParsingResult(List<ParsedMatch> words, string sentence, string parserVersion)
   {
      ParsedWords = words;
      Sentence = sentence.Replace(StringExtensions.InvisibleSpace.ToString(), string.Empty);
      ParserVersion = parserVersion;
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
         TextAnalysis.Version
      );

   public bool Equals(ParsingResult? other) =>
      other is not null && Sentence == other.Sentence && ParserVersion == other.ParserVersion && ParsedWords.SequenceEqual(other.ParsedWords);

   public override bool Equals(object? obj) => obj is ParsingResult other && Equals(other);

   public override int GetHashCode() => HashCode.Combine(Sentence, ParserVersion, ParsedWords.Count);
}
