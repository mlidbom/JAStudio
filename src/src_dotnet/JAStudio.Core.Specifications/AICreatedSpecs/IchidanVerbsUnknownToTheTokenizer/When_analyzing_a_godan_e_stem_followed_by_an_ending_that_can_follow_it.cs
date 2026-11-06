using System.Linq;
using Compze.Must;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.IchidanVerbsUnknownToTheTokenizer;

public class When_analyzing_a_godan_e_stem_followed_by_an_ending_that_can_follow_it : IchidanVerbUnknownToTheTokenizerSpecification
{
   [Theory]
   [InlineData("米を磨げば", "磨げ")]          // conditional ば
   [InlineData("米を磨げ！", "磨げ")]          // imperative
   [InlineData("待てど 待てども", "待て")]     // classical concessive ど and ども
   [InlineData("支離滅裂　極まれりだな", "極まれ")] // classical り in its own form
   [InlineData("食うとか飲めとか言うな", "飲め")] // a quoted imperative
   [InlineData("一発で盛り上げろ 行け夏目", "行け")] // an imperative followed by a noun
   public void the_e_stem_is_left_a_godan_verb(string sentence, string eStem) =>
      TokensOf(sentence).Any(it => it.Surface == eStem && it.IsGodanVerb).Must().BeTrue();
}
