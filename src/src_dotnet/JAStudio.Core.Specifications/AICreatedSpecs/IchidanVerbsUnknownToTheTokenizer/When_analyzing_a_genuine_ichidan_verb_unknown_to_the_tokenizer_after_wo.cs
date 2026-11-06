using Compze.Must;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

/// <summary>
/// 点ける ("to switch on") is missing from the tokenizer's dictionary, and 点け is also the e-stem of the intransitive godan verb 点く ("to come on"),<br/>
/// whose potential form would also be 点ける. An intransitive verb does not take を, so after を it is the transitive ichidan verb.
/// </summary>
public class When_analyzing_a_genuine_ichidan_verb_unknown_to_the_tokenizer_after_wo : IchidanVerbUnknownToTheTokenizerSpecification
{
   [Theory]
   [InlineData("電気を点けて下さい。")]
   [InlineData("テレビを点けてください。")]
   [InlineData("ラジオを点けました。")]
   public void the_ichidan_verb_is_displayed_rather_than_the_godan_verb_it_looks_like_the_potential_form_of(string sentence) =>
      DisplayedWordsOf(sentence).Must().Contain("点ける");
}
