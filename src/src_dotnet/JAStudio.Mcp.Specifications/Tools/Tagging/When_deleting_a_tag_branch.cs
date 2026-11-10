using System.Collections.Generic;
using System.Linq;
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
/// The notes: a sentence tagged with Violet Evergarden's episodes 07 and 08, <c>source::anime::violet::07</c> and <c>::08</c>, and <c>tmp</c>,<br/>
/// and a sentence tagged with episode 08 only. Their study-notes are tagged in Anki, here the pretend Anki: the first's <c>tmp::a</c> and<br/>
/// <c>marked</c>, the second's <c>tmp::b</c>.
/// </summary>
public class When_deleting_a_tag_branch : McpServerSpecification
{
   const string Episode07 = "source::anime::violet::07";
   const string Episode08 = "source::anime::violet::08";

   readonly SentenceNote _sentenceTaggedWithBothEpisodes;
   readonly SentenceNote _sentenceTaggedWithEpisode08Only;
   readonly PretendAnki _anki;

   public When_deleting_a_tag_branch()
   {
      _sentenceTaggedWithBothEpisodes = CreateTestSentence("明日学校に行く", "I'll go to school tomorrow");
      _sentenceTaggedWithBothEpisodes.Tags.Set(Tag.FromName(Episode07));
      _sentenceTaggedWithBothEpisodes.Tags.Set(Tag.FromName(Episode08));
      _sentenceTaggedWithBothEpisodes.Tags.Set(Tag.FromName("tmp"));

      _sentenceTaggedWithEpisode08Only = CreateTestSentence("家に帰る", "Go home");
      _sentenceTaggedWithEpisode08Only.Tags.Set(Tag.FromName(Episode08));

      _anki = GetService<PretendAnki>();
      _anki.Tag(_sentenceTaggedWithBothEpisodes, "tmp::a", "marked");
      _anki.Tag(_sentenceTaggedWithEpisode08Only, "tmp::b");
      GetService<JPCollection>().ReadTheStudyNotesFromAnkiAgain();
   }

   CallToolResult DeleteATagBranch(string branch) => CallTool("delete_a_tag_branch", new Dictionary<string, object?> { ["branch"] = branch });

   static bool HasATagIn(SentenceNote sentence, string branch) => sentence.Tags.Any(new TagBranch(branch).Contains);

   static bool HasAnAnkiTagIn(SentenceNote sentence, string branch) => sentence.AnkiTags.Any(new TagBranch(branch).Contains);

   public class the_branch_above_the_episodes : When_deleting_a_tag_branch
   {
      readonly DeletedTagBranch _result;

      public the_branch_above_the_episodes() => _result = ResultOf<DeletedTagBranch>(DeleteATagBranch("source::anime::violet"));

      [XF] public void no_sentence_has_an_episode_left() =>
         (HasATagIn(_sentenceTaggedWithBothEpisodes, "source::anime::violet") || HasATagIn(_sentenceTaggedWithEpisode08Only, "source::anime::violet")).Must().BeFalse();
      [XF] public void the_sentences_keep_their_other_tags() => HasATagIn(_sentenceTaggedWithBothEpisodes, "tmp").Must().BeTrue();
      [XF] public void the_study_notes_keep_their_tags_in_Anki() => _anki.TagsOf(_sentenceTaggedWithBothEpisodes).Must().SequenceEqual(["marked", "tmp::a"]);
      [XF] public void it_reports_both_sentences() => _result.NotesThatHadATagInIt.Must().Be(2);
      [XF] public void it_reports_no_Anki_notes_for_a_branch_of_the_notes_own_tags() => _result.AnkiNotesThatHadATagInIt.Must().BeNull();
   }

   public class anki_tmp : When_deleting_a_tag_branch
   {
      readonly DeletedTagBranch _result;

      public anki_tmp() => _result = ResultOf<DeletedTagBranch>(DeleteATagBranch("anki::tmp"));

      [XF] public void the_study_notes_keep_only_their_other_tags_in_Anki() =>
         (_anki.TagsOf(_sentenceTaggedWithBothEpisodes).SequenceEqual(["marked"]) && !_anki.TagsOf(_sentenceTaggedWithEpisode08Only).Any()).Must().BeTrue();
      [XF] public void the_sentences_no_longer_show_it() =>
         (HasAnAnkiTagIn(_sentenceTaggedWithBothEpisodes, "anki::tmp") || HasAnAnkiTagIn(_sentenceTaggedWithEpisode08Only, "anki::tmp")).Must().BeFalse();
      [XF] public void the_sentences_keep_their_own_tag_of_the_same_name() => HasATagIn(_sentenceTaggedWithBothEpisodes, "tmp").Must().BeTrue();
      [XF] public void it_reports_both_sentences() => _result.NotesThatHadATagInIt.Must().Be(2);
      [XF] public void it_reports_the_Anki_notes_it_was_deleted_from() => _result.AnkiNotesThatHadATagInIt.Must().Be(2);
   }

   public class the_anki_branch_itself : When_deleting_a_tag_branch
   {
      readonly CallToolResult _result;

      public the_anki_branch_itself() => _result = DeleteATagBranch("anki");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_failure_says_why() => TextOf(_result).Must().Contain(Tags.Anki.WhyTheBranchItselfIsNeverChanged);
      [XF] public void the_study_notes_keep_their_tags_in_Anki() => _anki.TagsOf(_sentenceTaggedWithEpisode08Only).Must().SequenceEqual(["tmp::b"]);
   }
}
