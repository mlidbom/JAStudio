using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>
/// The reservoir sentences worth taking into the working set: the fewest that give the corpus what it lacks, picked greedily. What the corpus lacks<br/>
/// is a few sentences for each thing a sentence can exercise, its features: a vocab's match, a failure reason rejecting a vocab's match, a word no<br/>
/// vocab has that sentences of several works show, where a name stays in one. Each pick is the sentence giving the most features still<br/>
/// lacking, audio and then a readable length breaking ties; a feature the picks have given enough of stops counting.
/// </summary>
/// <remarks>
/// The gains only shrink as sentences are picked, so a sentence's gain is worked out anew only when it comes to the top of the queue, the usual<br/>
/// lazy greedy pick: if its gain is still the top one, it's picked, and otherwise it goes back in with its new gain.
/// </remarks>
public sealed class SentencesWorthTakingIn
{
   const int SentencesWantedForAVocab = 3;
   const int SentencesWantedForARejection = 2;
   const int SentencesWantedForAWord = 1;
   const int WorksShowingADictionaryWordForItToCount = 3;
   const int WorksShowingAMissingWordForItToCount = 5;

   readonly List<string> _featureDescriptions = [];
   readonly List<int> _stillLacking = [];
   readonly Dictionary<string, int> _featureIds = [];

   SentencesWorthTakingIn(ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, int maximum)
   {
      var vocabByNumber = measures.Numbers.Vocab.ToDictionary(it => it.Number);
      var reasonByNumber = measures.Numbers.Reasons.ToDictionary(it => it.Value, it => it.Key);
      var candidates = reservoir.AnalyzedSentences().Select(AsCandidate).Where(it => it.Features.Length > 0).ToList();
      GainsBySource = candidates.GroupBy(it => it.SourceId).ToDictionary(it => it.Key, it => it.Sum(candidate => candidate.Features.Length));
      Picked = PickGreedily(candidates, maximum);
      return;

      Candidate AsCandidate(AnalyzedReservoirSentence sentence) =>
         new(sentence.Id, sentence.SourceId, CountWithExamples.WorthAsAnExample(sentence), [.. FeaturesOf(sentence.Analysis).Distinct()]);

      IEnumerable<int> FeaturesOf(ReservoirSentenceAnalysis analysis)
      {
         foreach(var word in analysis.Words)
         {
            if(word.Kind == ReservoirWordKind.Vocab)
               yield return Feature($"v{word.VocabNumber}", () => SentencesWantedForAVocab - corpus.SentencesMatching(vocabByNumber[word.VocabNumber].NoteId), () => $"the vocab {vocabByNumber[word.VocabNumber].Question}");
            else if(word.IsDisplayed && measures.SentencesByUnmatchedWord[(word.Form, word.Kind)].Works >= (word.Kind == ReservoirWordKind.Dictionary ? WorksShowingADictionaryWordForItToCount : WorksShowingAMissingWordForItToCount))
               yield return Feature($"w{word.Kind}{word.Form}", () => SentencesWantedForAWord - corpus.SentencesDisplayingUnmatched(word.Form), () => $"the {(word.Kind == ReservoirWordKind.Dictionary ? "dictionary-only" : "missing")} word {word.Form}");
         }

         foreach(var (vocabNumber, reasonNumber) in analysis.RejectedVocabMatches.SelectMany(it => it.ReasonNumbers.Select(reason => (it.VocabNumber, reason))))
            yield return Feature($"r{vocabNumber}:{reasonNumber}",
                                 () => SentencesWantedForARejection - corpus.Rejections(vocabByNumber[vocabNumber].NoteId, reasonByNumber[reasonNumber]),
                                 () => $"{vocabByNumber[vocabNumber].Question} rejected: {reasonByNumber[reasonNumber]}");
      }
   }

   /// <summary>The sentences picked, in the order they were picked: the most valuable first.</summary>
   public IReadOnlyList<SentenceWorthTakingIn> Picked { get; }

   /// <summary>For each source, by its id, how many features still lacking its sentences give, before any pick: how much the source has to give.</summary>
   public IReadOnlyDictionary<long, int> GainsBySource { get; }

   /// <summary>Picks up to <paramref name="maximum"/> sentences of <paramref name="reservoir"/>, by what they'd give the corpus.</summary>
   public static SentencesWorthTakingIn PickedFrom(ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, int maximum) =>
      new(reservoir, measures, corpus, maximum);

   /// <summary>The id of the feature <paramref name="key"/>, or none when the corpus already has enough of it.</summary>
   int Feature(string key, Func<int> lacking, Func<string> description)
   {
      if(_featureIds.TryGetValue(key, out var id)) return id;

      id = _featureIds[key] = _stillLacking.Count;
      _stillLacking.Add(Math.Max(0, lacking()));
      _featureDescriptions.Add(description());
      return id;
   }

   List<SentenceWorthTakingIn> PickGreedily(List<Candidate> candidates, int maximum)
   {
      foreach(var candidate in candidates) candidate.Features = [.. candidate.Features.Where(it => _stillLacking[it] > 0)];
      var queue = new PriorityQueue<Candidate, (int, double)>(candidates.Where(it => it.Features.Length > 0).Select(it => (it, Priority(it))));
      var picked = new List<SentenceWorthTakingIn>();
      while(picked.Count < maximum && queue.TryDequeue(out var candidate, out var priority))
      {
         var current = Priority(candidate);
         if(Gain(candidate) == 0) continue;
         if(current != priority)
         {
            queue.Enqueue(candidate, current);
            continue;
         }

         var given = candidate.Features.Where(it => _stillLacking[it] > 0).ToList();
         foreach(var feature in given) _stillLacking[feature]--;
         picked.Add(new SentenceWorthTakingIn(candidate.Id, candidate.SourceId, [.. given.Select(it => _featureDescriptions[it])]));
      }

      return picked;

      int Gain(Candidate candidate) => candidate.Features.Count(it => _stillLacking[it] > 0);

      // The queue gives the smallest priority first: the largest gain, then the best example (CountWithExamples.WorthAsAnExample).
      (int, double) Priority(Candidate candidate) => (-Gain(candidate), -candidate.WorthAsAnExample);
   }

   /// <summary>A sentence that gives the corpus something it lacks, with the ids of the features it gives, only those still lacking once the picking starts.</summary>
   sealed class Candidate(long id, long sourceId, double worthAsAnExample, int[] features)
   {
      public long Id { get; } = id;
      public long SourceId { get; } = sourceId;
      public double WorthAsAnExample { get; } = worthAsAnExample;
      public int[] Features { get; set; } = features;
   }
}
