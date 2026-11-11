using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>
/// What the reservoir's analyzed sentences exercise, counted in one pass over them: for each vocab, the sentences matching it; for each vocab<br/>
/// and failure reason, the sentences where the reason rejects one of its matches; and for each word no vocab has, the sentences displaying it,<br/>
/// each count with a few examples (<see cref="CountWithExamples"/>). Set against <see cref="WhatTheCorpusHas"/>, they say what the reservoir<br/>
/// could add to the corpus.
/// </summary>
public sealed class ReservoirMeasures
{
   ReservoirMeasures(ReservoirFile reservoir)
   {
      Numbers = reservoir.AnalysisNumbers();
      foreach(var sentence in reservoir.AnalyzedSentences())
      {
         AnalyzedSentences++;
         var words = sentence.Analysis.Words;
         if(words.Where(it => it.IsDisplayed).All(it => it.Kind == ReservoirWordKind.Vocab)) SentencesWithEveryDisplayedWordAVocab++;

         foreach(var vocabNumber in words.Where(it => it.Kind == ReservoirWordKind.Vocab).Select(it => it.VocabNumber).Distinct())
            CountFor(SentencesByVocab, vocabNumber).Add(sentence);

         foreach(var rejection in sentence.Analysis.RejectedVocabMatches.SelectMany(it => it.ReasonNumbers.Select(reason => (it.VocabNumber, reason))).Distinct())
            CountFor(SentencesByRejection, rejection).Add(sentence);

         foreach(var word in words.Where(it => it is { IsDisplayed: true } and not { Kind: ReservoirWordKind.Vocab }).Select(it => (it.Form, it.Kind)).Distinct())
            CountFor(SentencesByUnmatchedWord, word).Add(sentence);
      }
   }

   /// <summary>Measures the analyzed sentences of <paramref name="reservoir"/>, reading them all once.</summary>
   public static ReservoirMeasures Of(ReservoirFile reservoir) => new(reservoir);

   /// <summary>The numbers the analyses give the vocab and the failure reasons.</summary>
   public ReservoirAnalysisNumbers Numbers { get; }

   public long AnalyzedSentences { get; }

   /// <summary>How many sentences display only vocab: no word that only the dictionary has, or nothing does.</summary>
   public long SentencesWithEveryDisplayedWordAVocab { get; }

   /// <summary>The sentences with a valid match of each vocab, by the vocab's number.</summary>
   public Dictionary<int, CountWithExamples> SentencesByVocab { get; } = [];

   /// <summary>The sentences in which each failure reason rejects a match of each vocab, by the vocab's and the reason's numbers.</summary>
   public Dictionary<(int VocabNumber, int ReasonNumber), CountWithExamples> SentencesByRejection { get; } = [];

   /// <summary>The sentences that display each word no vocab has, as the sentence breakdown shows it, by its form and whether the dictionary has it.</summary>
   public Dictionary<(string Form, ReservoirWordKind Kind), CountWithExamples> SentencesByUnmatchedWord { get; } = [];

   static CountWithExamples CountFor<TKey>(Dictionary<TKey, CountWithExamples> counts, TKey key) where TKey : notnull
   {
      if(!counts.TryGetValue(key, out var count)) counts[key] = count = new CountWithExamples();
      return count;
   }
}
