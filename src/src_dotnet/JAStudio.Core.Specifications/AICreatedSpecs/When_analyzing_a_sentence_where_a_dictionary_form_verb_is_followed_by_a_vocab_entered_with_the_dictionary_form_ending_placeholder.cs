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

/// <summary>
/// The sample vocabulary's <c>う以上</c> requires the dictionary form start and has no forms but its question,<br/>
/// so it can only match after verbs not ending in <c>う</c> through the forms its question expands to.
/// </summary>
public class When_analyzing_a_sentence_where_a_dictionary_form_verb_is_followed_by_a_vocab_entered_with_the_dictionary_form_ending_placeholder : SentenceAnalysisViewModelCommon, IAIGeneratedSpec
{
   public When_analyzing_a_sentence_where_a_dictionary_form_verb_is_followed_by_a_vocab_entered_with_the_dictionary_form_ending_placeholder() : base(DataNeeded.Vocabulary) {}

   bool DisplaysAMatchOfTheVocab(string sentence, string vocabQuestion) =>
      new SentenceViewModel(CreateSentence(sentence), GetService<Settings>(), GetService<VocabCollection>())
        .DisplayedMatches
        .Any(it => it.VocabMatch?.Vocab.GetQuestion() == vocabQuestion);

   [Theory]
   [InlineData("買う以上")]
   [InlineData("書く以上")]
   [InlineData("泳ぐ以上")]
   [InlineData("話す以上")]
   [InlineData("待つ以上")]
   [InlineData("死ぬ以上")]
   [InlineData("遊ぶ以上")]
   [InlineData("読む以上")]
   [InlineData("帰る以上")]
   [InlineData("食べる以上")]
   [InlineData("する以上")]
   public void the_vocab_is_displayed_whichever_dictionary_form_ending_the_verb_has(string sentence) =>
      DisplaysAMatchOfTheVocab(sentence, "う以上").Must().BeTrue();
}
