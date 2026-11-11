using System.Linq;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Picking;

public sealed partial class SentencesWorthTakingIn
{
   /// <summary>A sentence that may give the corpus something it lacks, with the ids of its features the corpus lacks when the picking starts.</summary>
   /// <param name="FeaturesStillLacking">The ids of the sentence's features the corpus lacks sentences of, before any is picked.</param>
   sealed record Candidate(long Id, long SourceId, double WorthAsAnExample, int[] FeaturesStillLacking)
   {
      /// <summary>
      /// <paramref name="sentence"/> as a candidate, its features found by <paramref name="features"/>. The corpus lacks as many sentences of each<br/>
      /// feature found so far as it did when it was found, since none is picked yet.
      /// </summary>
      public static Candidate Of(AnalyzedReservoirSentence sentence, SentenceFeatures features)
      {
         return new Candidate(sentence.Id, sentence.SourceId, sentence.WorthAsAnExample, [.. features.Of(sentence.Analysis).Distinct().Where(features.IsStillLacking)]);
      }

      /// <summary>How many of its features the corpus still lacks: what picking it gives now.</summary>
      public int Gain(SentenceFeatures features) => FeaturesStillLacking.Count(features.IsStillLacking);
   }
}
