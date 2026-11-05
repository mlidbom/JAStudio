using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Specifications.LanguageServices.TextAnalysis;
using JAStudio.Core.UI.Web.Sentence;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

public class When_analyzing_a_sentence_containing_datte : SentenceAnalysisViewModelCommon, IAIGeneratedSpec
{
   public When_analyzing_a_sentence_containing_datte() : base(DataNeeded.Vocabulary) {}

   protected List<string> DisplayedWordsOf(string sentence) =>
      [..new SentenceViewModel(CreateSentence(sentence), GetService<Settings>(), GetService<VocabCollection>()).DisplayedMatches.Select(SurfaceAndMatchForm)];

   public class where_datte_is_directly_followed_by_a_verb_of_saying_thinking_or_hearing : When_analyzing_a_sentence_containing_datte
   {
      [Theory]
      [InlineData("俺も友達だって言えばよかったじゃん", "俺", "も", "友達", "だ", "って言う", "ば", "よい", "た", "じゃん")]
      [InlineData("大事なもんだって言ってたろう", "大事", "な", "もん", "だ", "って言う", "てた", "う")]
      [InlineData("本当に友達だって言えるのか", "本当に", "友達", "だ", "って言う", "える", "うの", "か")]
      [InlineData("薄情だっていうのは分かってます", "薄情", "だ", "っていう", "の", "は", "分かる", "て", "ます")]
      [InlineData("場違いだって思うかもしれません", "場違い", "だ", "って", "思う", "う", "かもしれません")]
      [InlineData("無敵な女だって思われたい", "無敵", "な", "女", "だ", "って", "思われる", "たい")]
      [InlineData("舞っているんだって思い込むように", "舞う", "ている", "んだ:のだ", "って", "思い込む", "う", "ように")]
      [InlineData("易しいテストだって聞いてたんだけど", "易しい", "テスト", "だ", "って", "聞く", "て", "たん", "だけど")]
      public void datte_is_displayed_as_da_followed_by_a_word_starting_with_the_quoting_tte(string sentence, params string[] expectedDisplayedWords) =>
         DisplayedWordsOf(sentence).Must().SequenceEqual(expectedDisplayedWords);
   }

   public class where_datte_starts_the_sentence_and_is_followed_by_a_verb_of_saying : When_analyzing_a_sentence_containing_datte
   {
      [Theory]
      [InlineData("だって言ったじゃん", "だって", "言う", "た", "じゃん")]
      public void datte_is_displayed_as_datte(string sentence, params string[] expectedDisplayedWords) =>
         DisplayedWordsOf(sentence).Must().SequenceEqual(expectedDisplayedWords);
   }

   public class where_datte_is_directly_followed_by_a_verb_that_is_not_a_verb_of_saying_thinking_or_hearing : When_analyzing_a_sentence_containing_datte
   {
      [Theory]
      [InlineData("子供だって知ってる", "子供", "だって", "知る", "てる")]
      [InlineData("見える俺だって信じられないのに", "見える", "る:う", "俺", "だって", "信じられない", "のに")]
      [InlineData("男友達だっているでしょうに", "男友達", "だって", "いる", "る:う", "でしょう", "に")]
      public void datte_is_displayed_as_datte(string sentence, params string[] expectedDisplayedWords) =>
         DisplayedWordsOf(sentence).Must().SequenceEqual(expectedDisplayedWords);
   }
}
