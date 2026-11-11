using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>
/// How many reservoir sentences have something, such as a vocab's match, with a few of them as examples: those with audio first, then those of<br/>
/// a readable length, around a short spoken line's. Only the examples' ids are kept, so that millions of counts fit in memory.
/// </summary>
public sealed class CountWithExamples
{
   const int ExamplesKept = 3;
   const int ReadableLength = 20;

   readonly List<(long Id, double Worth)> _examples = [];

   public int Count { get; private set; }

   /// <summary>The ids of the example sentences, the best first.</summary>
   public IReadOnlyList<long> ExampleIds => [.. _examples.Select(it => it.Id)];

   public void Add(AnalyzedReservoirSentence sentence)
   {
      Count++;
      var example = (sentence.Id, WorthAsAnExample(sentence));
      if(_examples.Count < ExamplesKept)
      {
         _examples.Add(example);
      } else if(example.Item2 > _examples[^1].Worth)
      {
         _examples[^1] = example;
      } else
      {
         return;
      }

      _examples.Sort((left, right) => right.Worth.CompareTo(left.Worth));
   }

   /// <summary>How good <paramref name="sentence"/> is as an example: one with audio can be studied, and a short spoken line's length is read at a glance.</summary>
   public static double WorthAsAnExample(AnalyzedReservoirSentence sentence) => (sentence.HasAudio ? 100 : 0) - System.Math.Abs(sentence.Text.Length - ReadableLength);
}
