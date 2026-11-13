using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Reservoir.Analysis.Numbering;

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
   readonly Action<string> _tellProgress;

   /// <param name="tellProgress">Told how each batch went: how many sentences, and how fast.</param>
   public ReservoirAnalyzing(ReservoirFile reservoir, AnalysisServices services, Action<string> tellProgress)
   {
      _reservoir = reservoir;
      _services = services;
      _tellProgress = tellProgress;
   }

   /// <summary>
   /// Analyzes every sentence with no analysis yet; every sentence, when the analyses kept are of an older format, which are forgotten first<br/>
   /// (<see cref="ReservoirFile.KeepsAnalysesOfAnOlderFormat"/>).
   /// </summary>
   public void AnalyzeEveryUnanalyzedSentence()
   {
      ForgetTheAnalysesKeptWhenTheyAreOfAnOlderFormat();
      var numbers = _reservoir.AnalysisNumbers();
      var analyzed = 0L;
      var lastAnalyzedId = 0L;
      while(_reservoir.UnanalyzedSentences(lastAnalyzedId, SentencesPerBatch) is { Count: > 0 } batch)
      {
         var watch = Stopwatch.StartNew();
         _reservoir.SaveAnalyses(AnalysesOf(batch, numbers), numbers);
         analyzed += batch.Count;
         lastAnalyzedId = batch[^1].Id;
         _tellProgress($"{analyzed} sentences analyzed, the last {batch.Count} at {batch.Count / watch.Elapsed.TotalSeconds:F0} a second");
      }
   }

   void ForgetTheAnalysesKeptWhenTheyAreOfAnOlderFormat()
   {
      if(!_reservoir.KeepsAnalysesOfAnOlderFormat) return;

      _reservoir.ForgetTheAnalysesOfAnOlderFormat();
      _tellProgress("The analyses kept were of an older format, so they're forgotten, and every sentence is analyzed anew.");
   }

   /// <summary>The analysis of each sentence of <paramref name="batch"/>, made in parallel, with the id of its sentence.</summary>
   List<(long SentenceId, ReservoirSentenceAnalysis Analysis)> AnalysesOf(IReadOnlyList<ReservoirSentence> batch, ReservoirAnalysisNumbers numbers) =>
      batch.AsParallel()
           .Select(it => (it.Id, ReservoirSentenceAnalysis.Of(new TextAnalysis(_services, it.Text, SentenceConfiguration.Empty()), numbers)))
           .ToList();
}
