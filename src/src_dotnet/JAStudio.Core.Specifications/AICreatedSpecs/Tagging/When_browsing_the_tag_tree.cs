using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// The tag tree, computed from the notes' tags: each path with the paths one level below it, and the number of notes in its branch,<br/>
/// and the tree a filter box shows for typed parts of a path. The notes:
/// <list type="bullet">
/// <item>a sentence tagged <c>source::anime::natsume::s3::01</c>;</item>
/// <item>a sentence tagged <c>source::anime::natsume::s3::02</c> and <c>-::priority::low</c>;</item>
/// <item>a sentence tagged both <c>source::anime::natsume::s10::01</c> and <c>source::anime::natsume::s10::02</c>;</item>
/// <item>a vocab tagged <c>source::wani</c>;</item>
/// <item>a kanji tagged <c>Source::kanken</c>, whose top level differs from the others' only in case;</item>
/// <item>a kanji with no tags.</item>
/// </list>
/// </summary>
public class When_browsing_the_tag_tree : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected When_browsing_the_tag_tree()
   {
      CreateTestSentence("明日学校に行く", "I'll go to school tomorrow").Tags.Set(Tag.FromName("source::anime::natsume::s3::01"));

      var sentenceAboutHome = CreateTestSentence("家に帰る", "Go home");
      sentenceAboutHome.Tags.Set(Tag.FromName("source::anime::natsume::s3::02"));
      sentenceAboutHome.Tags.Set(Tag.FromName("-::priority::low"));

      var sentenceWithTwoTagsInOneBranch = CreateTestSentence("駅に行く", "Go to the station");
      sentenceWithTwoTagsInOneBranch.Tags.Set(Tag.FromName("source::anime::natsume::s10::01"));
      sentenceWithTwoTagsInOneBranch.Tags.Set(Tag.FromName("source::anime::natsume::s10::02"));

      CreateVocab("行く", "to go", "いく").Tags.Set(Tag.FromName("source::wani"));
      CreateKanji("行", "go", "コウ", "い").Tags.Set(Tag.FromName("Source::kanken"));
      CreateKanji("駅", "station", "エキ", "");
   }

   static TagTreeNode NodeAt(IReadOnlyList<TagTreeNode> topLevel, params string[] levels) =>
      levels.Skip(1).Aggregate(TheOneNamed(levels[0], topLevel), (node, level) => TheOneNamed(level, node.Children));

   static TagTreeNode TheOneNamed(string level, IReadOnlyList<TagTreeNode> nodes) =>
      nodes.SingleOrDefault(it => it.LastLevel.Equals(level, StringComparison.OrdinalIgnoreCase)) ?? throw new InvalidOperationException($"No node is named {level}.");

   static List<string> LastLevelsOf(IReadOnlyList<TagTreeNode> nodes) => [..nodes.Select(it => it.LastLevel.ToLowerInvariant())];

   public class as_computed_from_the_notes : When_browsing_the_tag_tree
   {
      readonly TagTree _tree;

      public as_computed_from_the_notes() => _tree = GetService<JPCollection>().TagTree();

      TagTreeNode NodeAt(params string[] levels) => NodeAt(_tree.TopLevel, levels);

      [XF] public void the_top_level_holds_each_first_level_once_even_when_spelled_in_another_case() =>
         LastLevelsOf(_tree.TopLevel).Order(StringComparer.Ordinal).Must().SequenceEqual(["-", "source"]);

      [XF] public void a_node_holds_the_paths_one_level_below_it() => LastLevelsOf(NodeAt("source").Children).Must().SequenceEqual(["anime", "kanken", "wani"]);
      [XF] public void a_node_holds_its_whole_path() => NodeAt("source", "anime", "natsume").TagPath.Must().Be("source::anime::natsume");
      [XF] public void the_numbers_in_the_levels_are_ordered_as_numbers() => LastLevelsOf(NodeAt("source", "anime", "natsume").Children).Must().SequenceEqual(["s3", "s10"]);

      [XF] public void a_tag_counts_its_notes() => NodeAt("source", "anime", "natsume", "s3", "01").NumberOfNotesInTheBranch.Must().Be(1);
      [XF] public void a_parent_counts_the_notes_in_its_whole_branch() => NodeAt("source", "anime", "natsume").NumberOfNotesInTheBranch.Must().Be(3);
      [XF] public void a_note_with_two_tags_in_a_branch_counts_once() => NodeAt("source", "anime", "natsume", "s10").NumberOfNotesInTheBranch.Must().Be(1);
      [XF] public void the_top_level_counts_the_notes_in_every_spelling_of_it_and_not_the_notes_without_tags() => NodeAt("source").NumberOfNotesInTheBranch.Must().Be(5);

      public class filtered_by_typed_parts_of_a_path : as_computed_from_the_notes
      {
         TagTree Filtered(string typedParts) => _tree.WithOnlyThePathsMatching(new TypedTagPathParts(typedParts));

         [XF] public void only_the_matching_paths_and_their_parents_are_left() => LastLevelsOf(NodeAt(Filtered("s10").TopLevel, "source", "anime", "natsume").Children).Must().SequenceEqual(["s10"]);
         [XF] public void a_matching_path_keeps_everything_below_it() => LastLevelsOf(NodeAt(Filtered("s10").TopLevel, "source", "anime", "natsume", "s10").Children).Must().SequenceEqual(["01", "02"]);
         [XF] public void a_parent_left_keeps_the_number_of_notes_in_its_whole_branch() => NodeAt(Filtered("s10").TopLevel, "source").NumberOfNotesInTheBranch.Must().Be(5);
         [XF] public void a_top_level_with_no_matching_path_is_gone() => LastLevelsOf(Filtered("s10").TopLevel).Must().SequenceEqual(["source"]);
         [XF] public void every_part_must_be_in_a_path() => LastLevelsOf(NodeAt(Filtered("nat 02").TopLevel, "source", "anime", "natsume", "s10").Children).Must().SequenceEqual(["02"]);
         [XF] public void with_nothing_typed_every_path_is_left() => LastLevelsOf(Filtered(" ").TopLevel).Order(StringComparer.Ordinal).Must().SequenceEqual(["-", "source"]);
         [XF] public void with_no_matching_path_nothing_is_left() => Filtered("no-such-tag").TopLevel.Must().BeEmpty();
      }
   }
}
