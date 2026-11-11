using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Reservoir.Analysis.Numbering;
using MemoryPack;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>
/// What JAStudio's analysis made of a reservoir sentence, kept with the sentence so that the measures over the reservoir need no parsing: its words<br/>
/// (<see cref="ReservoirWord"/>), and the vocab matches the analysis rejected (<see cref="RejectedVocabMatch"/>). It's made by the same<br/>
/// <see cref="TextAnalysis"/> as a sentence's stored parsing result, with no configuration, since a reservoir sentence has none.
/// </summary>
/// <remarks>What it says of its sentence is told by methods, not properties, so that MemoryPack keeps only its words and rejections.</remarks>
[MemoryPackable]
public sealed partial record ReservoirSentenceAnalysis(ReservoirWord[] Words, RejectedVocabMatch[] RejectedVocabMatches)
{
   /// <summary>What <paramref name="analysis"/> made of its sentence, its vocab and failure reasons numbered by <paramref name="numbers"/>.</summary>
   public static ReservoirSentenceAnalysis Of(TextAnalysis analysis, ReservoirAnalysisNumbers numbers)
   {
      var displayed = analysis.DisplayMatches.ToHashSet();
      return new ReservoirSentenceAnalysis([.. analysis.ValidMatches.Union(displayed).Select(WordOf)],
                                           [.. analysis.RejectedVocabMatches.Select(RejectionOf)]);

      ReservoirWord WordOf(Match match) => new((short)match.StartIndex, match.ParsedForm, KindOf(match), VocabNumberOf(match), displayed.Contains(match), match.IsValid);

      int VocabNumberOf(Match match) => match is VocabMatch vocabMatch ? numbers.VocabNumber(vocabMatch.Vocab) : ReservoirWord.NoVocabNumber;

      RejectedVocabMatch RejectionOf(VocabMatch match) =>
         new((short)match.StartIndex, numbers.VocabNumber(match.Vocab), [.. match.FailureReasons.Select(numbers.ReasonNumber)]);
   }

   /// <summary>Whether the vocab with the number <paramref name="vocabNumber"/> has a valid match in the sentence.</summary>
   public bool HasAValidMatchOfTheVocabNumbered(int vocabNumber) => Words.Any(it => it.IsValid && it.VocabNumber == vocabNumber);

   /// <summary>The numbers of the vocab the sentence's words are matches of, each once.</summary>
   public IEnumerable<int> VocabNumbersOfItsValidMatches() => Words.Where(it => it is { IsValid: true, Kind: ReservoirWordKind.Vocab }).Select(it => it.VocabNumber).Distinct();

   /// <summary>Whether the sentence breakdown displays a missing word: one neither a vocab nor the dictionary has.</summary>
   public bool DisplaysAMissingWord() => Words.Any(it => it is { IsDisplayed: true, Kind: ReservoirWordKind.Missing });

   /// <summary>Whether every word the sentence breakdown displays is a vocab: none that only the dictionary has, or nothing does.</summary>
   public bool DisplaysOnlyVocab() => Words.Where(it => it.IsDisplayed).All(it => it.Kind == ReservoirWordKind.Vocab);

   /// <summary>The words the sentence breakdown displays that no vocab has, by their form and whether the dictionary has them, each once.</summary>
   public IEnumerable<(string Form, ReservoirWordKind Kind)> DisplayedWordsNoVocabHas() =>
      Words.Where(it => it is { IsDisplayed: true } and not { Kind: ReservoirWordKind.Vocab }).Select(it => (it.Form, it.Kind)).Distinct();

   /// <summary>Each vocab whose match the analysis rejected, with each failure reason rejecting it, every pair once.</summary>
   public IEnumerable<VocabRejectedForAReason> VocabRejections() =>
      RejectedVocabMatches.SelectMany(it => it.ReasonNumbers.Select(reason => new VocabRejectedForAReason(it.VocabNumber, reason))).Distinct();

   static ReservoirWordKind KindOf(Match match) => match switch
   {
      VocabMatch => ReservoirWordKind.Vocab,
      DictionaryMatch => ReservoirWordKind.Dictionary,
      MissingMatch => ReservoirWordKind.Missing,
      _ => throw new ArgumentOutOfRangeException(nameof(match), match.GetType(), "A kind of match the analysis doesn't make.")
   };
}
