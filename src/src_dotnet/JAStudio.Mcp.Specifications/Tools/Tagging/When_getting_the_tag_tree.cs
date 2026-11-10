using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;
using JAStudio.Mcp.Tools.Collection;
using JAStudio.Mcp.Tools.Tagging;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Tagging;

/// <summary>
/// The notes:
/// <list type="bullet">
/// <item>a sentence tagged <c>source::anime::natsume::s3::01</c>;</item>
/// <item>a sentence tagged <c>source::anime::natsume::s3::02</c> and <c>-::priority::low</c>;</item>
/// <item>a vocab tagged <c>source::wani::level::01</c>, with its reading studied;</item>
/// <item>a kanji tagged <c>source::wani::level::01</c>;</item>
/// <item>a vocab with no tags.</item>
/// </list>
/// </summary>
public class When_getting_the_tag_tree : McpServerSpecification
{
   public When_getting_the_tag_tree()
   {
      CreateTestSentence("明日学校に行く", "I'll go to school tomorrow").Tags.Set(Tag.FromName("source::anime::natsume::s3::01"));

      var sentenceAboutHome = CreateTestSentence("家に帰る", "Go home");
      sentenceAboutHome.Tags.Set(Tag.FromName("source::anime::natsume::s3::02"));
      sentenceAboutHome.Tags.Set(Tag.FromName("-::priority::low"));

      var studiedVocab = CreateVocab("行く", "to go", "いく");
      studiedVocab.Tags.Set(Tag.FromName("source::wani::level::01"));
      studiedVocab.SetStudyingStatus(new CardStudyingStatus(0, CardTypes.Reading, isSuspended: false, NoteTypes.Vocab));

      CreateKanji("行", "go", "コウ", "い").Tags.Set(Tag.FromName("source::wani::level::01"));
      CreateVocab("来る", "to come", "くる");
   }

   protected CallToolResult GetTheTagTree(Dictionary<string, object?> arguments) => CallTool("get_the_tag_tree", arguments);

   protected static TagTreeNodeDescription TheChildWithThePath(string tagPath, TagTreeNodeDescription node) => ChildrenOf(node).Single(it => it.TagPath == tagPath);

   protected static IReadOnlyList<TagTreeNodeDescription> ChildrenOf(TagTreeNodeDescription node) =>
      node.Children ?? throw new InvalidOperationException($"{node.TagPath} lists no children.");

   public class of_the_branch_source_listing_one_level_below_it : When_getting_the_tag_tree
   {
      readonly TagTreeNodeDescription _source;

      public of_the_branch_source_listing_one_level_below_it() =>
         _source = ResultOf<List<TagTreeNodeDescription>>(GetTheTagTree(new Dictionary<string, object?> { ["branch"] = "source", ["levelsListed"] = 1 })).Single();

      TagTreeNodeDescription Anime => TheChildWithThePath("source::anime", _source);

      [XF] public void it_is_the_node_of_source() => _source.TagPath.Must().Be("source");
      [XF] public void it_counts_the_two_sentences_none_studied() => _source.Sentences.Must().Be(new NoteCounts(Notes: 2, Studying: 0));
      [XF] public void it_counts_the_one_tagged_vocab_studied() => _source.Vocab.Must().Be(new NoteCounts(Notes: 1, Studying: 1));
      [XF] public void it_counts_the_one_kanji() => _source.Kanji.Must().Be(new NoteCounts(Notes: 1, Studying: 0));
      [XF] public void it_lists_anime_and_wani_below_it() => ChildrenOf(_source).Select(it => it.TagPath).Must().SequenceEqual(["source::anime", "source::wani"]);
      [XF] public void anime_leaves_out_vocab_and_kanji_since_none_are_in_its_branch() => (Anime.Vocab is null && Anime.Kanji is null).Must().BeTrue();
      [XF] public void anime_lists_no_children_since_they_are_below_the_level_listed() => Anime.Children.Must().BeNull();
      [XF] public void anime_says_four_paths_are_below_it_natsume_s3_01_and_02() => Anime.PathsBelowNotListed.Must().Be(4);
   }

   public class of_the_whole_tree_listing_three_levels_below_the_top_level : When_getting_the_tag_tree
   {
      readonly List<TagTreeNodeDescription> _topLevel;

      public of_the_whole_tree_listing_three_levels_below_the_top_level() =>
         _topLevel = ResultOf<List<TagTreeNodeDescription>>(GetTheTagTree(new Dictionary<string, object?> { ["levelsListed"] = 3 }));

      TagTreeNodeDescription Season3 =>
         TheChildWithThePath("source::anime::natsume::s3", TheChildWithThePath("source::anime::natsume", TheChildWithThePath("source::anime", _topLevel.Single(it => it.TagPath == "source"))));

      [XF] public void the_top_level_is_the_priority_tags_top_level_and_source() => _topLevel.Select(it => it.TagPath).Must().SequenceEqual(["-", "source"]);
      [XF] public void season_3_three_levels_below_source_is_listed_with_its_two_sentences() => Season3.Sentences.Must().Be(new NoteCounts(Notes: 2, Studying: 0));
      [XF] public void season_3_says_two_paths_are_below_it() => Season3.PathsBelowNotListed.Must().Be(2);
   }

   public class of_a_branch_no_note_has_a_tag_in : When_getting_the_tag_tree
   {
      readonly CallToolResult _result;

      public of_a_branch_no_note_has_a_tag_in() => _result = GetTheTagTree(new Dictionary<string, object?> { ["branch"] = "source::tv" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_failure_names_the_branch() => TextOf(_result).Must().Contain("source::tv");
   }
}
