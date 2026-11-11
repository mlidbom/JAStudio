using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Reservoir;
using JAStudio.Core.Reservoir.Feedback;

namespace JAStudio.Mcp.Tools.Reservoir;

/// <summary>What <see cref="ReservoirTools.WhatAVocabDoesInTheReservoir"/> reports: a <see cref="WhatAVocabDoesInTheReservoir"/>, its sentences as examples.</summary>
/// <param name="Vocab">The vocab's question.</param>
/// <param name="TextsSearchedFor">The texts a sentence holding the vocab must contain one of: its forms, and the stems of its forms.</param>
/// <param name="SentencesContainingTheTexts">How many of the reservoir's sentences contain one of them.</param>
/// <param name="SentencesAnalyzed">How many of those were analyzed anew: all of them, or an evenly spread sample.</param>
/// <param name="Matching">The sentences the vocab matches in now.</param>
/// <param name="NewlyMatching">Those it matches in now and didn't when the reservoir was analyzed in bulk.</param>
/// <param name="NoLongerMatching">Those it matched in then and doesn't now.</param>
/// <param name="Rejections">The sentences rejecting its matches, by why, the most frequent reason first.</param>
public sealed record VocabInTheReservoir(
   string Vocab,
   IReadOnlyList<string> TextsSearchedFor,
   int SentencesContainingTheTexts,
   int SentencesAnalyzed,
   SentencesWithExamples Matching,
   SentencesWithExamples NewlyMatching,
   SentencesWithExamples NoLongerMatching,
   IReadOnlyList<RejectionWithExamples> Rejections)
{
   const int Examples = 10;

   public static VocabInTheReservoir Of(WhatAVocabDoesInTheReservoir whatItDoes) =>
      new(whatItDoes.Vocab.GetQuestion(),
          whatItDoes.Vocab.TextsASentenceItCanMatchInContainsOneOf(),
          whatItDoes.SentencesContainingItsTexts,
          whatItDoes.SentencesAnalyzed,
          WithExamples(whatItDoes.Matching),
          WithExamples(whatItDoes.NewlyMatching),
          WithExamples(whatItDoes.NoLongerMatching),
          [.. whatItDoes.RejectedByReason.OrderByDescending(it => it.Value.Count).Select(it => new RejectionWithExamples(it.Key, WithExamples(it.Value)))]);

   static SentencesWithExamples WithExamples(IReadOnlyList<ReservoirSentence> sentences) => new(sentences.Count, [.. sentences.Take(Examples).Select(it => it.Text)]);
}
