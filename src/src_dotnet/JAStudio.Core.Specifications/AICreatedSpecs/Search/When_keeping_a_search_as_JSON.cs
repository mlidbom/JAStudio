using System;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Grouping;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search;

/// <summary>A search kept as JSON, such as the workspace's between sessions, is read back as the same search.</summary>
public class When_keeping_a_search_as_JSON : IAIGeneratedSpec
{
   static NoteCondition ReadBack(NoteCondition search) =>
      NoteConditionJson.ConditionFrom(JsonNode.Parse(NoteConditionJson.Of(search).ToJsonString()) as JsonObject ?? throw new InvalidOperationException("The JSON isn't an object."));

   public class with_every_kind_of_condition : When_keeping_a_search_as_JSON
   {
      static readonly NoteCondition Search =
         ConditionGroup.AllOf(
            NoteProperties.NoteType.Is<SentenceNote>(),
            NoteProperties.Tags.Include("source::anime"),
            ConditionGroup.Not(NoteProperties.StudyingReading.Is(true)),
            ConditionGroup.AnyOf(
               NoteProperties.SearchableText.Contains("家"),
               NoteProperties.Question.Is("家に帰る"),
               NoteProperties.Answer.MatchesWildcards("go*home"),
               NoteProperties.Answer.MatchesRegex("go(ing)?")));

      [XF] public void it_is_read_back_as_the_same_search() => ReadBack(Search).Must().Be(Search);
   }

   public class with_no_conditions : When_keeping_a_search_as_JSON
   {
      [XF] public void it_is_read_back_as_an_empty_search() => ReadBack(ConditionGroup.AllOf()).IsEmpty.Must().BeTrue();
   }
}
