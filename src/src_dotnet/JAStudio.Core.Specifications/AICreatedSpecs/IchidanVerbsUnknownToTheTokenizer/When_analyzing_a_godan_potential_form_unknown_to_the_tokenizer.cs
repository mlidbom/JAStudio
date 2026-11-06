using Compze.Must;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

public class When_analyzing_a_godan_potential_form_unknown_to_the_tokenizer : IchidanVerbUnknownToTheTokenizerSpecification
{
   public static readonly TheoryData<string> Endings = ["る", "る人", "るか", "ない", "なかった", "ないで", "た", "ます", "ません", "ました", "て", "てる", "てない", "そう", "まい", "ず", "れば"];

   [Theory]
   [MemberData(nameof(Endings))]
   public void the_tokens_are_those_of_a_potential_form_known_to_the_tokenizer(string ending) =>
      TokenDescriptionsOf(StemOfAPotentialFormUnknownToTheTokenizer + ending)
        .Must().SequenceEqual(AsIfAboutThePotentialFormUnknownToTheTokenizer(TokenDescriptionsOf(StemOfAPotentialFormKnownToTheTokenizer + ending)));

   [Theory]
   [MemberData(nameof(Endings))]
   public void the_displayed_words_are_those_of_a_potential_form_known_to_the_tokenizer(string ending) =>
      DisplayedWordsOf(StemOfAPotentialFormUnknownToTheTokenizer + ending)
        .Must().SequenceEqual(AsIfAboutThePotentialFormUnknownToTheTokenizer(DisplayedWordsOf(StemOfAPotentialFormKnownToTheTokenizer + ending)));
}
