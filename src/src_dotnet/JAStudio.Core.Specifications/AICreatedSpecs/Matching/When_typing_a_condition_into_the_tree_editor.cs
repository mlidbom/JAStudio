using System.Linq;
using System.Text.RegularExpressions;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// A condition made from what's typed into the Matching tab's tree editor, by a <see cref="ConditionTestWithTexts"/> such as "ends with one of", and what<br/>
/// the editor shows typed when a condition is changed.
/// </summary>
public class When_typing_a_condition_into_the_tree_editor : IAIGeneratedSpec
{
   static ConditionTestWithTexts TextTest(string name) => TextConditions.Level.TestsWithTexts.Single(it => it.Name == name);
   static ConditionTestWithTexts TokenTest(string name) => TokenConditions.Level.TestsWithTexts.Single(it => it.Name == name);

   /// <summary>
   /// <paramref name=reason/>, a reason that must be there, as a string that isn't nullable, which Compze.Must's string assertions need. A missing<br/>
   /// one shows as <c>(no reason)</c>, so the assertion fails saying so.
   /// </summary>
   static string TheReason(string? reason)
   {
      //todo:compze: Compze.Must's string assertions, such as Be and Contain, take an IAssertionContext<string>, not <string?>, and NotBeNull doesn't
      //   narrow one to the other, so asserting on a string? gives warning CS8620. Once they accept a string?, or NotBeNull narrows, assert on the reason itself.
      return reason ?? "(no reason)";
   }

   public class for_ends_with_one_of : When_typing_a_condition_into_the_tree_editor
   {
      static readonly ConditionTestWithTexts EndsWithOneOf = TextTest("ends with one of");

      [XF] public void texts_separated_by_spaces_make_one_ending_each() => EndsWithOneOf.MadeFrom("ん  い").Must().Be(TextConditions.EndsWithOneOf("い", "ん"));
      [XF] public void the_condition_shows_typed_as_its_texts_separated_by_spaces() => EndsWithOneOf.TypedTextOf(TextConditions.EndsWithOneOf("ん", "い")).Must().Be("い ん");
      [XF] public void it_made_an_ends_with_condition() => EndsWithOneOf.Made(TextConditions.EndsWithOneOf("ん")).Must().BeTrue();
      [XF] public void it_did_not_make_a_starts_with_condition() => EndsWithOneOf.Made(TextConditions.StartsWithOneOf("ん")).Must().BeFalse();
      [XF] public void texts_typed_have_no_reason_against_them() => EndsWithOneOf.WhyItMakesNoConditionFrom("ん").Must().BeNull();
      [XF] public void nothing_typed_says_a_comparison_needs_a_text() => TheReason(EndsWithOneOf.WhyItMakesNoConditionFrom(" ")).Must().Be("A condition needs at least one text to compare with.");
      [XF] public void making_one_from_nothing_typed_throws_since_the_editor_never_does() => Invoking(() => EndsWithOneOf.MadeFrom(" ")).Must().Throw<ArgumentAssertionFailedException>();
   }

   public class for_part_of_speech_is : When_typing_a_condition_into_the_tree_editor
   {
      static readonly ConditionTestWithTexts PartOfSpeechIs = TokenTest("part of speech is");

      [XF] public void levels_separated_by_spaces_are_the_levels_from_the_top() => PartOfSpeechIs.MadeFrom("名詞 接尾").Must().Be(TokenConditions.PartOfSpeechIs("名詞", "接尾"));
      [XF] public void levels_separated_by_a_middle_dot_are_too() => PartOfSpeechIs.MadeFrom("名詞・接尾").Must().Be(TokenConditions.PartOfSpeechIs("名詞", "接尾"));
      [XF] public void the_condition_shows_typed_as_its_levels_separated_by_a_middle_dot() => PartOfSpeechIs.TypedTextOf(TokenConditions.PartOfSpeechIs("名詞", "接尾")).Must().Be("名詞・接尾");
      [XF] public void a_part_of_speech_the_tokenizer_never_gives_says_so() =>
         TheReason(PartOfSpeechIs.WhyItMakesNoConditionFrom("名詞 存在しない")).Must().Be("The tokenizer gives no part of speech that is, or is a kind of, 名詞・存在しない.");

      [XF] public void making_one_the_tokenizer_never_gives_throws_since_the_editor_never_does() =>
         Invoking(() => PartOfSpeechIs.MadeFrom("存在しない")).Must().Throw<ArgumentAssertionFailedException>();
   }

   public class for_matches_the_regular_expression : When_typing_a_condition_into_the_tree_editor
   {
      static readonly ConditionTestWithTexts MatchesTheRegularExpression = TextTest("matches the regular expression");

      [XF] public void the_whole_text_is_the_pattern_spaces_included() => MatchesTheRegularExpression.MadeFrom("^a b$").Must().Be(TextConditions.MatchesRegularExpression("^a b$"));
      [XF] public void a_valid_pattern_has_no_reason_against_it() => MatchesTheRegularExpression.WhyItMakesNoConditionFrom("^a+$").Must().BeNull();
      [XF] public void an_invalid_pattern_says_why_it_is_invalid() => TheReason(MatchesTheRegularExpression.WhyItMakesNoConditionFrom("(")).Must().Contain("Not enough )'s");
      [XF] public void making_one_from_an_invalid_pattern_throws_since_the_editor_never_does() => Invoking(() => MatchesTheRegularExpression.MadeFrom("(")).Must().Throw<RegexParseException>();
   }
}
