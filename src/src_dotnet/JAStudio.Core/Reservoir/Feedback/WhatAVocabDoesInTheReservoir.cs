using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Feedback;

/// <summary>
/// What a vocab does in the reservoir as it is now, for feedback on an edit to it within seconds: the reservoir's sentences that could hold it<br/>
/// are analyzed anew, with the vocab as it is, and set against their bulk analysis, which saw it as it was. It says in which sentences the vocab<br/>
/// matches, which reject it and why, and in which its matching changed since the bulk analysis: what the edit did.
/// </summary>
/// <remarks>A vocab whose texts many sentences contain, such as a particle's, has a sample of them analyzed, spread evenly over the reservoir.</remarks>
public sealed class WhatAVocabDoesInTheReservoir
{
   const int ExamplesShown = 5;

   WhatAVocabDoesInTheReservoir(VocabNote vocab, IReadOnlyList<(long Id, string Text)> containingItsTexts, IReadOnlyList<(long Id, string Text)> analyzed)
   {
      Vocab = vocab;
      SentencesContainingItsTexts = containingItsTexts.Count;
      SentencesAnalyzed = analyzed.Count;
   }

   public VocabNote Vocab { get; }

   /// <summary>How many of the reservoir's sentences contain one of the texts a sentence holding the vocab must contain.</summary>
   public int SentencesContainingItsTexts { get; }

   /// <summary>How many of them were analyzed: all of them, or an evenly spread sample.</summary>
   public int SentencesAnalyzed { get; }

   /// <summary>The sentences the vocab matches in now.</summary>
   public List<(long Id, string Text)> Matching { get; } = [];

   /// <summary>The sentences that reject the vocab's match now, by why.</summary>
   public Dictionary<string, List<(long Id, string Text)>> RejectedByReason { get; } = [];

   /// <summary>The sentences the vocab matches in now that it didn't match in when the reservoir was analyzed in bulk.</summary>
   public List<(long Id, string Text)> NewlyMatching { get; } = [];

   /// <summary>The sentences the vocab matched in when the reservoir was analyzed in bulk that it doesn't match in now.</summary>
   public List<(long Id, string Text)> NoLongerMatching { get; } = [];

   /// <summary>Analyzes up to <paramref name="mostSentencesAnalyzed"/> of the reservoir's sentences that could hold <paramref name="vocab"/>.</summary>
   public static WhatAVocabDoesInTheReservoir Of(VocabNote vocab, ReservoirFile reservoir, ReservoirSentenceTexts texts, AnalysisServices services, int mostSentencesAnalyzed)
   {
      var containingItsTexts = texts.ContainingAnyOf(vocab.TextsASentenceItCanMatchInContainsOneOf());
      var analyzed = EvenlySpreadSample(containingItsTexts, mostSentencesAnalyzed);
      var result = new WhatAVocabDoesInTheReservoir(vocab, containingItsTexts, analyzed);
      var vocabNumber = reservoir.AnalysisNumbers().Vocab.SingleOrDefault(it => it.NoteId == vocab.GetId().ToString())?.Number;
      var bulkAnalyses = reservoir.AnalysesOf(analyzed.Select(it => it.Id));
      var analysesNow = analyzed.AsParallel().AsOrdered().Select(it => (Sentence: it, Matches: TheVocabsMatchesIn(new TextAnalysis(services, it.Text, SentenceConfiguration.Empty())))).ToList();
      foreach(var (sentence, matches) in analysesNow)
      {
         var matchesNow = matches.Any(it => it.IsValid);
         if(matchesNow) result.Matching.Add(sentence);
         if(bulkAnalyses.TryGetValue(sentence.Id, out var bulk))
         {
            var matchedInBulk = vocabNumber is {} number && bulk.Words.Any(it => it.VocabNumber == number);
            if(matchesNow && !matchedInBulk) result.NewlyMatching.Add(sentence);
            if(!matchesNow && matchedInBulk) result.NoLongerMatching.Add(sentence);
         }
         foreach(var reason in matches.Where(it => !it.IsValid).SelectMany(it => it.FailureReasons).Distinct())
            result.RejectedFor(reason).Add(sentence);
      }

      return result;

      IReadOnlyList<VocabMatch> TheVocabsMatchesIn(TextAnalysis analysis) =>
         [.. analysis.Locations.SelectMany(it => it.CandidateWords).SelectMany(it => it.Variants).SelectMany(it => it.Matches).OfType<VocabMatch>().Where(it => it.Vocab.Equals(vocab))];
   }

   /// <summary>What the vocab does, as text: the counts, and a few examples of each.</summary>
   public string Described()
   {
      var description = new StringBuilder()
                       .AppendLine($"{Vocab.GetQuestion()}: {SentencesContainingItsTexts:N0} reservoir sentences contain one of {string.Join(", ", Vocab.TextsASentenceItCanMatchInContainsOneOf())}; {SentencesAnalyzed:N0} analyzed.")
                       .AppendLine($"Matches in {Matching.Count:N0}: {Examples(Matching)}")
                       .AppendLine($"Newly matches, since the bulk analysis, in {NewlyMatching.Count:N0}: {Examples(NewlyMatching)}")
                       .AppendLine($"No longer matches in {NoLongerMatching.Count:N0}: {Examples(NoLongerMatching)}");
      foreach(var (reason, sentences) in RejectedByReason.OrderByDescending(it => it.Value.Count))
         description.AppendLine($"Rejected in {sentences.Count:N0}, {reason.ReplaceLineEndings(" ")}: {Examples(sentences)}");
      return description.ToString();

      static string Examples(List<(long Id, string Text)> sentences) => string.Join(" / ", sentences.Take(ExamplesShown).Select(it => $"「{it.Text}」"));
   }

   List<(long Id, string Text)> RejectedFor(string reason)
   {
      if(!RejectedByReason.TryGetValue(reason, out var sentences)) RejectedByReason[reason] = sentences = [];
      return sentences;
   }

   static IReadOnlyList<(long Id, string Text)> EvenlySpreadSample(IReadOnlyList<(long Id, string Text)> sentences, int most) =>
      sentences.Count <= most ? sentences : [.. Enumerable.Range(0, most).Select(index => sentences[(int)((long)index * sentences.Count / most)])];
}
