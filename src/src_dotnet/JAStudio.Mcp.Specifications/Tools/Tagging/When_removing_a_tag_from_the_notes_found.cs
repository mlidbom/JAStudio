using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Mcp.Tools.Tagging;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Tagging;

/// <summary>
/// The notes: a sentence tagged with both Violet Evergarden's episode 07 and its episode 08, <c>source::anime::violet::07</c> and <c>::08</c>,<br/>
/// and a sentence tagged with episode 08 only.
/// </summary>
public class When_removing_a_tag_from_the_notes_found : McpServerSpecification
{
   protected const string Episode07 = "source::anime::violet::07";
   protected const string Episode08 = "source::anime::violet::08";

   protected readonly SentenceNote _sentenceTaggedWithBothEpisodes;
   protected readonly SentenceNote _sentenceTaggedWithEpisode08Only;

   public When_removing_a_tag_from_the_notes_found()
   {
      _sentenceTaggedWithBothEpisodes = CreateTestSentence("明日学校に行く", "I'll go to school tomorrow");
      _sentenceTaggedWithBothEpisodes.Tags.Set(Tag.FromName(Episode07));
      _sentenceTaggedWithBothEpisodes.Tags.Set(Tag.FromName(Episode08));

      _sentenceTaggedWithEpisode08Only = CreateTestSentence("家に帰る", "Go home");
      _sentenceTaggedWithEpisode08Only.Tags.Set(Tag.FromName(Episode08));
   }

   protected CallToolResult RemoveATagFromTheNotesFound(JsonObject search, string tag) =>
      CallTool("remove_a_tag_from_the_notes_found", new Dictionary<string, object?> { ["search"] = search, ["tag"] = tag });

   protected static bool HasTheTag(SentenceNote sentence, string tag) => sentence.Tags.Contains(Tag.FromName(tag));

   public class episode_08_from_the_notes_tagged_with_both_episodes : When_removing_a_tag_from_the_notes_found
   {
      readonly NotesTheTagWasRemovedFrom _result;

      public episode_08_from_the_notes_tagged_with_both_episodes() =>
         _result = ResultOf<NotesTheTagWasRemovedFrom>(RemoveATagFromTheNotesFound(
                                                          new JsonObject { ["allOf"] = new JsonArray(new JsonObject { ["tagsInclude"] = Episode07 }, new JsonObject { ["tagsInclude"] = Episode08 }) },
                                                          Episode08));

      [XF] public void the_sentence_tagged_with_both_no_longer_has_episode_08() => HasTheTag(_sentenceTaggedWithBothEpisodes, Episode08).Must().BeFalse();
      [XF] public void the_sentence_tagged_with_both_keeps_episode_07() => HasTheTag(_sentenceTaggedWithBothEpisodes, Episode07).Must().BeTrue();
      [XF] public void the_sentence_the_search_did_not_find_keeps_episode_08() => HasTheTag(_sentenceTaggedWithEpisode08Only, Episode08).Must().BeTrue();
      [XF] public void it_reports_one_note_changed() => _result.NotesChanged.Must().Be(1);
      [XF] public void it_lists_the_sentence_it_changed() => _result.Notes.Select(it => it.Question).Must().SequenceEqual(["明日学校に行く"]);
   }

   public class the_path_above_the_episodes_from_every_note : When_removing_a_tag_from_the_notes_found
   {
      readonly NotesTheTagWasRemovedFrom _result;

      public the_path_above_the_episodes_from_every_note() =>
         _result = ResultOf<NotesTheTagWasRemovedFrom>(RemoveATagFromTheNotesFound(new JsonObject { ["allOf"] = new JsonArray() }, "source::anime::violet"));

      [XF] public void it_reports_no_note_changed_since_no_note_has_that_tag_itself() => _result.NotesChanged.Must().Be(0);
      [XF] public void the_tags_below_it_stay() => HasTheTag(_sentenceTaggedWithEpisode08Only, Episode08).Must().BeTrue();
   }

   public class a_tag_JAStudio_defines : When_removing_a_tag_from_the_notes_found
   {
      readonly CallToolResult _result;

      public a_tag_JAStudio_defines()
      {
         _sentenceTaggedWithEpisode08Only.Tags.Set(Tags.TTSAudio);
         _result = RemoveATagFromTheNotesFound(new JsonObject { ["allOf"] = new JsonArray() }, Tags.TTSAudio.Name);
      }

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_failure_names_the_tag() => TextOf(_result).Must().Contain(Tags.TTSAudio.Name);
      [XF] public void the_note_keeps_the_tag() => _sentenceTaggedWithEpisode08Only.Tags.Contains(Tags.TTSAudio).Must().BeTrue();
   }
}
