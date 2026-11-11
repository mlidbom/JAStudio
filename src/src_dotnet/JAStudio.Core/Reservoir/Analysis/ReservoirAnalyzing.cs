using System;
using System.Diagnostics;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>
/// Analyzes the reservoir's sentences that have no analysis yet, as JAStudio analyzes a sentence for its stored parsing result, with the vocab<br/>
/// JAStudio has loaded. It goes in batches, the sentences of a batch in parallel, and saves each batch as it's done, so it can be stopped and<br/>
/// started again, and it takes in sentences added to the reservoir meanwhile.
/// </summary>
public sealed class ReservoirAnalyzing
{
   const int SentencesPerBatch = 20_000;

   readonly ReservoirFile _reservoir;
   readonly AnalysisServices _services;
   readonly Action<string> _report;

   /// <param name="report">Told how each batch went: how many sentences, and how fast.</param>
   public ReservoirAnalyzing(ReservoirFile reservoir, AnalysisServices services, Action<string> report)
   {
      _reservoir = reservoir;
      _services = services;
      _report = report;
   }

   public void AnalyzeEveryUnanalyzedSentence()
   {
      var numbers = _reservoir.AnalysisNumbers();
      var analyzed = 0L;
      var lastAnalyzedId = 0L;
      while(_reservoir.UnanalyzedSentences(lastAnalyzedId, SentencesPerBatch) is { Count: > 0 } batch)
      {
         var watch = Stopwatch.StartNew();
         var analyses = batch.AsParallel()
                             .Select(it => (it.Id, ReservoirSentenceAnalysis.Of(new TextAnalysis(_services, it.Text, SentenceConfiguration.Empty()), numbers)))
                             .ToList();
         _reservoir.SaveAnalyses(analyses, numbers);
         analyzed += batch.Count;
         lastAnalyzedId = batch[^1].Id;
         _report($"{analyzed} sentences analyzed, the last {batch.Count} at {batch.Count / watch.Elapsed.TotalSeconds:F0} a second");
      }
   }
}
