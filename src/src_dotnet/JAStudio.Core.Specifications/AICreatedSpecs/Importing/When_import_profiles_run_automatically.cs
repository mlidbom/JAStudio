using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.TaskRunners;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

/// <summary>
/// The import profile for Immersion Kit's sentence cards runs automatically, and a sentence card is in Anki. A second profile, for a subs2srs deck's<br/>
/// note type, has a raw note in Anki too, and would import it if it ran. The automatic runs happen in the background, so each specification waits<br/>
/// for the one it expects, and the runs happen one after another, in the order of the saved profiles.
/// </summary>
public class When_import_profiles_run_automatically : ImportingSpecification, IAIGeneratedSpec
{
   const string Subs2srsNoteType = "subs2srs Sentence";
   static readonly TimeSpan TimeToWaitForTheRun = TimeSpan.FromSeconds(30);

   readonly ConcurrentQueue<ImportRun> _executedRuns = new();
   readonly ConcurrentQueue<ImportProfile> _profilesStoppedByProblems = new();
   protected readonly RawNote SentenceCard;
   protected readonly RawNote Subs2srsNote;

   protected readonly ImportProfile Subs2srsProfile = new()
   {
      Name = "subs2srs",
      AnkiNoteType = Subs2srsNoteType,
      FieldMapping = new SentenceFieldMapping { SourceQuestion = "Expression", SourceAnswer = "Meaning" }
   };

   public When_import_profiles_run_automatically()
   {
      AutomaticImportRuns.Executed.Subscribe(_executedRuns.Enqueue);
      AutomaticImportRuns.StoppedByProblems.Subscribe(_profilesStoppedByProblems.Enqueue);

      AnkiRawNotes.AddNoteType(new AnkiNoteType(Subs2srsNoteType, ["Expression", "Meaning"]));
      Subs2srsNote = AnkiRawNotes.Add(Subs2srsNoteType, new Dictionary<string, string> { ["Expression"] = "猫が好きだ", ["Meaning"] = "I like cats" });
      SentenceCard = AddImmersionKitSentenceCard("ずっと1人で生きていくんだ", "anime_the_garden_of_words_000000346", "garden_of_words_346.mp3", "garden_of_words_346.jpg");

      ImmersionKitProfile.RunsAutomatically = true;
   }

   AutomaticImportRuns AutomaticImportRuns => GetService<AutomaticImportRuns>();
   SavedImportProfiles SavedProfiles => GetService<SavedImportProfiles>();

   protected void Save(params ImportProfile[] profiles) => profiles.ToList().ForEach(SavedProfiles.Add);

   protected void AnkiImportedAFile() => AutomaticImportRuns.AfterAnkiImportedAFile();
   protected void ANoteWasAddedInAnki(string ankiNoteType) => AutomaticImportRuns.AfterANoteWasAddedInAnki(ankiNoteType);

   protected IReadOnlyList<ImportRun> ExecutedRuns => [.. _executedRuns];
   protected IReadOnlyList<ImportProfile> ProfilesStoppedByProblems => [.. _profilesStoppedByProblems];

   protected void WaitForAnExecutedRun() => WaitUntilTheRuns(it => it.Executed.Count > 0);
   protected void WaitForARunStoppedByProblems() => WaitUntilTheRuns(it => it.StoppedByProblems.Count > 0);
   protected void WaitForExecutedRuns(int count) => WaitUntilTheRuns(it => it.Executed.Count == count);

   /// <summary>Waits for the automatic runs to have done what <paramref name="condition"/> asks, and fails showing what they did when they haven't in time.</summary>
   void WaitUntilTheRuns(Func<WhatTheRunsDid, bool> condition)
   {
      SpinWait.SpinUntil(() => condition(WhatTheRunsHaveDone), TimeToWaitForTheRun);
      WhatTheRunsHaveDone.Must().Satisfy(condition);
   }

   WhatTheRunsDid WhatTheRunsHaveDone => new([.. ExecutedRuns.Select(it => it.Profile.Name)],
                                             [.. ProfilesStoppedByProblems.Select(it => it.Name)],
                                             [.. GetService<RethrowingFatalErrorHandler>().HandledExceptions.Select(it => it.ToString())]);

   /// <summary>
   /// The automatic runs so far, by the names of their profiles: those executed, and those stopped by problems. And the exceptions background tasks<br/>
   /// threw, such as a run's: the runs happen in a background task.
   /// </summary>
   record WhatTheRunsDid(IReadOnlyList<string> Executed, IReadOnlyList<string> StoppedByProblems, IReadOnlyList<string> BackgroundExceptions);

   protected IEnumerable<string> TheQuestionsOfTheImportedSentences() => Sentences.All().Select(it => it.SourceData.Question.Value);

   public class with_the_subs2srs_profile_not_running_automatically : When_import_profiles_run_automatically
   {
      public with_the_subs2srs_profile_not_running_automatically() => Save(Subs2srsProfile, ImmersionKitProfile);

      public class after_Anki_imported_a_file : with_the_subs2srs_profile_not_running_automatically
      {
         public after_Anki_imported_a_file()
         {
            AnkiImportedAFile();
            WaitForAnExecutedRun();
         }

