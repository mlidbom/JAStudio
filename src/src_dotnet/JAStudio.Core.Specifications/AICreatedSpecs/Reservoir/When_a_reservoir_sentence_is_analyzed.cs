using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Analysis.Numbering;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A sentence's analysis kept as the reservoir keeps it (<see cref="ReservoirSentenceAnalysis"/>): the parsing case of 厳密に言えばだけど俺一人が友達だけど, with<br/>
/// the vocab of the stripped-down corpus, whose configuration rejects its first だけど, which the sentence breakdown displays all the same, as<br/>
/// nothing else covers its word.
/// </summary>
public class When_a_reservoir_sentence_is_analyzed(JAStudioWithTheStrippedDownCorpus jastudio) : IAIGeneratedSpec, IClassFixture<JAStudioWithTheStrippedDownCorpus>
{
   const string TheParsingCase = "incorrect exclusions - 厳密に言えばだけど俺一人が友達だけど - incorrect matches";
   const int StartOfTheFirstだけど = 6;

   readonly ReservoirSentenceAnalysis _analysis = ReservoirSentenceAnalysis.Of(jastudio.AnalysisOf(ParsingCasesFolder.CaseNamed(TheParsingCase)), new ReservoirAnalysisNumbers([], []));

   ReservoirWord TheFirstだけど => _analysis.Words.Single(it => it.StartIndex == StartOfTheFirstだけど && it.Form == "だけど");

   [XF] public void a_rejected_match_the_breakdown_displays_is_kept_as_displayed() => TheFirstだけど.IsDisplayed.Must().BeTrue();

   [XF] public void a_rejected_match_the_breakdown_displays_is_kept_as_not_valid() => TheFirstだけど.IsValid.Must().BeFalse();

   [XF] public void the_sentences_valid_matches_are_kept_as_valid() => _analysis.Words.Any(it => it is { Kind: ReservoirWordKind.Vocab, IsValid: true }).Must().BeTrue();
}
