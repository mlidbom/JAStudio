using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>
/// How many reservoir sentences have something, such as a vocab's match, with a few of them as examples: the best by<br/>
/// <see cref="AnalyzedReservoirSentence.WorthAsAnExample"/>, those with audio first, then those of a readable length. Only the examples' ids<br/>
/// are kept, so that millions of counts fit in memory.
/// </summary>
public sealed class CountWithExamples
{
   const int ExamplesKept = 3;

   /// <summary>
   /// How many different works are counted, up to this many. How far a word spreads across works tells general vocabulary from a name or a<br/>
   /// show's own terms, which stay in one or two; beyond this many, it's general anyway.
   /// </summary>
   const int WorksCountedUpTo = 30;

   /// <summary>The examples kept, the best first.</summary>
   readonly List<(long Id, double Worth)> _examples = [];

   readonly List<long> _sourceIdsOfTheWorksCounted = [];

   /// <summary>How many sentences were counted.</summary>
   public int Count { get; private set; }

   /// <summary>How many different works the sentences come from, counted up to <see cref="WorksCountedUpTo"/>.</summary>
   public int Works => _sourceIdsOfTheWorksCounted.Count;

   /// <summary>The ids of the example sentences, the best first.</summary>
   public IReadOnlyList<long> ExampleIds => [.. _examples.Select(it => it.Id)];

   /// <summary>Counts <paramref name="sentence"/>, its work, and keeps it as an example when it's among the best.</summary>
   public void Add(AnalyzedReservoirSentence sentence)
   {
      Count++;
      CountTheWork(sentence.SourceId);
      KeepAsAnExampleWhenAmongTheBest(sentence);
   }

   void CountTheWork(long sourceId)
   {
      if(_sourceIdsOfTheWorksCounted.Count < WorksCountedUpTo && !_sourceIdsOfTheWorksCounted.Contains(sourceId)) _sourceIdsOfTheWorksCounted.Add(sourceId);
   }

   void KeepAsAnExampleWhenAmongTheBest(AnalyzedReservoirSentence sentence)
   {
      var worth = sentence.WorthAsAnExample;
      if(!IsAmongTheBestExamples(worth)) return;

      if(_examples.Count == ExamplesKept) _examples.RemoveAt(ExamplesKept - 1);
      _examples.Add((sentence.Id, worth));
      _examples.Sort((left, right) => right.Worth.CompareTo(left.Worth));
   }

   bool IsAmongTheBestExamples(double worth) => _examples.Count < ExamplesKept || worth > _examples[^1].Worth;
}
