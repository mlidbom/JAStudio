using System.IO;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

public class When_running_an_import_profile : ImportingSpecification, IAIGeneratedSpec
{
   public class on_an_Immersion_Kit_sentence_card : When_running_an_import_profile
   {
      protected readonly RawNote SentenceCard;
      protected readonly ImportRun FirstRun;
      protected readonly SentenceNote Sentence;

      public on_an_Immersion_Kit_sentence_card()
      {
         SentenceCard = AddImmersionKitSentenceCard("ずっと1人で生きていくんだ", "anime_the_garden_of_words_000000346", "garden_of_words_346.mp3", "garden_of_words_346.jpg",
                                                    english: "You've been living your whole life alone!!", reading: "ずっと 1 人[にん]で 生[い]きていくんだ");
         FirstRun = Importer.Prepare(ImmersionKitProfile);
         FirstRun.Execute();
         Sentence = Sentences.All().Single();
      }

      [XF] public void the_sentence_takes_its_source_question_from_the_Expression_field() => Sentence.SourceQuestion.Value.Must().Be("ずっと1人で生きていくんだ");
      [XF] public void the_sentence_takes_its_source_answer_from_the_English_field() => Sentence.SourceAnswer.Value.Must().Be("You've been living your whole life alone!!");
      [XF] public void the_sentence_takes_its_reading_from_the_Reading_field_with_its_furigana_as_it_is() => Sentence.Reading.Value.Must().Be("ずっと 1 人[にん]で 生[い]きていくんだ");
      [XF] public void the_sentence_takes_its_external_id_from_the_ID_field() => Sentence.ExternalId.Value.Must().Be("anime_the_garden_of_words_000000346");
      [XF] public void the_sentences_audio_references_the_sentence_cards_audio_file() => Sentence.Audio.RawValue().Must().Be("[sound:garden_of_words_346.mp3]");
      [XF] public void the_sentences_screenshot_references_the_sentence_cards_screenshot_file() => Sentence.Screenshot.RawValue().Must().Be("<img src=\"garden_of_words_346.jpg\">");
      [XF] public void the_sentence_is_tagged_with_the_profiles_tag() => Sentence.Tags.ToStringList().Must().Contain("source::immersion_kit");

      [XF] public void the_audio_is_stored_in_the_profiles_audio_folder_normalized_as_mp3gain_normalizes_it() =>
         File.ReadAllBytes(Path.Combine(MediaLayerFolder, AudioFolder, "garden_of_words_346.mp3")).Must().SequenceEqual(SentenceAudioNormalizedByMp3Gain);

      [XF] public void Ankis_copy_of_the_audio_is_replaced_by_the_normalized_file() =>
         File.ReadAllBytes(Path.Combine(AnkiMediaFolder, "garden_of_words_346.mp3")).Must().SequenceEqual(SentenceAudioNormalizedByMp3Gain);

      [XF] public void the_screenshot_is_stored_in_the_profiles_screenshot_folder() => File.Exists(Path.Combine(MediaLayerFolder, ScreenshotFolder, "garden_of_words_346.jpg")).Must().BeTrue();
      [XF] public void the_sentence_finds_its_audio_in_the_media_layer() => Sentence.PreferredAudio.Must().NotBeNull();
      [XF] public void the_sentence_card_is_tagged_as_imported_in_Anki() => AnkiRawNotes.IsTaggedAsImported(SentenceCard).Must().BeTrue();
      [XF] public void the_run_lists_the_sentence_as_imported() => FirstRun.ImportedSentences.Must().SequenceEqual([Sentence]);

      public class and_then_checked_again : on_an_Immersion_Kit_sentence_card
      {
         readonly ImportRun _secondRun;
         public and_then_checked_again() => _secondRun = Importer.Prepare(ImmersionKitProfile);

         [XF] public void the_check_finds_the_sentence_card() => _secondRun.CheckedRawNotes.Count.Must().Be(1);
         [XF] public void the_sentence_card_was_imported_by_an_earlier_run() => CheckOf(_secondRun, SentenceCard).Outcome.Must().Be(RawNoteOutcome.ImportedByAnEarlierRun);
         [XF] public void a_finding_names_the_sentence_with_its_external_id() => AFindingSays(CheckOf(_secondRun, SentenceCard), "A sentence in the corpus has the external id anime_the_garden_of_words_000000346: the sentence \"ずっと1人で生きていくんだ\"").Must().BeTrue();
         [XF] public void there_is_nothing_to_import() => _secondRun.HasNotesToImportAndNoProblems.Must().BeFalse();
      }

      public class and_then_the_sentence_is_deleted : on_an_Immersion_Kit_sentence_card
      {
         public and_then_the_sentence_is_deleted() => GetService<JPCollection>().Delete(Sentence);

         public class and_checked_again : and_then_the_sentence_is_deleted
         {
            readonly CheckedRawNote _check;
            public and_checked_again() => _check = CheckOf(Importer.Prepare(ImmersionKitProfile), SentenceCard);

            [XF] public void the_sentence_card_was_still_imported_by_an_earlier_run() => _check.Outcome.Must().Be(RawNoteOutcome.ImportedByAnEarlierRun);
            [XF] public void a_finding_says_no_sentence_has_its_external_id_any_more() => AFindingSays(_check, "No sentence in the corpus has the external id anime_the_garden_of_words_000000346 any more").Must().BeTrue();
            [XF] public void a_finding_says_no_note_uses_its_audio_file() => AFindingSays(_check, "The file garden_of_words_346.mp3 is in the media layer, but no note uses it").Must().BeTrue();
         }

         public class and_the_sentence_cards_imported_tag_is_removed_in_Anki : and_then_the_sentence_is_deleted
         {
            readonly ImportRun _run;

