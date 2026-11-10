using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>A match-condition describes itself as the editor would show it, which is also what a failing spec prints.</summary>
public class When_describing_a_match_condition : IAIGeneratedSpec
{
   public class with_conditions_on_subjects_found_from_the_word_nested_in_groups : When_describing_a_match_condition
   {
      readonly string _description = ConditionGroup.AllOf(
                                        PrecedingToken.Meets(TokenConditions.IsInAnIrrealisForm),
                                        ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAnIchidanVerb)),
                                        PrecedingToken.Meets(ConditionGroup.AnyOf(
                                                                TokenConditions.IsAnAdverb,
                                                                ConditionGroup.AllOf(
                                                                   TokenConditions.PartOfSpeechIs("形容詞"),
                                                                   TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("く"))))),
                                        FirstToken.Meets(TokenConditions.PartOfSpeechIs("名詞", "接尾", "人名")),
                                        FollowingText.Meets(TextConditions.StartsWithOneOf("湧", "わ")),
                                        IsASingleToken)
                                    .ToString();

      [XF] public void it_shows_one_condition_per_line_with_a_subjects_name_on_the_line_of_its_condition_and_each_groups_conditions_indented_under_it() =>
         _description.Must().Be(string.Join(Environment.NewLine,
                                            "All of:",
                                            "   The preceding token: is in an irrealis form",
                                            "   Not: The preceding token: is an ichidan verb",
                                            "   The preceding token: Any of:",
                                            "      is an adverb",
                                            "      All of:",
                                            "         part of speech is i-adjective (形容詞)",
                                            "         Surface: ends with one of く",
                                            "   The first token: part of speech is noun › suffix › person-name (名詞・接尾・人名)",
                                            "   The following text: starts with one of わ, 湧",
                                            "   is a single token"));
   }
}
