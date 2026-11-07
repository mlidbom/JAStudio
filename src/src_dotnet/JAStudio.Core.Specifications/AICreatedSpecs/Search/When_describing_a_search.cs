using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Grouping;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search;

/// <summary>A search describes itself as the search editor shows it, which is also what a failing spec prints.</summary>
public class When_describing_a_search : IAIGeneratedSpec
{
   public class with_groups_nested_in_groups : When_describing_a_search
   {
      readonly string _description = ConditionGroup.AllOf(
                                        NoteProperties.Tags.Include("source::anime::natsume"),
                                        ConditionGroup.Not(NoteProperties.Tags.Include("-::priority::low")),
                                        ConditionGroup.AnyOf(
                                           NoteProperties.Question.Contains("行く"),
                                           NoteProperties.Answer.MatchesRegex("go(ing)?")),
                                        NoteProperties.StudyingReading.Is(false))
                                    .ToString();

      [XF] public void it_shows_one_condition_per_line_with_each_groups_conditions_indented_under_it_and_a_negation_on_the_line_of_what_it_negates() =>
         _description.Must().Be(string.Join(Environment.NewLine,
                                            "All of:",
                                            "   Tags include source::anime::natsume",
                                            "   Not: Tags include -::priority::low",
                                            "   Any of:",
                                            "      Question contains \"行く\"",
                                            "      Answer matches the regular expression \"go(ing)?\"",
                                            "   Studying reading is false"));
   }
}