            public and_the_sentence_cards_imported_tag_is_removed_in_Anki()
            {
               AnkiRawNotes.RemoveTheImportedTag(SentenceCard);
               _run = Importer.Prepare(ImmersionKitProfile);
            }

            [XF] public void the_sentence_card_will_be_imported() => CheckOf(_run, SentenceCard).Outcome.Must().Be(RawNoteOutcome.WillBeImported);
            [XF] public void a_finding_says_the_import_reuses_its_audio_file() => AFindingSays(CheckOf(_run, SentenceCard), "The file garden_of_words_346.mp3 is in the media layer, but no note uses it: the import reuses it").Must().BeTrue();

            public class and_run : and_the_sentence_cards_imported_tag_is_removed_in_Anki
            {
               public and_run() => _run.Execute();

               [XF] public void the_sentence_is_back() => Sentences.All().Single().ExternalId.Value.Must().Be("anime_the_garden_of_words_000000346");
               [XF] public void it_finds_its_audio_in_the_media_layer() => Sentences.All().Single().PreferredAudio.Must().NotBeNull();
            }
         }

         public class and_another_sentence_card_has_a_different_screenshot_with_the_same_name : and_then_the_sentence_is_deleted
         {
            readonly CheckedRawNote _check;

            public and_another_sentence_card_has_a_different_screenshot_with_the_same_name()
            {
               var otherCard = AddImmersionKitSentenceCardWithoutItsMediaFiles("別の文", "anime_another_id", "", "<img src=\"garden_of_words_346.jpg\">");
               File.WriteAllBytes(Path.Combine(AnkiMediaFolder, "garden_of_words_346.jpg"), [0x01, 0x02]);
               _check = CheckOf(Importer.Prepare(ImmersionKitProfile), otherCard);
            }

            [XF] public void the_card_has_a_problem() => _check.Outcome.Must().Be(RawNoteOutcome.Problem);
            [XF] public void a_finding_says_a_different_file_by_that_name_is_in_the_media_layer() => AFindingSays(_check, "A different file named garden_of_words_346.jpg is in the media layer, though no note uses it").Must().BeTrue();
         }
      }

      public class and_then_on_a_sentence_card_with_the_same_external_id : on_an_Immersion_Kit_sentence_card
      {
         readonly RawNote _duplicate;
         readonly ImportRun _run;

         public and_then_on_a_sentence_card_with_the_same_external_id()
         {
            _duplicate = AddImmersionKitSentenceCard("ずっと1人で生きていくんだ", "anime_the_garden_of_words_000000346", "other_audio.mp3", "other_screenshot.jpg");
            _run = Importer.Prepare(ImmersionKitProfile);
         }

         [XF] public void the_card_is_skipped_as_already_in_JAStudio() => CheckOf(_run, _duplicate).Outcome.Must().Be(RawNoteOutcome.AlreadyInJAStudio);
         [XF] public void a_finding_names_the_sentence_with_its_external_id() => AFindingSays(CheckOf(_run, _duplicate), "A sentence in the corpus has the external id anime_the_garden_of_words_000000346: the sentence \"ずっと1人で生きていくんだ\"").Must().BeTrue();
         [XF] public void there_is_nothing_to_import() => _run.HasNotesToImportAndNoProblems.Must().BeFalse();
         [XF] public void the_card_is_not_tagged_as_imported() => AnkiRawNotes.IsTaggedAsImported(_duplicate).Must().BeFalse();
      }

      public class and_then_on_a_sentence_card_with_the_same_audio_file : on_an_Immersion_Kit_sentence_card
      {
         readonly RawNote _duplicate;
         readonly ImportRun _run;

         public and_then_on_a_sentence_card_with_the_same_audio_file()
         {
            _duplicate = AddImmersionKitSentenceCardWithoutItsMediaFiles("別の文", "anime_another_id", "[sound:garden_of_words_346.mp3]", "");
            _run = Importer.Prepare(ImmersionKitProfile);
         }

         [XF] public void the_card_is_skipped_as_already_in_JAStudio() => CheckOf(_run, _duplicate).Outcome.Must().Be(RawNoteOutcome.AlreadyInJAStudio);

         [XF] public void a_finding_names_the_sentence_using_the_file() =>
            AFindingSays(CheckOf(_run, _duplicate), "The file garden_of_words_346.mp3 is in the media layer, used by the sentence \"ずっと1人で生きていくんだ\"").Must().BeTrue();

         [XF] public void nothing_stops_the_run() => _run.CanExecute.Must().BeTrue();
      }
   }

   public class with_a_profile_that_deletes_the_raw_notes_afterwards : When_running_an_import_profile
   {
      readonly RawNote _sentenceCard;

      public with_a_profile_that_deletes_the_raw_notes_afterwards()
      {
         _sentenceCard = AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.mp3", "anime_1.jpg");
         ImmersionKitProfile.DeletesTheRawNotesAfterwards = true;
         Importer.Prepare(ImmersionKitProfile).Execute();
      }

      [XF] public void the_sentence_is_imported() => Sentences.All().Single().SourceQuestion.Value.Must().Be("文です");
      [XF] public void the_sentence_card_is_deleted_from_Anki() => AnkiRawNotes.Contains(_sentenceCard).Must().BeFalse();
   }

   public class after_the_profile_is_edited_once_the_run_was_prepared : When_running_an_import_profile
   {
      public after_the_profile_is_edited_once_the_run_was_prepared()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.mp3", "anime_1.jpg");
         var run = Importer.Prepare(ImmersionKitProfile);
         ImmersionKitProfile.TagsToAdd = ["source::something_else"];
         run.Execute();
      }

      [XF] public void the_run_does_what_it_was_prepared_to_do() => Sentences.All().Single().Tags.ToStringList().Must().Contain("source::immersion_kit");
   }
}
