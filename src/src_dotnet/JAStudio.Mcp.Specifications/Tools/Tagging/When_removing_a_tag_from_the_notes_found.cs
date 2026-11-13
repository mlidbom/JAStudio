using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TestUtils;
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
      _sentenceTaggedWithBothEpisodes = WithAStudyNote(CreateTestSentence("明日学校に行く", "I'll go to school tomorrow"));
      _sentenceTaggedWithBothEpisodes.Tags.Set(Tag.FromName(Episode07));
      _sentenceTaggedWithBothEpisodes.Tags.Set(Tag.FromName(Episode08));

      _sentenceTaggedWithEpisode08Only = WithAStudyNote(CreateTestSentence("家に帰る", "Go home"));
      _sentenceTaggedWithEpisode08Only.Tags.Set(Tag.FromName(Episode08));
   }

   protected CallToolResult RemoveATagFromTheNotesFound(JsonObject search, string tag) =>
      CallTool("remove_a_tag_from_the_notes_found", new Dictionary<string, object?> { ["search"] = search, ["tag"] = tag });

   protected static string[] TagNamesOf(SentenceNote sentence) => [.. sentence.Tags.Select(it => it.Name)];

   public class episode_08_from_the_notes_tagged_with_both_episodes : When_removing_a_tag_from_the_notes_found
   {
      readonly NotesTheTagChangedOn _result;

      public episode_08_from_the_notes_tagged_with_both_episodes() =>
         _result = ResultOf<NotesTheTagChangedOn>(RemoveATagFromTheNotesFound(
                                                          new JsonObject { ["allOf"] = new JsonArray(new JsonObject { ["tagsInclude"] = Episode07 }, new JsonObject { ["tagsInclude"] = Episode08 }) },
                                                          Episode08));

      [XF] public void the_sentence_tagged_with_both_no_longer_has_episode_08() => TagNamesOf(_sentenceTaggedWithBothEpisodes).Must().Satisfy(it => !it.Contains(Episode08));
      [XF] public void the_sentence_tagged_with_both_keeps_episode_07() => TagNamesOf(_sentenceTaggedWithBothEpisodes).Must().Contain(Episode07);
      [XF] public void the_sentence_the_search_did_not_find_keeps_episode_08() => TagNamesOf(_sentenceTaggedWithEpisode08Only).Must().Contain(Episode08);
      [XF] public void it_reports_one_note_changed() => _result.NotesChanged.Must().Be(1);
      [XF] public void it_lists_the_sentence_it_changed() => _result.Notes.Select(it => it.Question).Must().SequenceEqual(["明日学校に行く"]);
   }

   public class the_path_above_the_episodes_from_every_note : When_removing_a_tag_from_the_notes_found
   {
      readonly NotesTheTagChangedOn _result;

      public the_path_above_the_episodes_from_every_note() =>
         _result = ResultOf<NotesTheTagChangedOn>(RemoveATagFromTheNotesFound(new JsonObject { ["allOf"] = new JsonArray() }, "source::anime::violet"));

      [XF] public void it_reports_no_note_changed_since_no_note_has_that_tag_itself() => _result.NotesChanged.Must().Be(0);
      [XF] public void the_tags_below_it_stay() => TagNamesOf(_sentenceTaggedWithEpisode08Only).Must().Contain(Episode08);
   }

   public class a_tag_JAStudio_defines : When_removing_a_tag_from_the_notes_found
   {
      readonly CallToolResult _result;

      public a_tag_JAStudio_defines()
      {
         _sentenceTaggedWithEpisode08Only.Tags.Set(Tags.Sentence.Uses.HiddenMatches);
         _result = RemoveATagFromTheNotesFound(new JsonObject { ["allOf"] = new JsonArray() }, Tags.Sentence.Uses.HiddenMatches.Name);
      }

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_failure_names_the_tag() => TextOf(_result).Must().Contain(Tags.Sentence.Uses.HiddenMatches.Name);
      [XF] public void the_note_keeps_the_tag() => _sentenceTaggedWithEpisode08Only.Tags.Must().Contain(Tags.Sentence.Uses.HiddenMatches);
   }

   /// <summary>A tag below <c>anki::</c>, one of the notes' tags in Anki: the study-note of each sentence is tagged <c>marked</c> in Anki, here the pretend Anki.</summary>
   public class anki_marked_from_the_sentences_tagged_with_episode_08 : When_removing_a_tag_from_the_notes_found
   {
      readonly NotesTheTagChangedOn _result;

      public anki_marked_from_the_sentences_tagged_with_episode_08()
      {
         GetService<PretendAnki>().Tag(_sentenceTaggedWithBothEpisodes, "marked");
         GetService<PretendAnki>().Tag(_sentenceTaggedWithEpisode08Only, "marked");
         GetService<JPCollection>().ReadTheStudyNotesFromAnkiAgain();
         _result = ResultOf<NotesTheTagChangedOn>(RemoveATagFromTheNotesFound(new JsonObject { ["tagsInclude"] = Episode08 }, "anki::marked"));
      }

      [XF] public void the_study_notes_no_longer_have_it_in_Anki() =>
         (GetService<PretendAnki>().TagsOf(_sentenceTaggedWithBothEpisodes).Count + GetService<PretendAnki>().TagsOf(_sentenceTaggedWithEpisode08Only).Count).Must().Be(0);
      [XF] public void the_sentences_no_longer_show_it() => new { BothEpisodes = _sentenceTaggedWithBothEpisodes.AnkiTags, Episode08Only = _sentenceTaggedWithEpisode08Only.AnkiTags }.Must().Satisfy(it => !it.BothEpisodes.Any() && !it.Episode08Only.Any());
      [XF] public void it_reports_both_sentences_changed() => _result.NotesChanged.Must().Be(2);
   }
}
