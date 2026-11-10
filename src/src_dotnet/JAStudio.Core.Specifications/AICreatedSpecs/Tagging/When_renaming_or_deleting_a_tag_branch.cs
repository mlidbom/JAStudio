using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// Renaming a branch of the tag tree, its last level or its whole path, and deleting one, on every note with a tag in it. The notes:
/// <list type="bullet">
/// <item>a sentence tagged <c>source::anime</c>, <c>source::anime::natsume::s3::01</c> and <c>-::priority::low</c>;</item>
/// <item>a sentence tagged <c>source::anime::natsume::s3::02</c>;</item>
/// <item>a vocab tagged <c>source::anime::tv::s3::01</c>;</item>
/// <item>a kanji tagged with a tag JAStudio defines and maintains, below <c>-</c>, and <c>to-be-moved::</c> followed by that tag's last level.</item>
/// </list>
/// </summary>
public class When_renaming_or_deleting_a_tag_branch : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   const string Natsume = "source::anime::natsume";
   static readonly TagBranch TheBranchOfATagJAStudioDefines = new(Tags.Kanji.InAnyVocabForm.Name);

   readonly JPCollection _collection;
   readonly JPNote _sentenceAboutSchool;
   readonly JPNote _sentenceAboutHome;
   readonly JPNote _vocab;

   public When_renaming_or_deleting_a_tag_branch()
   {
      _collection = GetService<JPCollection>();
      _sentenceAboutSchool = Tagged(CreateTestSentence("明日学校に行く", "I'll go to school tomorrow"), "source::anime", $"{Natsume}::s3::01", "-::priority::low");
      _sentenceAboutHome = Tagged(CreateTestSentence("家に帰る", "Go home"), $"{Natsume}::s3::02");
      _vocab = Tagged(CreateVocab("行く", "to go", "いく"), "source::anime::tv::s3::01");
      Tagged(CreateKanji("行", "go", "コウ", "い"), Tags.Kanji.InAnyVocabForm.Name, $"to-be-moved::{TheBranchOfATagJAStudioDefines.LastLevel}");
   }

   static JPNote Tagged(JPNote note, params string[] tagNames)
   {
      foreach(var tagName in tagNames)
         note.Tags.Set(Tag.FromName(tagName));
      return note;
   }

   static List<string> TagsOf(JPNote note) => [..note.Tags.Select(it => it.Name).Order(StringComparer.Ordinal)];

   IReadOnlyList<Tag> TagsInUse() => _collection.TagsInUse();

   TagBranchRename RenamingTheLastLevel(string branchPath, string typedLastLevel) => TagBranchRename.OfTheLastLevel(new TagBranch(branchPath), typedLastLevel, [..TagsInUse()]);

   TagBranchRename RenamingWithItsParents(string branchPath, string typedPath) => TagBranchRename.WithItsParents(new TagBranch(branchPath), typedPath, [..TagsInUse()]);

   void Apply(TagBranchRename rename) => _collection.NotesWithATagIn(rename.Branch).ForEach(rename.ApplyTo);

   static string WhyItCannotBeDone(TagBranchRename rename) => rename.WhyItCannotBeDone ?? throw new InvalidOperationException($"Renaming {rename.Branch.Path} to {rename.Renamed.Path} can be done.");

   public class renaming_the_last_level_of_source_anime_to_tv : When_renaming_or_deleting_a_tag_branch
   {
      readonly TagBranchRename _rename;

      public renaming_the_last_level_of_source_anime_to_tv() => _rename = RenamingTheLastLevel("source::anime", "tv");

      [XF] public void the_branch_becomes_source_tv() => _rename.Renamed.Path.Must().Be("source::tv");
      [XF] public void it_can_be_done() => _rename.WhyItCannotBeDone.Must().BeNull();
      [XF] public void it_merges_with_no_tag_in_use() => _rename.TagsInUseItMergesWith.Must().BeEmpty();
      [XF] public void it_changes_the_notes_with_a_tag_in_the_branch() => _collection.NotesWithATagIn(_rename.Branch).Count.Must().Be(3);

      public class after_applying_it_to_those_notes : renaming_the_last_level_of_source_anime_to_tv
      {
         public after_applying_it_to_those_notes() => Apply(_rename);

         [XF] public void every_tag_in_the_branch_is_renamed_and_the_other_tags_stay() =>
            TagsOf(_sentenceAboutSchool).Must().SequenceEqual(["-::priority::low", "source::tv", "source::tv::natsume::s3::01"]);
         [XF] public void a_note_with_one_tag_in_the_branch_has_it_renamed() => TagsOf(_sentenceAboutHome).Must().SequenceEqual(["source::tv::natsume::s3::02"]);
         [XF] public void no_note_is_left_with_a_tag_in_the_branch() => _collection.NotesWithATagIn(new TagBranch("source::anime")).Must().BeEmpty();
      }
   }

   public class renaming_the_last_level_of_natsume_to_tv_where_a_tag_in_use_has_the_renamed_name : When_renaming_or_deleting_a_tag_branch
   {
      readonly TagBranchRename _rename;

      public renaming_the_last_level_of_natsume_to_tv_where_a_tag_in_use_has_the_renamed_name() => _rename = RenamingTheLastLevel(Natsume, "tv");

      [XF] public void it_can_be_done() => _rename.WhyItCannotBeDone.Must().BeNull();
      [XF] public void it_merges_with_that_tag() => _rename.TagsInUseItMergesWith.Must().SequenceEqual(["source::anime::tv::s3::01"]);

      public class after_applying_it : renaming_the_last_level_of_natsume_to_tv_where_a_tag_in_use_has_the_renamed_name
      {
         public after_applying_it() => Apply(_rename);

         [XF] public void the_notes_of_both_have_the_same_tag() => _sentenceAboutSchool.Tags.Contains(_vocab.Tags.Single()).Must().BeTrue();
      }
   }

   public class renaming_the_last_level_to : When_renaming_or_deleting_a_tag_branch
   {
      string WhyRenamingNatsumeTo(string typedLastLevel) => WhyItCannotBeDone(RenamingTheLastLevel(Natsume, typedLastLevel));

      [XF] public void a_level_with_a_level_separator_cannot_be_done() => WhyRenamingNatsumeTo("tv::natsume").Must().Be("A level can't contain ::. Rename with parents moves a tag below other tags.");
      [XF] public void a_level_with_a_space_cannot_be_done() => WhyRenamingNatsumeTo("natsume s3").Must().Be("A tag can't contain spaces.");
      [XF] public void nothing_cannot_be_done() => WhyRenamingNatsumeTo("").Must().Be("A tag needs a name.");
      [XF] public void the_same_name_cannot_be_done() => WhyRenamingNatsumeTo("natsume").Must().Be("Type the new name.");
      [XF] public void the_same_name_in_another_case_cannot_be_done() =>
         WhyRenamingNatsumeTo("Natsume").Must().Be("Only the case differs, and tags ignore case, so the tag can't be renamed this way.");
   }

   public class renaming_with_its_parents : When_renaming_or_deleting_a_tag_branch
   {
      public class to_another_branch : renaming_with_its_parents
      {
         public to_another_branch() => Apply(RenamingWithItsParents(Natsume, "drama::natsume"));

         [XF] public void the_tags_move_there() => TagsOf(_sentenceAboutHome).Must().SequenceEqual(["drama::natsume::s3::02"]);
         [XF] public void the_tags_outside_the_branch_stay() => TagsOf(_sentenceAboutSchool).Must().SequenceEqual(["-::priority::low", "drama::natsume::s3::01", "source::anime"]);
      }

      [XF] public void to_a_path_below_itself_cannot_be_done() =>
         WhyItCannotBeDone(RenamingWithItsParents(Natsume, $"{Natsume}::old")).Must().Be("A tag can't be moved below itself.");
      [XF] public void to_a_path_with_an_empty_level_cannot_be_done() =>
         WhyItCannotBeDone(RenamingWithItsParents(Natsume, "source::::natsume")).Must().Be("A tag can't have an empty level, as source::::natsume has.");
      [XF] public void so_that_a_tag_becomes_one_jastudio_defines_cannot_be_done() =>
         WhyItCannotBeDone(RenamingWithItsParents("to-be-moved", TheBranchOfATagJAStudioDefines.PathsAboveIt[^1]))
            .Must().Be($"to-be-moved::{TheBranchOfATagJAStudioDefines.LastLevel} would become {Tags.Kanji.InAnyVocabForm.Name}, which JAStudio defines.");
   }

   [XF] public void renaming_a_branch_holding_a_tag_jastudio_defines_cannot_be_done() =>
      WhyItCannotBeDone(RenamingTheLastLevel("-", "dash")).Must().Be($"- holds {Tags.Kanji.InAnyVocabForm.Name}, whose name JAStudio uses, so it can't be renamed by hand.");

   [XF] public void deleting_a_branch_holding_a_tag_jastudio_maintains_cannot_be_done() =>
      new TagBranch("-").WhyItCannotBeDeletedByHand(TagsInUse()).Must().NotBeNull().Be($"- holds {Tags.Kanji.InAnyVocabForm.Name}, which JAStudio maintains, so it can't be deleted by hand.");

   [XF] public void a_branch_holding_no_tag_jastudio_defines_can_be_renamed() => new TagBranch("-::priority").WhyItCannotBeRenamedByHand(TagsInUse()).Must().BeNull();
   [XF] public void a_branch_holding_no_tag_jastudio_defines_can_be_deleted() => new TagBranch("-::priority").WhyItCannotBeDeletedByHand(TagsInUse()).Must().BeNull();

   public class the_source_branch : When_renaming_or_deleting_a_tag_branch
   {
      const string WhySourceIsNeverChanged = "source holds every note's source, so it's never renamed or deleted.";

      [XF] public void itself_cannot_be_renamed() => WhyItCannotBeDone(RenamingTheLastLevel("source", "origin")).Must().Be(WhySourceIsNeverChanged);
      [XF] public void itself_cannot_be_deleted() => new TagBranch("source").WhyItCannotBeDeletedByHand(TagsInUse()).Must().NotBeNull().Be(WhySourceIsNeverChanged);
      [XF] public void a_branch_in_it_can_be_moved_out_of_it() => RenamingWithItsParents("source::anime", "media::anime").WhyItCannotBeDone.Must().BeNull();
      [XF] public void a_branch_can_be_moved_into_it() => RenamingWithItsParents("-::priority", "source::priority").WhyItCannotBeDone.Must().BeNull();
      [XF] public void a_branch_in_it_is_in_it() => new TagBranch("source::anime").IsInTheSourceBranch.Must().BeTrue();
      [XF] public void itself_is_in_it() => new TagBranch("source").IsInTheSourceBranch.Must().BeTrue();
      [XF] public void a_branch_outside_it_is_not_in_it() => new TagBranch("-::priority").IsInTheSourceBranch.Must().BeFalse();

      public class with_a_vocab_JAStudio_created_from_the_dictionary : the_source_branch
      {
         public with_a_vocab_JAStudio_created_from_the_dictionary() => Tagged(CreateVocab("来る", "to come", "くる"), Tags.Source.Jamdict.Name);

         [XF] public void source_jamdict_whose_name_JAStudio_uses_cannot_be_renamed() =>
            WhyItCannotBeDone(RenamingTheLastLevel(Tags.Source.Jamdict.Name, "jmdict")).Must().Be($"{Tags.Source.Jamdict.Name} holds {Tags.Source.Jamdict.Name}, whose name JAStudio uses, so it can't be renamed by hand.");

         [XF] public void source_jamdict_which_JAStudio_does_not_maintain_can_be_deleted() =>
            new TagBranch(Tags.Source.Jamdict.Name).WhyItCannotBeDeletedByHand(TagsInUse()).Must().BeNull();
      }
   }

   public class after_deleting_natsume_from_the_notes_with_a_tag_in_it : When_renaming_or_deleting_a_tag_branch
   {
      public after_deleting_natsume_from_the_notes_with_a_tag_in_it()
      {
         var natsume = new TagBranch(Natsume);
         _collection.NotesWithATagIn(natsume).ForEach(it => it.Tags.RemoveTheBranch(natsume));
      }

      [XF] public void every_tag_in_the_branch_is_gone_and_the_tags_above_and_beside_it_stay() => TagsOf(_sentenceAboutSchool).Must().SequenceEqual(["-::priority::low", "source::anime"]);
      [XF] public void a_note_with_only_tags_in_the_branch_has_none_left() => TagsOf(_sentenceAboutHome).Must().BeEmpty();
      [XF] public void the_notes_outside_the_branch_keep_their_tags() => TagsOf(_vocab).Must().SequenceEqual(["source::anime::tv::s3::01"]);
   }
}
