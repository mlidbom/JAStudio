using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TestUtils;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// A note's Anki tags: the tags its study-note has in Anki, which JAStudio shows below <c>anki::</c>, reads as the collection loads, and never stores<br/>
/// or changes. The sentence 家に帰る, tagged <c>source::anime::natsume</c> and <c>tmp::x</c>, whose study-note the student tagged <c>marked</c> and<br/>
/// <c>-::priority::high::5</c> in Anki, and the sentence 駅に行く, with no tags in either; then the collection loaded again, as at startup.
/// </summary>
public class When_a_note_has_tags_in_Anki : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly NoteId _sentenceId;
   protected readonly JPCollection Collection;

   public When_a_note_has_tags_in_Anki()
   {
      var sentence = CreateTestSentence("家に帰る", "Go home");
      sentence.Tags.Set(Tag.FromName("source::anime::natsume"));
      sentence.Tags.Set(Tag.FromName("tmp::x"));
      CreateTestSentence("駅に行く", "Go to the station");
      GetService<PretendAnki>().Tag(sentence, "marked", "-::priority::high::5");
      _sentenceId = sentence.GetId();

      Collection = GetService<JPCollection>();
      Collection.ReloadFromBackend();
   }

   SentenceNote TheSentence => Collection.Sentences.WithIdOrNone(_sentenceId) ?? throw new System.InvalidOperationException("The sentence is gone.");

   [XF] public void its_Anki_tags_are_its_study_notes_tags_below_anki() =>
      TheSentence.AnkiTags.Select(it => it.Name).Order().Must().SequenceEqual(["anki::-::priority::high::5", "anki::marked"]);

   [XF] public void its_own_tags_are_only_those_of_the_corpus() => TheSentence.Tags.Select(it => it.Name).Order().Must().SequenceEqual(["source::anime::natsume", "tmp::x"]);

   [XF] public void the_corpus_stores_only_its_own_tags() => TheSentence.ToDto().Tags.Order().Must().SequenceEqual(["source::anime::natsume", "tmp::x"]);

   [XF] public void all_its_tags_are_its_own_and_its_Anki_tags() => TheSentence.AllTags.Count().Must().Be(4);

   [XF] public void a_search_for_an_Anki_tag_finds_only_it() =>
      Collection.NotesMatching(NoteProperties.Tags.Include("anki::marked")).Select(it => it.GetId()).Must().SequenceEqual([_sentenceId]);

   [XF] public void the_tag_tree_has_the_anki_branch_with_it() => (Collection.TagTree().NodeOf(Tags.Anki.Branch) ?? throw new System.InvalidOperationException("The tag tree has no anki branch.")).NumberOfNotesInTheBranch.Must().Be(1);

   [XF] public void the_tag_paths_offered_for_a_search_include_its_Anki_tags() => Collection.TagPathsInUse().All.Must().Contain("anki::marked");

   [XF] public void its_priority_is_read_from_its_Anki_priority_tag() => TheSentence.PriorityTagValue().Must().Be(5);

   [XF] public void its_Anki_tags_are_not_offered_as_tags_to_add_or_rename() => Collection.TagsInUse().Any(it => it.Name == "anki::marked").Must().BeFalse();

   public class read_only : When_a_note_has_tags_in_Anki
   {
      [XF] public void a_tag_below_anki_cannot_be_added_or_removed_by_hand() => Tags.WhyItCannotBeATagAPersonManages("anki::marked").Must().NotBeNull().Be(Tags.Anki.WhyItIsReadOnly);

      [XF] public void the_anki_branch_cannot_be_renamed_by_hand() => Tags.Anki.Branch.WhyItCannotBeRenamedByHand(Collection.TagsInUse()).Must().NotBeNull().Be(Tags.Anki.WhyItIsReadOnly);

      [XF] public void a_branch_below_anki_cannot_be_deleted_by_hand() =>
         new TagBranch("anki::marked").WhyItCannotBeDeletedByHand(Collection.TagsInUse()).Must().NotBeNull().Be(Tags.Anki.WhyItIsReadOnly);

      [XF] public void a_branch_cannot_be_renamed_into_the_anki_branch() =>
         TagBranchRename.WithItsParents(new TagBranch("tmp"), "anki::tmp", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().NotBeNull().Be(Tags.Anki.WhyItIsReadOnly);

      [XF] public void a_top_level_branch_cannot_be_renamed_to_anki() =>
         TagBranchRename.OfTheLastLevel(new TagBranch("tmp"), "anki", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().NotBeNull().Be(Tags.Anki.WhyItIsReadOnly);

      [XF] public void a_level_named_anki_below_another_level_is_a_tag_like_any_other() =>
         TagBranchRename.OfTheLastLevel(new TagBranch("tmp::x"), "anki", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().BeNull();
   }
}
