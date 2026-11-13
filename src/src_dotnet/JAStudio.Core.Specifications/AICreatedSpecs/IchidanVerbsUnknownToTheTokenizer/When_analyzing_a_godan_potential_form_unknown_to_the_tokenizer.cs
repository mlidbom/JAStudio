using System.Linq;
using Compze.Must;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

public class When_analyzing_a_godan_potential_form_unknown_to_the_tokenizer : IchidanVerbUnknownToTheTokenizerSpecification
{
   public static readonly TheoryData<string> Endings = ["る", "る人", "るか", "ない", "なかった", "ないで", "た", "ます", "ません", "ました", "て", "てる", "てない", "そう", "まい", "ず", "れば"];

   [Theory]
   [MemberData(nameof(Endings))]
   public void the_pre_processed_tokens_are_those_of_a_potential_form_known_to_the_tokenizer(string ending) =>
      TokenDescriptionsOf(StemOfAPotentialFormUnknownToTheTokenizer + ending)
        .Must().SequenceEqual(AsIfAboutThePotentialFormUnknownToTheTokenizer(TokenDescriptionsOf(StemOfAPotentialFormKnownToTheTokenizer + ending)));

   [Theory]
   [MemberData(nameof(Endings))]
   public void the_pre_processing_stage_made_them_from_tokens_it_corrected(string ending) =>
      PreProcessedTokensOf(StemOfAPotentialFormUnknownToTheTokenizer + ending).Must().Satisfy(tokens => tokens.Any(it => it.SourceToken is CorrectedToken));

   [Theory]
   [MemberData(nameof(Endings))]
   public void the_tokenizers_tokens_are_left_as_it_gave_them_linked_to_each_other(string ending) =>
      TextAnalysis.FromText(GetService<AnalysisServices>(), StemOfAPotentialFormUnknownToTheTokenizer + ending).TokenizedText.Tokens
                  .Must().Satisfy(tokens => tokens[0].Surface == StemOfAPotentialFormUnknownToTheTokenizer &&
                                            tokens.All(it => it.IsTheTokenizersOwn) &&
                                            tokens.Zip(tokens.Skip(1)).All(neighbours => neighbours.First.Next == neighbours.Second));
}
