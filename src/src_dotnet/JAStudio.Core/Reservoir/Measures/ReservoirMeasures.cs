using System.Collections.Generic;
using Compze.Internals.SystemCE.CollectionsCE.GenericCE;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Analysis.Numbering;

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
      foreach(var sentence in reservoir.AnalyzedSentences()) Measure(sentence);
   }

   /// <summary>Measures the analyzed sentences of <paramref name="reservoir"/>, reading them all once.</summary>
   public static ReservoirMeasures Of(ReservoirFile reservoir) => new(reservoir);

   /// <summary>The numbers the analyses give the vocab and the failure reasons.</summary>
   public ReservoirAnalysisNumbers Numbers { get; }

   /// <summary>How many sentences were measured: every one with an analysis.</summary>
   public long AnalyzedSentences { get; private set; }

   /// <summary>How many sentences display only vocab: no word that only the dictionary has, or nothing does.</summary>
   public long SentencesWithEveryDisplayedWordAVocab { get; private set; }

   /// <summary>The sentences with a valid match of each vocab, by the vocab's number.</summary>
   public Dictionary<int, CountWithExamples> SentencesByVocab { get; } = [];

   /// <summary>The sentences in which each failure reason rejects a match of each vocab, by the vocab's and the reason's numbers.</summary>
   public Dictionary<VocabRejectedForAReason, CountWithExamples> SentencesByRejection { get; } = [];

   /// <summary>The sentences that display each word no vocab has, as the sentence breakdown shows it, by its form and whether the dictionary has it.</summary>
   public Dictionary<(string Form, ReservoirWordKind Kind), CountWithExamples> SentencesByUnmatchedWord { get; } = [];

   /// <summary>The sentences matching <paramref name="vocab"/>: a count of none when no sentence does.</summary>
   public CountWithExamples SentencesMatching(VocabNote vocab) =>
      (Numbers.VocabNumberOrNone(vocab) is {} vocabNumber ? SentencesByVocab.GetValueOrDefault(vocabNumber) : null) ?? new CountWithExamples();

   /// <summary>The sentences where the failure reason <paramref name="reason"/> rejects a match of <paramref name="vocab"/>: a count of none when it rejects none.</summary>
   public CountWithExamples Rejections(VocabNote vocab, string reason) =>
      (Numbers.VocabNumberOrNone(vocab) is {} vocabNumber && Numbers.ReasonNumberOrNone(reason) is {} reasonNumber
          ? SentencesByRejection.GetValueOrDefault(new VocabRejectedForAReason(vocabNumber, reasonNumber))
          : null) ?? new CountWithExamples();

   void Measure(AnalyzedReservoirSentence sentence)
   {
      AnalyzedSentences++;
      if(sentence.Analysis.DisplaysOnlyVocab()) SentencesWithEveryDisplayedWordAVocab++;

      foreach(var vocabNumber in sentence.Analysis.VocabNumbersOfItsValidMatches())
         SentencesByVocab.GetOrAdd(vocabNumber, () => new CountWithExamples()).Add(sentence);

      foreach(var rejection in sentence.Analysis.VocabRejections())
         SentencesByRejection.GetOrAdd(rejection, () => new CountWithExamples()).Add(sentence);

      foreach(var word in sentence.Analysis.DisplayedWordsNoVocabHas())
         SentencesByUnmatchedWord.GetOrAdd(word, () => new CountWithExamples()).Add(sentence);
   }
}
