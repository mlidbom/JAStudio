using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

/// <summary>Checking prepares an <see cref="ImportRun"/> without changing anything, and any problem it finds stops the whole run.</summary>
public class When_checking_an_import_profile : ImportingSpecification, IAIGeneratedSpec
{
   ImportRun Check() => Importer.Prepare(ImmersionKitProfile);

   static bool AProblemSays(ImportRun run, string text) => run.Problems.Any(it => it.Description.Contains(text));

   public class that_has_no_Anki_note_type : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public that_has_no_Anki_note_type()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.mp3", "anime_1.jpg");
         ImmersionKitProfile.AnkiNoteType = "";
         _run = Check();
      }

      [XF] public void a_problem_asks_for_the_note_type() => AProblemSays(_run, "Choose the Anki note type").Must().BeTrue();
      [XF] public void no_raw_notes_are_selected() => _run.RawNotes.Must().BeEmpty();
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

      [XF] public void a_problem_says_JAStudios_own_note_types_cannot_be_imported_from() => AProblemSays(_run, "JAStudio's own note types can't be imported from").Must().BeTrue();
   }

   public class whose_source_question_is_unmapped : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public whose_source_question_is_unmapped()
      {
         ImmersionKitProfile.FieldMapping.SourceQuestion = "";
         _run = Check();
      }

      [XF] public void a_problem_asks_for_the_source_question_to_be_mapped() => AProblemSays(_run, "Map an Anki field to the sentence's source question").Must().BeTrue();
   }

   public class that_maps_a_field_the_note_type_does_not_have : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public that_maps_a_field_the_note_type_does_not_have()
      {
         ImmersionKitProfile.FieldMapping.SourceComments = "Notes";
         _run = Check();
      }

      [XF] public void a_problem_names_the_missing_field() => AProblemSays(_run, "has no field named Notes").Must().BeTrue();
   }

   public class whose_audio_folder_is_outside_free_and_the_commercial_repositories : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public whose_audio_folder_is_outside_free_and_the_commercial_repositories()
      {
         ImmersionKitProfile.AudioFolder = "vocab/TTS";
         _run = Check();
      }

      [XF] public void a_problem_says_where_the_audio_folder_must_be() => AProblemSays(_run, "must be free/ or a commercial repository").Must().BeTrue();
   }

   public class with_a_sentence_card_whose_audio_is_missing_from_Ankis_media_folder : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public with_a_sentence_card_whose_audio_is_missing_from_Ankis_media_folder()
      {
         AddImmersionKitSentenceCardWithoutItsMediaFiles("文です", "anime_1", "[sound:missing.mp3]", "");
         _run = Check();
      }

      [XF] public void a_problem_names_the_missing_file() => AProblemSays(_run, "Its file missing.mp3 isn't in Anki's media folder").Must().BeTrue();
      [XF] public void it_cannot_run() => _run.CanExecute.Must().BeFalse();
   }

   public class with_a_sentence_card_whose_audio_is_not_an_mp3_file : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public with_a_sentence_card_whose_audio_is_not_an_mp3_file()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "anime_1.wav.ogg", "anime_1.jpg");
         _run = Check();
      }

      [XF] public void a_problem_says_the_corpus_holds_only_mp3() => AProblemSays(_run, "isn't an MP3 file, and the corpus holds only MP3").Must().BeTrue();
   }

   public class with_a_sentence_card_whose_question_is_empty : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public with_a_sentence_card_whose_question_is_empty()
      {
         AddImmersionKitSentenceCard(" ", "anime_1", "anime_1.mp3", "anime_1.jpg");
         _run = Check();
      }

      [XF] public void a_problem_says_the_question_field_is_empty() => AProblemSays(_run, "Its field Expression, the sentence's source question, is empty").Must().BeTrue();
   }

   public class with_two_sentence_cards_referencing_the_same_audio_file : When_checking_an_import_profile
   {
      readonly ImportRun _run;

      public with_two_sentence_cards_referencing_the_same_audio_file()
      {
         AddImmersionKitSentenceCard("文です", "anime_1", "shared.mp3", "anime_1.jpg");
         AddImmersionKitSentenceCardWithoutItsMediaFiles("別の文", "anime_2", "[sound:shared.mp3]", "");
         _run = Check();
      }

      [XF] public void a_problem_says_another_card_of_the_run_references_it_too() => AProblemSays(_run, "Its file shared.mp3 is referenced by another raw note of this run too").Must().BeTrue();
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

      [XF] public void there_are_no_problems() => _run.Problems.Must().BeEmpty();
      [XF] public void every_card_is_to_be_imported() => _run.NotesToImport.Count.Must().Be(2);
      [XF] public void nothing_is_imported_yet() => Sentences.All().Must().BeEmpty();
   }
}