         [XF] public void the_sentence_card_is_imported() => TheQuestionsOfTheImportedSentences().Must().Contain("ずっと1人で生きていくんだ");
         [XF] public void the_sentence_card_is_tagged_as_imported_in_Anki() => AnkiRawNotes.IsTaggedAsImported(SentenceCard).Must().BeTrue();
         [XF] public void the_run_of_the_Immersion_Kit_profile_is_notified_with_the_sentence_it_imported() => ExecutedRuns.Single().ImportedSentences.Count.Must().Be(1);
         [XF] public void the_subs2srs_note_is_not_imported() => TheQuestionsOfTheImportedSentences().Must().Satisfy(it => !it.Contains("猫が好きだ"));
         [XF] public void the_subs2srs_note_is_not_tagged_as_imported() => AnkiRawNotes.IsTaggedAsImported(Subs2srsNote).Must().BeFalse();
      }
   }

   public class with_the_subs2srs_profile_running_automatically_too : When_import_profiles_run_automatically
   {
      public with_the_subs2srs_profile_running_automatically_too()
      {
         Subs2srsProfile.RunsAutomatically = true;
         Save(Subs2srsProfile, ImmersionKitProfile);
      }

      public class after_Anki_imported_a_file : with_the_subs2srs_profile_running_automatically_too
      {
         public after_Anki_imported_a_file()
         {
            AnkiImportedAFile();
            WaitForExecutedRuns(2);
         }

         [XF] public void the_sentence_card_is_imported() => TheQuestionsOfTheImportedSentences().Must().Contain("ずっと1人で生きていくんだ");
         [XF] public void the_subs2srs_note_is_imported() => TheQuestionsOfTheImportedSentences().Must().Contain("猫が好きだ");
      }

      public class after_a_note_of_the_Immersion_Kit_note_type_was_added_in_Anki : with_the_subs2srs_profile_running_automatically_too
      {
         public after_a_note_of_the_Immersion_Kit_note_type_was_added_in_Anki()
         {
            ANoteWasAddedInAnki(ImmersionKitNoteType);
            WaitForAnExecutedRun();
         }

         [XF] public void the_sentence_card_is_imported() => TheQuestionsOfTheImportedSentences().Must().Contain("ずっと1人で生きていくんだ");
         [XF] public void the_subs2srs_profile_does_not_run_so_its_note_is_not_imported() => TheQuestionsOfTheImportedSentences().Must().Satisfy(it => !it.Contains("猫が好きだ"));
         [XF] public void only_the_run_of_the_Immersion_Kit_profile_is_notified() => ExecutedRuns.Select(it => it.Profile.Name).Must().SequenceEqual(["Immersion Kit"]);
      }
   }

   public class with_an_automatic_profile_of_a_note_type_the_Anki_profile_does_not_have_saved_before_the_Immersion_Kit_profile : When_import_profiles_run_automatically
   {
      public with_an_automatic_profile_of_a_note_type_the_Anki_profile_does_not_have_saved_before_the_Immersion_Kit_profile() =>
         Save(new ImportProfile { Name = "Kanshudo", AnkiNoteType = "Kanshudo Sentence", RunsAutomatically = true }, ImmersionKitProfile);

      public class after_Anki_imported_a_file : with_an_automatic_profile_of_a_note_type_the_Anki_profile_does_not_have_saved_before_the_Immersion_Kit_profile
      {
         public after_Anki_imported_a_file()
         {
            AnkiImportedAFile();
            WaitForAnExecutedRun();
         }

         [XF] public void that_profile_does_not_run_so_the_missing_note_type_does_not_stop_it_with_a_problem() => ProfilesStoppedByProblems.Must().BeEmpty();
         [XF] public void the_sentence_card_is_imported() => TheQuestionsOfTheImportedSentences().Must().Contain("ずっと1人で生きていくんだ");
      }
   }

   public class with_a_second_sentence_card_whose_audio_file_is_not_in_Ankis_media_folder : When_import_profiles_run_automatically
   {
      public with_a_second_sentence_card_whose_audio_file_is_not_in_Ankis_media_folder()
      {
         AddImmersionKitSentenceCardWithoutItsMediaFiles("学校に行く", "anime_the_garden_of_words_000000400", "[sound:garden_of_words_400.mp3]", "");
         Save(ImmersionKitProfile);
      }

      public class after_Anki_imported_a_file : with_a_second_sentence_card_whose_audio_file_is_not_in_Ankis_media_folder
      {
         public after_Anki_imported_a_file()
         {
            AnkiImportedAFile();
            WaitForARunStoppedByProblems();
         }

         [XF] public void the_Immersion_Kit_profile_is_notified_as_stopped_by_problems() => ProfilesStoppedByProblems.Must().SequenceEqual([ImmersionKitProfile]);
         [XF] public void nothing_is_imported() => Sentences.All().Must().BeEmpty();
         [XF] public void the_first_sentence_card_is_not_tagged_as_imported() => AnkiRawNotes.IsTaggedAsImported(SentenceCard).Must().BeFalse();
         [XF] public void no_run_is_notified_as_executed() => ExecutedRuns.Must().BeEmpty();
      }
   }
}
