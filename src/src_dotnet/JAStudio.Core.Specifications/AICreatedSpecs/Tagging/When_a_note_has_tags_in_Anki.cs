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
/// A note's Anki tags: the tags its study-note has in Anki, which JAStudio shows below <c>anki::</c>, reads as the collection loads and when it reads<br/>
/// the study-notes from Anki again, never stores, and changes only in Anki (<see cref="When_changing_the_notes_Anki_tags"/>). The sentence 家に帰る, tagged <c>source::anime::natsume</c> and <c>tmp::x</c>, whose<br/>
/// study-note the student tagged <c>marked</c> and<br/>
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

   [XF] public void its_Anki_tags_are_offered_as_tags_to_add_or_rename() => Collection.TagsInUse().Any(it => it.Name == "anki::marked").Must().BeTrue();

   [XF] public void it_is_found_as_a_note_with_a_tag_in_the_branch_of_one_of_its_Anki_tags() =>
      Collection.NotesWithATagIn(new TagBranch("anki::-::priority")).Select(it => it.GetId()).Must().SequenceEqual([_sentenceId]);

   /// <summary>
   /// During a session, the student's study-note in Anki is now tagged <c>leech</c> and <c>marked</c>, as after a review that made a card a leech, and the<br/>
   /// study-notes are read from Anki again, as after that review. What else reading them again does: <see cref="When_the_study_notes_are_read_from_Anki_again"/>.
   /// </summary>
   public class and_then_its_study_note_is_tagged_in_Anki_and_the_study_notes_are_read_again : When_a_note_has_tags_in_Anki
   {
      public and_then_its_study_note_is_tagged_in_Anki_and_the_study_notes_are_read_again()
      {
         GetService<PretendAnki>().Tag(TheSentence, "leech", "marked");
         Collection.ReadTheStudyNotesFromAnkiAgain();
      }

      [XF] public void its_Anki_tags_are_the_new_ones() => TheSentence.AnkiTags.Select(it => it.Name).Order().Must().SequenceEqual(["anki::leech", "anki::marked"]);
      [XF] public void its_own_tags_are_unchanged() => TheSentence.Tags.Select(it => it.Name).Order().Must().SequenceEqual(["source::anime::natsume", "tmp::x"]);
   }

   public class what_can_be_done_to_Anki_tags_by_hand : When_a_note_has_tags_in_Anki
   {
      [XF] public void a_tag_below_anki_can_be_added_or_removed() => Tags.WhyItCannotBeATagAPersonManages("anki::marked").Must().BeNull();

      [XF] public void anki_itself_cannot_be_a_tag() => Tags.WhyItCannotBeATagAPersonManages("anki").Must().NotBeNull().Be(Tags.Anki.WhyTheBranchItselfIsNeverChanged);

      [XF] public void the_anki_branch_itself_cannot_be_renamed() => Tags.Anki.Branch.WhyItCannotBeRenamedByHand(Collection.TagsInUse()).Must().NotBeNull().Be(Tags.Anki.WhyTheBranchItselfIsNeverChanged);

      [XF] public void the_anki_branch_itself_cannot_be_deleted() => Tags.Anki.Branch.WhyItCannotBeDeletedByHand(Collection.TagsInUse()).Must().NotBeNull().Be(Tags.Anki.WhyTheBranchItselfIsNeverChanged);

      [XF] public void a_branch_below_anki_can_be_deleted() => new TagBranch("anki::marked").WhyItCannotBeDeletedByHand(Collection.TagsInUse()).Must().BeNull();

      [XF] public void a_branch_below_anki_can_be_renamed_within_it() =>
         TagBranchRename.OfTheLastLevel(new TagBranch("anki::marked"), "flagged", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().BeNull();

      [XF] public void a_branch_below_anki_cannot_be_renamed_out_of_it() =>
         TagBranchRename.WithItsParents(new TagBranch("anki::marked"), "marked", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().NotBeNull().Be(Tags.Anki.WhyNoTagMovesIntoOrOutOfIt);

      [XF] public void a_branch_below_anki_cannot_be_renamed_to_anki_itself() =>
         TagBranchRename.WithItsParents(new TagBranch("anki::marked"), "anki", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().NotBeNull().Be(Tags.Anki.WhyTheBranchItselfIsNeverChanged);

      [XF] public void a_branch_cannot_be_renamed_into_the_anki_branch() =>
         TagBranchRename.WithItsParents(new TagBranch("tmp"), "anki::tmp", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().NotBeNull().Be(Tags.Anki.WhyNoTagMovesIntoOrOutOfIt);

      [XF] public void a_top_level_branch_cannot_be_renamed_to_anki() =>
         TagBranchRename.OfTheLastLevel(new TagBranch("tmp"), "anki", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().NotBeNull().Be(Tags.Anki.WhyNoTagMovesIntoOrOutOfIt);

      [XF] public void a_level_named_anki_below_another_level_is_a_tag_like_any_other() =>
         TagBranchRename.OfTheLastLevel(new TagBranch("tmp::x"), "anki", [..Collection.TagsInUse()]).WhyItCannotBeDone.Must().BeNull();

      [XF] public void the_priority_tags_branch_cannot_be_renamed_since_JAStudio_finds_the_priorities_by_its_name() =>
         Tags.PriorityBranch.WhyItCannotBeRenamedByHand(Collection.TagsInUse()).Must().NotBeNull().Contain("priorities");

      [XF] public void the_branch_above_the_priority_tags_branch_cannot_be_renamed_either() =>
         new TagBranch("anki::-").WhyItCannotBeRenamedByHand(Collection.TagsInUse()).Must().NotBeNull().Contain("priorities");

      [XF] public void a_priority_tag_can_be_renamed() => new TagBranch("anki::-::priority::high").WhyItCannotBeRenamedByHand(Collection.TagsInUse()).Must().BeNull();
   }
}
