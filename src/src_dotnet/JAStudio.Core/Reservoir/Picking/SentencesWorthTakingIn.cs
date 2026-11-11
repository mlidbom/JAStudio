using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Reservoir.Measures;

namespace JAStudio.Core.Reservoir.Picking;

/// <summary>
/// The reservoir sentences worth taking into the working set: the fewest that give the corpus what it lacks, picked greedily. What the corpus lacks<br/>
/// is a few sentences for each thing a sentence can exercise, its features (<see cref="SentenceFeatures"/>). Each pick is the sentence giving the<br/>
/// most features still lacking, audio and then a readable length breaking ties; a feature the picks have given enough of stops counting. Only<br/>
/// sentences of a sentence's length are candidates (<see cref="LongestSentenceWorthTakingIn"/>).
/// </summary>
/// <remarks>
/// The gains only shrink as sentences are picked, so a sentence's gain is worked out anew only when it comes to the top of the queue, the usual<br/>
/// lazy greedy pick: if its gain is still the top one, it's picked, and otherwise it goes back in with its new gain.
/// </remarks>
public sealed partial class SentencesWorthTakingIn
{
   /// <summary>
   /// The longest sentence worth taking in, in characters: as long as the corpus's sentences, which are 15 characters long at the median and 50 at<br/>
   /// the 99th percentile (2026-11-11). A longer text, such as an audiobook's paragraph, gives the most features of all, so the greedy pick took<br/>
   /// little else, but it's no sentence to study, nor one whose parsing is checked at a glance.
   /// </summary>
   public const int LongestSentenceWorthTakingIn = 50;

   SentencesWorthTakingIn(ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, int maximum)
   {
      var features = new SentenceFeatures(measures, corpus);
      var candidates = reservoir.AnalyzedSentences()
                                .Where(it => it.Text.Length <= LongestSentenceWorthTakingIn)
                                .Select(it => Candidate.Of(it, features))
                                .Where(it => it.FeaturesStillLacking.Length > 0)
                                .ToList();
      GainsBySource = candidates.GroupBy(it => it.SourceId).ToDictionary(it => it.Key, it => it.Sum(candidate => candidate.FeaturesStillLacking.Length));
      Picked = PickGreedily(candidates, features, maximum);
   }

   /// <summary>The sentences picked, in the order they were picked: the most valuable first.</summary>
   public IReadOnlyList<SentenceWorthTakingIn> Picked { get; }

   /// <summary>For each source, by its id, how many features still lacking its sentences of a sentence's length give, before any pick: how much the source has to give.</summary>
   public IReadOnlyDictionary<long, int> GainsBySource { get; }

   /// <summary>Picks up to <paramref name="maximum"/> sentences of <paramref name="reservoir"/>, by what they'd give the corpus.</summary>
   public static SentencesWorthTakingIn PickedFrom(ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, int maximum) =>
      new(reservoir, measures, corpus, maximum);

   static List<SentenceWorthTakingIn> PickGreedily(IEnumerable<Candidate> candidates, SentenceFeatures features, int maximum)
   {
      var queue = new PriorityQueue<Candidate, (int, double)>(candidates.Select(it => (it, Priority(it))));
      var picked = new List<SentenceWorthTakingIn>();
      while(picked.Count < maximum && queue.TryDequeue(out var candidate, out var priority))
      {
         if(candidate.Gain(features) == 0) continue;

         var current = Priority(candidate);
         if(current != priority)
         {
            queue.Enqueue(candidate, current);
            continue;
         }

         picked.Add(Pick(candidate));
      }

      return picked;

      SentenceWorthTakingIn Pick(Candidate candidate)
      {
         var given = candidate.FeaturesStillLacking.Where(features.IsStillLacking).ToList();
         foreach(var feature in given) features.GiveOnce(feature);
         return new SentenceWorthTakingIn(candidate.Id, candidate.SourceId, [.. given.Select(features.DescriptionOf)]);
      }

      // The queue gives the smallest priority first: the largest gain, then the best example (AnalyzedReservoirSentence.WorthAsAnExample).
      (int, double) Priority(Candidate candidate) => (-candidate.Gain(features), -candidate.WorthAsAnExample);
   }
}
