using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TestUtils;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// Changing the notes' Anki tags in JAStudio (<see cref="AnkiTagChanges"/>), the tags of their study-notes in Anki, which JAStudio shows below <c>anki::</c>:<br/>
/// each change is made in Anki, here the pretend Anki of the specifications, then the study-notes are read from Anki again, so the notes show it.<br/>
/// The sentence 家に帰る, tagged <c>source::anime::natsume</c>, whose study-note is tagged <c>marked</c>, <c>tmp::a</c> and <c>tmp::a::b</c> in Anki,<br/>
/// and the sentence 駅に行く, whose study-note has no tags; then the collection loaded again, as at startup.
/// </summary>
public class When_changing_the_notes_Anki_tags : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly NoteId _goingHomeId;
   readonly NoteId _goingToTheStationId;
   protected readonly JPCollection Collection;
   protected readonly PretendAnki Anki;
   protected readonly AnkiTagChanges AnkiTagChanges;

   public When_changing_the_notes_Anki_tags()
   {
      var goingHome = CreateTestSentence("家に帰る", "Go home");
      goingHome.Tags.Set(Tag.FromName("source::anime::natsume"));
      var goingToTheStation = CreateTestSentence("駅に行く", "Go to the station");
      Anki = GetService<PretendAnki>();
      Anki.Tag(goingHome, "marked", "tmp::a", "tmp::a::b");
      _goingHomeId = goingHome.GetId();
      _goingToTheStationId = goingToTheStation.GetId();

      Collection = GetService<JPCollection>();
      AnkiTagChanges = GetService<AnkiTagChanges>();
      Collection.ReloadFromBackend();
   }

   protected SentenceNote GoingHome => Collection.Sentences.WithIdOrNone(_goingHomeId) ?? throw new System.InvalidOperationException("The sentence is gone.");
   protected SentenceNote GoingToTheStation => Collection.Sentences.WithIdOrNone(_goingToTheStationId) ?? throw new System.InvalidOperationException("The sentence is gone.");

   static string[] NamesOf(System.Collections.Generic.IEnumerable<Tag> tags) => [..tags.Select(it => it.Name).Order(System.StringComparer.OrdinalIgnoreCase)];

   public class adding_anki_leech_to_both_sentences : When_changing_the_notes_Anki_tags
   {
      public adding_anki_leech_to_both_sentences() => AnkiTagChanges.AddTo([GoingHome, GoingToTheStation], Tags.Anki.For("leech"));

      [XF] public void the_study_note_of_each_has_leech_in_Anki() => (Anki.TagsOf(GoingHome).Contains("leech") && Anki.TagsOf(GoingToTheStation).Contains("leech")).Must().BeTrue();
      [XF] public void each_shows_anki_leech_among_its_Anki_tags() => (GoingHome.AnkiTags.Contains(Tags.Anki.For("leech")) && GoingToTheStation.AnkiTags.Contains(Tags.Anki.For("leech"))).Must().BeTrue();
      [XF] public void their_own_tags_are_unchanged() => (NamesOf(GoingHome.Tags).SequenceEqual(["source::anime::natsume"]) && !GoingToTheStation.Tags.Any()).Must().BeTrue();
   }

   public class removing_anki_marked_from_the_sentence_going_home : When_changing_the_notes_Anki_tags
   {
      public removing_anki_marked_from_the_sentence_going_home() => AnkiTagChanges.RemoveFrom([GoingHome], Tags.Anki.For("marked"));

      [XF] public void its_study_note_keeps_only_its_other_tags_in_Anki() => Anki.TagsOf(GoingHome).Must().SequenceEqual(["tmp::a", "tmp::a::b"]);
      [XF] public void it_shows_only_its_other_Anki_tags() => NamesOf(GoingHome.AnkiTags).Must().SequenceEqual(["anki::tmp::a", "anki::tmp::a::b"]);
   }

   public class renaming_anki_tmp_to_anki_scratch : When_changing_the_notes_Anki_tags
   {
      public renaming_anki_tmp_to_anki_scratch()
      {
         var rename = TagBranchRename.OfTheLastLevel(new TagBranch("anki::tmp"), "scratch", [..Collection.TagsInUse()]);
         AnkiTagChanges.Rename(rename);
      }

      [XF] public void the_tags_in_the_branch_are_renamed_in_Anki_and_the_others_kept() => Anki.TagsOf(GoingHome).Must().SequenceEqual(["marked", "scratch::a", "scratch::a::b"]);
      [XF] public void the_sentence_shows_the_renamed_Anki_tags() => NamesOf(GoingHome.AnkiTags).Must().SequenceEqual(["anki::marked", "anki::scratch::a", "anki::scratch::a::b"]);
   }

   public class deleting_anki_tmp : When_changing_the_notes_Anki_tags
   {
      public deleting_anki_tmp() => AnkiTagChanges.Delete(new TagBranch("anki::tmp"));

      [XF] public void the_tags_in_the_branch_are_gone_from_Anki_and_the_others_kept() => Anki.TagsOf(GoingHome).Must().SequenceEqual(["marked"]);
      [XF] public void the_sentence_shows_only_its_other_Anki_tag() => NamesOf(GoingHome.AnkiTags).Must().SequenceEqual(["anki::marked"]);
   }

   public class counting_the_Anki_notes_with_a_tag_in_anki_tmp : When_changing_the_notes_Anki_tags
   {
      [XF] public void it_is_the_one_study_note_with_one() => AnkiTagChanges.NumberOfAnkiNotesWithATagIn(new TagBranch("anki::tmp")).Must().Be(1);
   }
}
