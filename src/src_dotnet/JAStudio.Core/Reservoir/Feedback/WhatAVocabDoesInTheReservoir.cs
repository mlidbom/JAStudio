using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Reservoir.Feedback;

/// <summary>
/// What a vocab does in the reservoir as it is now, for feedback on an edit to it within seconds: the reservoir's sentences that could hold it<br/>
/// are analyzed anew, with the vocab as it is, and set against their bulk analysis, which saw it as it was. It says in which sentences the vocab<br/>
/// matches, which reject it and why, and in which its matching changed since the bulk analysis: what the edit did.
/// </summary>
/// <remarks>A vocab whose texts many sentences contain, such as a particle's, has a sample of them analyzed, spread evenly over the reservoir.</remarks>
public sealed partial class WhatAVocabDoesInTheReservoir
{
   const int ExamplesShown = 5;

   WhatAVocabDoesInTheReservoir(VocabNote vocab, int sentencesContainingItsTexts, IReadOnlyList<SentenceAnalyzedAnew> sentencesAnalyzed)
   {
      Vocab = vocab;
      SentencesContainingItsTexts = sentencesContainingItsTexts;
      SentencesAnalyzed = sentencesAnalyzed.Count;
      Matching = [.. sentencesAnalyzed.Where(it => it.MatchesNow).Select(it => it.Sentence)];
      NewlyMatching = [.. sentencesAnalyzed.Where(it => it.IsNewlyMatching).Select(it => it.Sentence)];
      NoLongerMatching = [.. sentencesAnalyzed.Where(it => it.IsNoLongerMatching).Select(it => it.Sentence)];
      RejectedByReason = sentencesAnalyzed.SelectMany(sentence => sentence.FailureReasons.Select(reason => (Reason: reason, sentence.Sentence)))
                                          .GroupBy(it => it.Reason)
                                          .ToDictionary(it => it.Key, IReadOnlyList<ReservoirSentence> (it) => [.. it.Select(rejection => rejection.Sentence)]);
   }

   public VocabNote Vocab { get; }

   /// <summary>How many of the reservoir's sentences contain one of the texts a sentence holding the vocab must contain.</summary>
   public int SentencesContainingItsTexts { get; }

   /// <summary>How many of them were analyzed: all of them, or an evenly spread sample.</summary>
   public int SentencesAnalyzed { get; }

   /// <summary>The sentences the vocab matches in now.</summary>
   public IReadOnlyList<ReservoirSentence> Matching { get; }

   /// <summary>The sentences that reject the vocab's match now, by why, each reason in the order of the first sentence giving it.</summary>
   public IReadOnlyDictionary<string, IReadOnlyList<ReservoirSentence>> RejectedByReason { get; }

   /// <summary>The sentences the vocab matches in now that it didn't match in when the reservoir was analyzed in bulk.</summary>
   public IReadOnlyList<ReservoirSentence> NewlyMatching { get; }

   /// <summary>The sentences the vocab matched in when the reservoir was analyzed in bulk that it doesn't match in now.</summary>
   public IReadOnlyList<ReservoirSentence> NoLongerMatching { get; }

   /// <summary>Analyzes up to <paramref name="mostSentencesAnalyzed"/> of the reservoir's sentences that could hold <paramref name="vocab"/>.</summary>
   public static WhatAVocabDoesInTheReservoir Of(VocabNote vocab, ReservoirFile reservoir, ReservoirSentenceTexts texts, AnalysisServices services, int mostSentencesAnalyzed)
   {
      var containingItsTexts = texts.ContainingAnyOf(vocab.TextsASentenceItCanMatchInContainsOneOf());
      var sample = EvenlySpreadSample(containingItsTexts, mostSentencesAnalyzed);
      var vocabNumber = reservoir.AnalysisNumbers().VocabNumberOrNone(vocab);
      var bulkAnalyses = reservoir.AnalysesOf(sample.Select(it => it.Id));
      var analyzedAnew = sample.AsParallel()
                               .AsOrdered()
                               .Select(sentence => SentenceAnalyzedAnew.For(vocab, sentence, services, bulkAnalyses.GetValueOrDefault(sentence.Id), vocabNumber))
                               .ToList();
      return new WhatAVocabDoesInTheReservoir(vocab, containingItsTexts.Count, analyzedAnew);
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

      static string Examples(IReadOnlyList<ReservoirSentence> sentences) => string.Join(" / ", sentences.Take(ExamplesShown).Select(it => $"「{it.Text}」"));
   }

   /// <summary>Up to <paramref name="most"/> of <paramref name="sentences"/>: all of them when there are no more, else as many spread evenly over them.</summary>
   static IReadOnlyList<ReservoirSentence> EvenlySpreadSample(IReadOnlyList<ReservoirSentence> sentences, int most) =>
      sentences.Count <= most ? sentences : [.. Enumerable.Range(0, most).Select(index => sentences[(int)((long)index * sentences.Count / most)])];
}
