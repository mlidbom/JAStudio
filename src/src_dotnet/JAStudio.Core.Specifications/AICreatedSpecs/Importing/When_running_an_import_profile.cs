using System.IO;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Media;
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

      protected string StoredAudioPath => Path.Combine(MediaLayerFolder, AudioFolder, "garden_of_words_346.mp3");
      protected string StoredScreenshotPath => Path.Combine(MediaLayerFolder, ScreenshotFolder, "garden_of_words_346.jpg");

      public on_an_Immersion_Kit_sentence_card()
      {
         SentenceCard = AddImmersionKitSentenceCard("ずっと1人で生きていくんだ", "anime_the_garden_of_words_000000346", "garden_of_words_346.mp3", "garden_of_words_346.jpg",
                                                    english: "You've been living your whole life alone!!", reading: "ずっと 1 人[にん]で 生[い]きていくんだ");
         FirstRun = Importer.Prepare(ImmersionKitProfile);
         FirstRun.ExecuteUnlessOutOfDate();
         Sentence = Sentences.All().Single();
      }

      [XF] public void the_sentence_takes_its_source_question_from_the_Expression_field() => Sentence.SourceData.Question.Value.Must().Be("ずっと1人で生きていくんだ");
      [XF] public void the_sentence_takes_its_source_answer_from_the_English_field() => Sentence.SourceData.Answer.Value.Must().Be("You've been living your whole life alone!!");
      [XF] public void the_sentence_takes_its_reading_from_the_Reading_field_with_its_furigana_as_it_is() => Sentence.Reading.Value.Must().Be("ずっと 1 人[にん]で 生[い]きていくんだ");
      [XF] public void the_sentence_takes_its_external_id_from_the_ID_field() => Sentence.SourceData.ExternalId.Value.Must().Be("anime_the_garden_of_words_000000346");
      [XF] public void the_sentences_source_audio_is_the_sentence_cards_audio_file() => Sentence.SourceData.Audio.All.Must().SequenceEqual([new AudioReference("garden_of_words_346.mp3")]);
      [XF] public void the_sentences_source_screenshots_are_the_sentence_cards_screenshot_file() => Sentence.SourceData.Screenshots.All.Must().SequenceEqual([new ImageReference("garden_of_words_346.jpg")]);
      [XF] public void the_sentence_has_no_media_of_the_corpuss_own() => (Sentence.Audio.All.Count, Sentence.Screenshots.All.Count).Must().Be((0, 0));
      [XF] public void the_sentence_is_tagged_with_the_profiles_tag() => Sentence.Tags.ToStringList().Must().Contain("source::immersion_kit");

      [XF] public void the_audio_is_stored_in_the_profiles_audio_folder_normalized_as_mp3gain_normalizes_it() =>
         File.ReadAllBytes(StoredAudioPath).Must().SequenceEqual(SentenceAudioNormalizedByMp3Gain);

      [XF] public void Ankis_copy_of_the_audio_is_replaced_by_the_normalized_file() =>
         File.ReadAllBytes(Path.Combine(AnkiMediaFolder, "garden_of_words_346.mp3")).Must().SequenceEqual(SentenceAudioNormalizedByMp3Gain);

      [XF] public void the_screenshot_is_stored_in_the_profiles_screenshot_folder() => StoredScreenshotPath.Must().Satisfy(File.Exists);
      [XF] public void the_sentence_finds_its_audio_in_the_media_layer() => Sentence.PreferredAudio.Must().NotBeNull();
      [XF] public void the_sentence_card_is_tagged_as_imported_in_Anki() => AnkiRawNotes.IsTaggedAsImported(SentenceCard).Must().BeTrue();
      [XF] public void the_run_lists_the_sentence_as_imported() => FirstRun.ImportedSentences.Must().SequenceEqual([Sentence]);

      public class and_then_checked_again : on_an_Immersion_Kit_sentence_card
      {
         readonly ImportRun _secondRun;
         public and_then_checked_again() => _secondRun = Importer.Prepare(ImmersionKitProfile);

         [XF] public void the_check_finds_the_sentence_card() => _secondRun.CheckedRawNotes.Count.Must().Be(1);
         [XF] public void the_sentence_card_was_imported_by_an_earlier_run() => CheckOf(_secondRun, SentenceCard).Outcome.Must().Be(RawNoteOutcome.ImportedByAnEarlierRun);
         [XF] public void its_external_id_is_fine() => CheckOf(_secondRun, SentenceCard).ExternalId.Mark.Must().Be(CheckMark.Fine);
         [XF] public void the_finding_of_its_external_id_names_the_sentence() => CheckOf(_secondRun, SentenceCard).ExternalId.Finding.Must().Contain("the sentence \"ずっと1人で生きていくんだ\"");
         [XF] public void its_audio_is_fine() => CheckOf(_secondRun, SentenceCard).Audio.Mark.Must().Be(CheckMark.Fine);
         [XF] public void nothing_is_wrong() => CheckOf(_secondRun, SentenceCard).WhatIsWrong.Must().BeEmpty();
         [XF] public void there_is_nothing_to_import() => _secondRun.HasNotesToImportAndNoProblems.Must().BeFalse();
      }

      public class and_then_the_sentence_is_deleted : on_an_Immersion_Kit_sentence_card
      {
         public and_then_the_sentence_is_deleted() => GetService<JPCollection>().Delete(Sentence, ItsAnkiNote.IsDeleted);

         [XF] public void its_audio_is_deleted_from_the_media_layer_with_it() => StoredAudioPath.Must().Satisfy(it => !File.Exists(it));
         [XF] public void its_screenshot_is_deleted_from_the_media_layer_with_it() => StoredScreenshotPath.Must().Satisfy(it => !File.Exists(it));

         public class and_checked_again : and_then_the_sentence_is_deleted
         {
            readonly CheckedRawNote _check;
            public and_checked_again() => _check = CheckOf(Importer.Prepare(ImmersionKitProfile), SentenceCard);

            [XF] public void the_sentence_card_is_a_problem_since_nothing_of_it_is_left_in_JAStudio() => _check.Outcome.Must().Be(RawNoteOutcome.Problem);
            [XF] public void whats_wrong_says_to_remove_its_imported_tag_or_delete_it() => _check.WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("Imported by an earlier run, but no longer in JAStudio: remove its jastudio::imported tag in Anki to import it again, or delete it")));
            [XF] public void whats_wrong_says_it_once_for_the_note() => _check.WhatIsWrong.Count.Must().Be(1);
            [XF] public void its_external_id_is_marked_as_a_problem() => _check.ExternalId.Mark.Must().Be(CheckMark.Problem);
            [XF] public void the_finding_of_its_external_id_says_no_sentence_has_it_any_more() => _check.ExternalId.Finding.Must().Contain("No sentence in the corpus has the external id anime_the_garden_of_words_000000346 any more");
            [XF] public void the_finding_of_its_audio_says_the_file_is_not_in_the_media_layer() => _check.Audio.Finding.Must().Contain("The file garden_of_words_346.mp3 isn't in the media layer");
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
            [XF] public void the_finding_of_its_audio_says_the_file_will_be_stored_again() => CheckOf(_run, SentenceCard).Audio.Finding.Must().Contain($"The file garden_of_words_346.mp3 will be stored in {AudioFolder}");

            public class and_run : and_the_sentence_cards_imported_tag_is_removed_in_Anki
            {
               public and_run() => _run.ExecuteUnlessOutOfDate();

               [XF] public void the_sentence_is_back() => Sentences.All().Single().SourceData.ExternalId.Value.Must().Be("anime_the_garden_of_words_000000346");
               [XF] public void it_finds_its_audio_in_the_media_layer() => Sentences.All().Single().PreferredAudio.Must().NotBeNull();
            }
         }
      }

      /// <summary>The sentence's audio and screenshot are removed from it, which leaves both files in the media layer, used by no note.</summary>
      public class and_then_the_sentence_stops_using_its_media_files : on_an_Immersion_Kit_sentence_card
      {
         public and_then_the_sentence_stops_using_its_media_files()
         {
            Sentence.SourceData.Audio.Remove(Sentence.SourceData.Audio.All.Single());
            Sentence.SourceData.Screenshots.Remove(Sentence.SourceData.Screenshots.All.Single());
         }

         public class and_another_sentence_card_has_the_same_audio_file : and_then_the_sentence_stops_using_its_media_files
         {
            readonly CheckedRawNote _check;

            public and_another_sentence_card_has_the_same_audio_file()
            {
               var otherCard = AddImmersionKitSentenceCard("別の文", "anime_another_id", "garden_of_words_346.mp3", "another_screenshot.jpg");
               _check = CheckOf(Importer.Prepare(ImmersionKitProfile), otherCard);
            }

            [XF] public void the_card_will_be_imported() => _check.Outcome.Must().Be(RawNoteOutcome.WillBeImported);
            [XF] public void its_audio_is_fine() => _check.Audio.Mark.Must().Be(CheckMark.Fine);
            [XF] public void the_finding_of_its_audio_says_the_import_reuses_the_file() => _check.Audio.Finding.Must().Contain("The file garden_of_words_346.mp3 is in the media layer, but no note uses it: the import reuses it");
         }

         public class and_another_sentence_card_has_a_different_screenshot_with_the_same_name : and_then_the_sentence_stops_using_its_media_files
         {
            readonly CheckedRawNote _check;

            public and_another_sentence_card_has_a_different_screenshot_with_the_same_name()
            {
               var otherCard = AddImmersionKitSentenceCardWithoutItsMediaFiles("別の文", "anime_another_id", "", "<img src=\"garden_of_words_346.jpg\">");
               File.WriteAllBytes(Path.Combine(AnkiMediaFolder, "garden_of_words_346.jpg"), [0x01, 0x02]);
               _check = CheckOf(Importer.Prepare(ImmersionKitProfile), otherCard);
            }

            [XF] public void the_card_has_a_problem() => _check.Outcome.Must().Be(RawNoteOutcome.Problem);
            [XF] public void its_screenshot_is_marked_as_a_problem() => _check.Screenshot.Mark.Must().Be(CheckMark.Problem);
            [XF] public void whats_wrong_says_a_different_file_by_that_name_is_in_the_media_layer() => _check.WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("A different file named garden_of_words_346.jpg is in the media layer")));
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
         [XF] public void its_external_id_is_marked_as_already_in_JAStudio() => CheckOf(_run, _duplicate).ExternalId.Mark.Must().Be(CheckMark.AlreadyInJAStudio);
         [XF] public void whats_wrong_names_the_sentence_with_its_external_id() => CheckOf(_run, _duplicate).WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("External id taken by the sentence \"ずっと1人で生きていくんだ\"")));
         [XF] public void its_audio_is_not_checked() => CheckOf(_run, _duplicate).Audio.Mark.Must().Be(CheckMark.Neutral);
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

         [XF] public void its_audio_is_marked_as_already_in_JAStudio() => CheckOf(_run, _duplicate).Audio.Mark.Must().Be(CheckMark.AlreadyInJAStudio);
         [XF] public void whats_wrong_names_the_sentence_using_the_file() => CheckOf(_run, _duplicate).WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("Audio used by the sentence \"ずっと1人で生きていくんだ\"")));

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
         Importer.Prepare(ImmersionKitProfile).ExecuteUnlessOutOfDate();
      }

      [XF] public void the_sentence_is_imported() => Sentences.All().Single().SourceData.Question.Value.Must().Be("文です");
      [XF] public void the_sentence_card_is_deleted_from_Anki() => AnkiRawNotes.Must().Satisfy(it => !it.Contains(_sentenceCard));
   }

   public class after_the_profile_is_edited_once_the_run_was_prepared : When_running_an_import_profile
   {
      public after_the_profile_is_edited_once_the_run_was_prepared()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.mp3", "anime_1.jpg");
         var run = Importer.Prepare(ImmersionKitProfile);
         ImmersionKitProfile.TagsToAdd = ["source::something_else"];
         run.ExecuteUnlessOutOfDate();
      }

      [XF] public void the_run_does_what_it_was_prepared_to_do() => Sentences.All().Single().Tags.ToStringList().Must().Contain("source::immersion_kit");
   }
}
