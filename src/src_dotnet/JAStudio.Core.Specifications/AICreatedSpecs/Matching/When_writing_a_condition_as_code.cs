using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping.ConditionGroup;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// A condition is written as code (<see cref="ConditionCode"/>), as the Matching tab's tree editor, the failure reasons and a failing specification show<br/>
/// it: the dots going from the word to what's tested, <c>!</c> for a negation, <c>AllOf(...)</c> and <c>AnyOf(...)</c> with their conditions on lines<br/>
/// of their own, texts as string literals, and the tokenizer's values in Japanese with their English as a comment.
/// </summary>
public class When_writing_a_condition_as_code : IAIGeneratedSpec
{
   public class a_vocabs_match_condition : When_writing_a_condition_as_code
   {
      /// <summary>Written in C# as its code reads (<see cref="TokenConditionsInCode"/>), so the C# and the code it's written as can be read side by side.</summary>
      static readonly AllOfGroup<CandidateWordVariant> MatchCondition =
         AllOf(
            PrecedingToken.IsInAnIrrealisForm,
            !PrecedingToken.IsAnIchidanVerb,
            PrecedingToken.Surface.EndsWithOneOf("ん"),
            AnyOf(
               !Surface.In("本の"),
               FollowingToken.PartOfSpeech.Is("名詞", "数")),
            !LastToken.AllOf(
               TokenConditions.PartOfSpeech.Is("動詞"),
               TokenConditions.InflectedForm.In("連用形", "連用タ接続")),
            FollowingText.MatchesRegularExpression("^よ\"$"),
            IsASingleToken);

      readonly string _code = ConditionCode.AsText(ConditionCode.LinesOfAMatchCondition(MatchCondition));

      [XF] public void is_a_comment_heading_then_each_condition_a_statement_with_each_groups_conditions_indented_and_several_values_one_to_a_line_in_ordinal_order_with_their_english() =>
         _code.Must().Be(string.Join(Environment.NewLine,
                                     "// A word matches the vocab when each of these holds:",
                                     "PrecedingToken.IsInAnIrrealisForm",
                                     "!PrecedingToken.IsAnIchidanVerb",
                                     "PrecedingToken.Surface.EndsWithOneOf(\"ん\")",
                                     "AnyOf(",
                                     "   !Surface.In(\"本の\"),",
                                     "   FollowingToken.PartOfSpeech.Is(名詞・数))   // noun › numeric",
                                     "!LastToken.AllOf(",
                                     "   PartOfSpeech.Is(動詞),   // verb",
                                     "   InflectedForm.In(",
                                     "      連用タ接続,   // Continuative ta-connection",
                                     "      連用形))   // Continuative/masu-stem verbs/adjective",
                                     "FollowingText.MatchesRegularExpression(@\"^よ\"\"$\")",
                                     "IsASingleToken"));
   }

   public class a_condition_on_one_line : When_writing_a_condition_as_code
   {
      static readonly Condition<CandidateWordVariant> Condition =
         ConditionGroup.AnyOf(
            ConditionGroup.Not(Surface.Meets(TextConditions.In("本の"))),
            FollowingToken.Meets(TokenConditions.InflectedForm.In("連用形", "連用タ接続")));

      [XF] public void joins_its_lines_without_their_comments_with_a_space_after_each_comma() =>
         ConditionCode.OnOneLine(Condition).Must().Be("AnyOf(!Surface.In(\"本の\"), FollowingToken.InflectedForm.In(連用タ接続, 連用形))");

      [XF] public void is_the_text_the_condition_is() => Condition.ToString().Must().Be(ConditionCode.OnOneLine(Condition));
   }

   public class an_empty_group : When_writing_a_condition_as_code
   {
      [XF] public void opens_and_closes_on_its_line() => FollowingToken.Meets(ConditionGroup.AnyOf<IAnalysisToken>()).ToString().Must().Be("FollowingToken.AnyOf()");
   }

   public class a_text_with_a_quote_and_a_backslash : When_writing_a_condition_as_code
   {
      [XF] public void is_a_string_literal_with_both_escaped() => Surface.Meets(TextConditions.In("a\"b\\c")).ToString().Must().Be("Surface.In(\"a\\\"b\\\\c\")");
   }

   public class an_inflection_type_family : When_writing_a_condition_as_code
   {
      [XF] public void says_in_english_that_it_is_every_type_of_the_family() =>
         ConditionCode.AsText(ConditionCode.LinesOf(TokenConditions.InflectionType.In("五段"))).Must().Be("InflectionType.In(五段)   // every godan type");
   }
}
