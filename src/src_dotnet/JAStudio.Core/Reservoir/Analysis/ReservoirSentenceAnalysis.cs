using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using MemoryPack;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>
/// What JAStudio's analysis made of a reservoir sentence, kept with the sentence so that the measures over the reservoir need no parsing: its words<br/>
/// (<see cref="ReservoirWord"/>), and the vocab matches the analysis rejected (<see cref="RejectedVocabMatch"/>). It's made by the same<br/>
/// <see cref="TextAnalysis"/> as a sentence's stored parsing result, with no configuration, since a reservoir sentence has none.
/// </summary>
[MemoryPackable]
public sealed partial record ReservoirSentenceAnalysis(ReservoirWord[] Words, RejectedVocabMatch[] RejectedVocabMatches)
{
   /// <summary>What <paramref name="analysis"/> made of its sentence, its vocab and failure reasons numbered by <paramref name="numbers"/>.</summary>
   public static ReservoirSentenceAnalysis Of(TextAnalysis analysis, ReservoirAnalysisNumbers numbers)
   {
      var displayed = analysis.DisplayMatches.ToHashSet();
      return new ReservoirSentenceAnalysis([.. analysis.ValidMatches.Union(displayed).Select(WordOf)],
                                           [.. EveryMatch().OfType<VocabMatch>().Where(it => !it.IsValid).Select(RejectionOf)]);

      ReservoirWord WordOf(Match match) =>
         new((short)match.StartIndex, match.ParsedForm, KindOf(match), match is VocabMatch vocabMatch ? numbers.VocabNumber(vocabMatch.Vocab) : 0, displayed.Contains(match));

      RejectedVocabMatch RejectionOf(VocabMatch match) =>
         new((short)match.StartIndex, numbers.VocabNumber(match.Vocab), [.. match.FailureReasons.Select(numbers.ReasonNumber)]);

      IEnumerable<Match> EveryMatch() =>
         analysis.Locations.SelectMany(it => it.CandidateWords).SelectMany(it => it.Variants).SelectMany(it => it.Matches);
   }

   static ReservoirWordKind KindOf(Match match) => match switch
   {
      VocabMatch => ReservoirWordKind.Vocab,
      DictionaryMatch => ReservoirWordKind.Dictionary,
      MissingMatch => ReservoirWordKind.Missing,
      _ => throw new System.ArgumentOutOfRangeException(nameof(match), match.GetType(), "A kind of match the analysis doesn't make.")
   };
}
