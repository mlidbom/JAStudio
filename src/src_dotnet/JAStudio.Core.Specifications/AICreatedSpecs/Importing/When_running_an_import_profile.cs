using System.IO;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

public class When_running_an_import_profile : ImportingSpecification, IAIGeneratedSpec
{
   public class on_an_Immersion_Kit_sentence_card : When_running_an_import_profile
   {
      readonly RawNote _sentenceCard;
      readonly SentenceNote _sentence;

      public on_an_Immersion_Kit_sentence_card()
      {
         _sentenceCard = AddImmersionKitSentenceCard("ずっと1人で生きていくんだ", "anime_the_garden_of_words_000000346", "garden_of_words_346.mp3", "garden_of_words_346.jpg",
                                                     english: "You've been living your whole life alone!!", reading: "ずっと 1 人[にん]で 生[い]きていくんだ");
         Importer.Prepare(ImmersionKitProfile).Execute();
         _sentence = Sentences.All().Single();
      }

      [XF] public void the_sentence_takes_its_source_question_from_the_Expression_field() => _sentence.SourceQuestion.Value.Must().Be("ずっと1人で生きていくんだ");
      [XF] public void the_sentence_takes_its_source_answer_from_the_English_field() => _sentence.SourceAnswer.Value.Must().Be("You've been living your whole life alone!!");
      [XF] public void the_sentence_takes_its_reading_from_the_Reading_field_with_its_furigana_as_it_is() => _sentence.Reading.Value.Must().Be("ずっと 1 人[にん]で 生[い]きていくんだ");
      [XF] public void the_sentence_takes_its_external_id_from_the_ID_field() => _sentence.ExternalId.Value.Must().Be("anime_the_garden_of_words_000000346");
      [XF] public void the_sentences_audio_references_the_sentence_cards_audio_file() => _sentence.Audio.RawValue().Must().Be("[sound:garden_of_words_346.mp3]");
      [XF] public void the_sentences_screenshot_references_the_sentence_cards_screenshot_file() => _sentence.Screenshot.RawValue().Must().Be("<img src=\"garden_of_words_346.jpg\">");
      [XF] public void the_sentence_is_tagged_with_the_profiles_tag() => _sentence.Tags.ToStringList().Must().Contain("source::immersion_kit");

      [XF] public void the_audio_is_stored_in_the_profiles_audio_folder_normalized_as_mp3gain_normalizes_it() =>
         File.ReadAllBytes(Path.Combine(MediaLayerFolder, AudioFolder, "garden_of_words_346.mp3")).Must().SequenceEqual(SentenceAudioNormalizedByMp3Gain);

      [XF] public void Ankis_copy_of_the_audio_is_replaced_by_the_normalized_file() =>
         File.ReadAllBytes(Path.Combine(AnkiMediaFolder, "garden_of_words_346.mp3")).Must().SequenceEqual(SentenceAudioNormalizedByMp3Gain);

      [XF] public void the_screenshot_is_stored_in_the_profiles_screenshot_folder() => File.Exists(Path.Combine(MediaLayerFolder, ScreenshotFolder, "garden_of_words_346.jpg")).Must().BeTrue();
      [XF] public void the_sentence_finds_its_audio_in_the_media_layer() => _sentence.PreferredAudio.Must().NotBeNull();
      [XF] public void the_sentence_card_is_tagged_as_imported_in_Anki() => AnkiRawNotes.IsTaggedAsImported(_sentenceCard).Must().BeTrue();
      [XF] public void a_new_run_finds_no_raw_notes() => Importer.Prepare(ImmersionKitProfile).RawNotes.Must().BeEmpty();

      public class and_then_on_a_sentence_card_with_the_same_external_id : on_an_Immersion_Kit_sentence_card
      {
         readonly ImportRun _run;

         public and_then_on_a_sentence_card_with_the_same_external_id()
         {
            AddImmersionKitSentenceCard("ずっと1人で生きていくんだ", "anime_the_garden_of_words_000000346", "other_audio.mp3", "other_screenshot.jpg");
            _run = Importer.Prepare(ImmersionKitProfile);
            _run.Execute();
         }

         [XF] public void the_run_warns_that_the_card_looks_imported_already_by_its_external_id() =>
            _run.AlreadyImported.Single().WhatMatched.Must().Contain("external id anime_the_garden_of_words_000000346 is in the corpus");

         [XF] public void no_second_sentence_is_added() => Sentences.All().Count.Must().Be(1);
         [XF] public void the_card_is_tagged_as_imported_like_the_rest() => AnkiRawNotes.IsTaggedAsImported(_run.RawNotes.Single()).Must().BeTrue();
      }

      public class and_then_on_a_sentence_card_with_the_same_audio_file : on_an_Immersion_Kit_sentence_card
      {
         readonly ImportRun _run;

         public and_then_on_a_sentence_card_with_the_same_audio_file()
         {
            AddImmersionKitSentenceCardWithoutItsMediaFiles("別の文", "anime_another_id", "[sound:garden_of_words_346.mp3]", "");
            _run = Importer.Prepare(ImmersionKitProfile);
         }

         [XF] public void the_run_warns_that_the_card_looks_imported_already_by_its_file() =>
            _run.AlreadyImported.Single().WhatMatched.Must().Contain("file garden_of_words_346.mp3 is in the media layer");

         [XF] public void the_run_can_still_run() => _run.CanExecute.Must().BeTrue();
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
