using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Feedback;

public sealed partial class WhatAVocabDoesInTheReservoir
{
   /// <summary>
   /// A reservoir sentence that could hold the vocab, analyzed anew with the vocab as it is now, and set against the sentence's bulk analysis,<br/>
   /// which saw the vocab as it was then.
   /// </summary>
   sealed class SentenceAnalyzedAnew
   {
      readonly ReservoirSentenceAnalysis? _bulkAnalysis;
      readonly int? _vocabNumber;

      SentenceAnalyzedAnew(ReservoirSentence sentence, IReadOnlyList<VocabMatch> vocabsMatches, ReservoirSentenceAnalysis? bulkAnalysis, int? vocabNumber)
      {
         Sentence = sentence;
         MatchesNow = vocabsMatches.Any(it => it.IsValid);
         FailureReasons = [.. vocabsMatches.Where(it => !it.IsValid).SelectMany(it => it.FailureReasons).Distinct()];
         _bulkAnalysis = bulkAnalysis;
         _vocabNumber = vocabNumber;
      }

      /// <summary><paramref name="sentence"/>, analyzed anew for <paramref name="vocab"/> as it is now.</summary>
      /// <param name="vocab">The vocab whose matches in the sentence are wanted.</param>
      /// <param name="sentence">The reservoir's sentence to analyze.</param>
      /// <param name="services">Analyzes the sentence, as JAStudio parses sentences.</param>
      /// <param name="bulkAnalysis">The sentence's bulk analysis, or none when the sentence was added to the reservoir after it.</param>
      /// <param name="vocabNumber">The vocab's number in the bulk analyses, or none when they never referred to it, as when it's new.</param>
      public static SentenceAnalyzedAnew For(VocabNote vocab, ReservoirSentence sentence, AnalysisServices services, ReservoirSentenceAnalysis? bulkAnalysis, int? vocabNumber) =>
         new(sentence, new TextAnalysis(services, sentence.Text, SentenceConfiguration.Empty()).MatchesOf(vocab), bulkAnalysis, vocabNumber);

      public ReservoirSentence Sentence { get; }

      /// <summary>Whether the vocab has a valid match in the sentence now.</summary>
      public bool MatchesNow { get; }

      /// <summary>Why the vocab's matches in the sentence that are rejected now are rejected, each reason once.</summary>
      public IReadOnlyList<string> FailureReasons { get; }

      /// <summary>Whether the vocab matches in the sentence now, and didn't when the sentence was analyzed in bulk.</summary>
      public bool IsNewlyMatching => MatchesNow && WasAnalyzedInBulk && !MatchedInTheBulkAnalysis;

      /// <summary>Whether the vocab matched in the sentence when it was analyzed in bulk, and doesn't now.</summary>
      public bool IsNoLongerMatching => !MatchesNow && WasAnalyzedInBulk && MatchedInTheBulkAnalysis;

      bool WasAnalyzedInBulk => _bulkAnalysis is not null;

      bool MatchedInTheBulkAnalysis => _bulkAnalysis is {} bulkAnalysis && _vocabNumber is {} vocabNumber && bulkAnalysis.HasAValidMatchOfTheVocabNumbered(vocabNumber);
   }
}
