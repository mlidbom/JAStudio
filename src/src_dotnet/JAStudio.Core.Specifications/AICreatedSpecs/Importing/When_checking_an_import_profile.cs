using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

/// <summary>Checking prepares an <see cref="ImportRun"/> without changing anything, and any problem it finds stops the whole run.</summary>
public class When_checking_an_import_profile : ImportingSpecification, IAIGeneratedSpec
{
   ImportRun Check() => Importer.Prepare(ImmersionKitProfile);


   public class that_has_no_Anki_note_type : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public that_has_no_Anki_note_type()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.mp3", "anime_1.jpg");
         ImmersionKitProfile.AnkiNoteType = "";
         _run = Check();
      }

      [XF] public void a_problem_with_the_profile_asks_for_the_note_type() => _run.ProblemsWithTheProfile.Must().Satisfy(problems => problems.Any(it => it.Description.Contains("Choose the Anki note type")));
      [XF] public void no_raw_notes_are_selected() => _run.CheckedRawNotes.Must().BeEmpty();
      [XF] public void it_cannot_run() => _run.CanExecute.Must().BeFalse();
   }

   public class that_takes_the_raw_notes_of_one_of_JAStudios_own_note_types : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public that_takes_the_raw_notes_of_one_of_JAStudios_own_note_types()
      {
         ImmersionKitProfile.AnkiNoteType = NoteTypes.Sentence;
         _run = Check();
      }

      [XF] public void a_problem_with_the_profile_says_JAStudios_own_note_types_cannot_be_imported_from() => _run.ProblemsWithTheProfile.Must().Satisfy(problems => problems.Any(it => it.Description.Contains("JAStudio's own note types can't be imported from")));
   }

   public class whose_source_question_is_unmapped : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public whose_source_question_is_unmapped()
      {
         ImmersionKitProfile.FieldMapping.SourceQuestion = "";
         _run = Check();
      }

      [XF] public void a_problem_with_the_profile_asks_for_the_source_question_to_be_mapped() => _run.ProblemsWithTheProfile.Must().Satisfy(problems => problems.Any(it => it.Description.Contains("Map an Anki field to the sentence's source question")));
   }

   public class that_maps_a_field_the_note_type_does_not_have : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public that_maps_a_field_the_note_type_does_not_have()
      {
         ImmersionKitProfile.FieldMapping.SourceComments = "Notes";
         _run = Check();
      }

      [XF] public void a_problem_with_the_profile_names_the_missing_field() => _run.ProblemsWithTheProfile.Must().Satisfy(problems => problems.Any(it => it.Description.Contains("has no field named Notes")));
   }

   public class whose_audio_folder_is_outside_free_and_the_commercial_repositories : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public whose_audio_folder_is_outside_free_and_the_commercial_repositories()
      {
         ImmersionKitProfile.AudioFolder = "vocab/TTS";
         _run = Check();
      }

      [XF] public void a_problem_with_the_profile_says_where_the_audio_folder_must_be() => _run.ProblemsWithTheProfile.Must().Satisfy(problems => problems.Any(it => it.Description.Contains("must be free/ or a commercial repository")));
   }

   public class with_a_sentence_card_whose_audio_is_missing_from_Ankis_media_folder : When_checking_an_import_profile
   {
      readonly ImportRun _run;
      readonly RawNote _card;

      public with_a_sentence_card_whose_audio_is_missing_from_Ankis_media_folder()
      {
         _card = AddImmersionKitSentenceCardWithoutItsMediaFiles("文です", "anime_1", "[sound:missing.mp3]", "");
         _run = Check();
      }

      [XF] public void the_card_has_a_problem() => CheckOf(_run, _card).Outcome.Must().Be(RawNoteOutcome.Problem);
      [XF] public void its_audio_is_marked_as_a_problem() => CheckOf(_run, _card).Audio.Mark.Must().Be(CheckMark.Problem);
      [XF] public void whats_wrong_names_the_missing_file() => CheckOf(_run, _card).WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("Audio missing.mp3 isn't in Anki's media folder")));
      [XF] public void it_cannot_run() => _run.CanExecute.Must().BeFalse();
   }

   public class with_a_sentence_card_whose_audio_is_not_an_mp3_file : When_checking_an_import_profile
   {
      readonly ImportRun _run;
      readonly RawNote _card;

      public with_a_sentence_card_whose_audio_is_not_an_mp3_file()
      {
         _card = AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.wav.ogg", "anime_1.jpg");
         _run = Check();
      }

      [XF] public void whats_wrong_says_it_is_not_mp3() => CheckOf(_run, _card).WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("Audio anime_1.wav.ogg isn't MP3")));
      [XF] public void the_finding_of_its_audio_says_the_corpus_holds_only_mp3() => CheckOf(_run, _card).Audio.Finding.Must().Contain("the corpus holds only MP3");
   }

   public class with_a_sentence_card_whose_question_is_empty : When_checking_an_import_profile
   {
      readonly ImportRun _run;
      readonly RawNote _card;

      public with_a_sentence_card_whose_question_is_empty()
      {
         _card = AddImmersionKitSentenceCard(" ", "anime_1", "anime_1.mp3", "anime_1.jpg");
         _run = Check();
      }

      [XF] public void its_question_is_marked_as_a_problem() => CheckOf(_run, _card).Question.Mark.Must().Be(CheckMark.Problem);
      [XF] public void whats_wrong_says_the_question_is_empty() => CheckOf(_run, _card).WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("Question is empty")));
   }

   public class with_two_sentence_cards_using_the_same_audio_file : When_checking_an_import_profile
   {
      readonly ImportRun _run;
      readonly RawNote _secondCard;

      public with_two_sentence_cards_using_the_same_audio_file()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "shared.mp3", "anime_1.jpg");
         _secondCard = AddImmersionKitSentenceCardWithoutItsMediaFiles("別の文", "anime_2", "[sound:shared.mp3]", "");
         _run = Check();
      }

      [XF] public void whats_wrong_says_another_card_of_the_run_uses_it_too() => CheckOf(_run, _secondCard).WhatIsWrong.Must().Satisfy(whatIsWrong => whatIsWrong.Any(it => it.Contains("Audio shared.mp3 is used by another raw note of this run too")));
   }

   public class with_valid_sentence_cards : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public with_valid_sentence_cards()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.mp3", "anime_1.jpg");
         AddImmersionKitSentenceCard("別の文", "anime_2", "anime_2.mp3", "anime_2.jpg");
         _run = Check();
      }

      [XF] public void nothing_stops_the_run() => _run.CanExecute.Must().BeTrue();
      [XF] public void every_card_will_be_imported() => _run.With(RawNoteOutcome.WillBeImported).Count.Must().Be(2);
      [XF] public void nothing_is_imported_yet() => Sentences.All().Must().BeEmpty();
      [XF] public void every_checked_field_is_fine() => _run.CheckedRawNotes.SelectMany(it => new[] { it.Question, it.ExternalId, it.Audio, it.Screenshot }).Must().Satisfy(fields => fields.All(it => it.Mark == CheckMark.Fine));
   }

   public class that_maps_no_external_id : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public that_maps_no_external_id()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.mp3", "anime_1.jpg");
         ImmersionKitProfile.FieldMapping.ExternalId = "";
         _run = Check();
      }

      [XF] public void the_external_id_is_not_checked() => _run.CheckedRawNotes.Single().ExternalId.Mark.Must().Be(CheckMark.Neutral);
      [XF] public void the_card_will_be_imported() => _run.CheckedRawNotes.Single().Outcome.Must().Be(RawNoteOutcome.WillBeImported);
   }
}
