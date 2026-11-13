using System.Linq;
using Compze.Must;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
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
   public void the_tokenizers_e_stem_is_read_as_an_ichidan_verb_while_its_tags_stay_a_godan_verbs(string ending) =>
      TheTokenizersTokensOf(StemOfAPotentialFormUnknownToTheTokenizer + ending)[0]
        .Must().Satisfy(eStem => eStem.Surface == StemOfAPotentialFormUnknownToTheTokenizer &&
                                 eStem.IsIchidanVerb && !eStem.IsGodanVerb &&
                                 eStem.InflectionType.Family == InflectionTypes.Godan.Family);

   [Theory]
   [MemberData(nameof(Endings))]
   public void the_tokenizers_tokens_are_left_as_it_gave_them_linked_to_each_other(string ending) =>
      TheTokenizersTokensOf(StemOfAPotentialFormUnknownToTheTokenizer + ending)
        .Must().Satisfy(tokens => tokens[0].Surface == StemOfAPotentialFormUnknownToTheTokenizer &&
                                  tokens.All(it => it.IsTheTokenizersOwn) &&
                                  tokens.Zip(tokens.Skip(1)).All(neighbours => neighbours.First.Next == neighbours.Second));
}
