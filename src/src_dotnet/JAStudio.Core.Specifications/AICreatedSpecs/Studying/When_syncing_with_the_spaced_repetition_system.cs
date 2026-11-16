using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Studying;
using JAStudio.Core.TestUtils;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Studying;

/// <summary>
/// JAStudio's sync with the spaced repetition system (<see cref="SRSSync"/>), which a person starts: it reads the study-notes, gives each note<br/>
/// what the spaced repetition system holds of its study-note, writes the study-notes that differ from what JAStudio would write now, and answers<br/>
/// with what it did. The sentences 家に帰る and 駅に行く, each with a study-note in Anki as JAStudio writes it, untagged and with no card suspended.
/// </summary>
public class When_syncing_with_the_spaced_repetition_system : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly JPCollection Collection;
   protected readonly PretendAnki Anki;
   protected readonly SRSSync Sync;
   readonly NoteId _idOfTheSentenceAboutHome;
   readonly NoteId _idOfTheSentenceAboutTheStation;

   public When_syncing_with_the_spaced_repetition_system()
   {
      _idOfTheSentenceAboutHome = WithAStudyNote(CreateTestSentence("家に帰る", "Go home")).GetId();
      _idOfTheSentenceAboutTheStation = WithAStudyNote(CreateTestSentence("駅に行く", "Go to the station")).GetId();
      Collection = GetService<JPCollection>();
      Anki = GetService<PretendAnki>();
      Sync = GetService<SRSSync>();
   }

   protected SentenceNote TheSentenceAboutHome => TheSentenceWith(_idOfTheSentenceAboutHome);
   protected SentenceNote TheSentenceAboutTheStation => TheSentenceWith(_idOfTheSentenceAboutTheStation);

   SentenceNote TheSentenceWith(NoteId id) => Collection.Sentences.WithIdOrNone(id) ?? throw new InvalidOperationException("The sentence is gone.");

   protected static IEnumerable<string> QuestionsOf(IEnumerable<JPNote> notes) => notes.Select(it => it.GetQuestion()).Order(StringComparer.Ordinal);

   public class before_the_collection_has_loaded : When_syncing_with_the_spaced_repetition_system
   {
      [XF] public void a_sync_cannot_run_and_says_that_the_collection_has_not_loaded() => Sync.WhyItCannotRunNow.Must().Be("JAStudio hasn't loaded the collection yet.");
      [XF] public void running_one_anyway_throws() => Invoking(() => Sync.Run()).Must().Throw<InvalidOperationException>();
   }

   public class in_a_collection_loaded_as_at_startup : When_syncing_with_the_spaced_repetition_system
   {
      public in_a_collection_loaded_as_at_startup() => Collection.ReloadFromBackend();

      [XF] public void a_sync_can_run() => Sync.WhyItCannotRunNow.Must().BeNull();

      public class with_nothing_changed_since_the_load : in_a_collection_loaded_as_at_startup
      {
         readonly SRSSyncReport _report;

         public with_nothing_changed_since_the_load() => _report = Sync.Run();

         [XF] public void it_reads_both_study_notes() => _report.StudyNotesRead.Must().Be(2);
         [XF] public void no_notes_study_note_had_changed() => QuestionsOf(_report.NotesWhoseStudyNoteHadChanged).Must().BeEmpty();
         [XF] public void no_notes_study_note_is_gone() => QuestionsOf(_report.NotesWhoseStudyNoteIsGone).Must().BeEmpty();
         [XF] public void it_writes_no_study_note() => QuestionsOf(_report.NotesWhoseStudyNoteWasWritten).Must().BeEmpty();
         [XF] public void no_study_note_holds_no_notes_id() => _report.StudyNotesThatHoldNoNotesId.Count.Must().Be(0);
         [XF] public void no_study_note_is_of_a_note_the_collection_has_not() => _report.StudyNotesWhoseNoteTheCollectionHasNot.Count.Must().Be(0);
      }

      public class after_the_student_suspended_the_reading_card_of_the_sentence_about_home_and_tagged_it_in_Anki : in_a_collection_loaded_as_at_startup
      {
         readonly SRSSyncReport _report;

         public after_the_student_suspended_the_reading_card_of_the_sentence_about_home_and_tagged_it_in_Anki()
         {
            Anki.SuspendTheCard(TheSentenceAboutHome, CardTypes.Reading);
            Anki.Tag(TheSentenceAboutHome, "leech");
            _report = Sync.Run();
         }

         [XF] public void the_sentence_has_a_suspended_card() => TheSentenceAboutHome.HasSuspendedCards().Must().BeTrue();
         [XF] public void the_sentence_has_the_tag_as_an_Anki_tag() => TheSentenceAboutHome.AnkiTags.Select(it => it.Name).Must().SequenceEqual(["anki::leech"]);
         [XF] public void it_reports_only_that_sentence_as_one_whose_study_note_had_changed() => QuestionsOf(_report.NotesWhoseStudyNoteHadChanged).Must().SequenceEqual(["家に帰る"]);
         [XF] public void it_writes_no_study_note() => QuestionsOf(_report.NotesWhoseStudyNoteWasWritten).Must().BeEmpty();
      }

      public class after_the_study_note_of_the_sentence_about_home_was_deleted_in_Anki : in_a_collection_loaded_as_at_startup
      {
         readonly SRSSyncReport _report;

         public after_the_study_note_of_the_sentence_about_home_was_deleted_in_Anki()
         {
            Anki.DeleteTheStudyNoteOf(TheSentenceAboutHome);
            _report = Sync.Run();
         }

         [XF] public void the_sentence_no_longer_has_a_study_note() => TheSentenceAboutHome.HasAStudyNote().Must().BeFalse();
         [XF] public void it_reads_the_one_study_note_left() => _report.StudyNotesRead.Must().Be(1);
         [XF] public void it_reports_that_sentence_as_one_whose_study_note_is_gone() => QuestionsOf(_report.NotesWhoseStudyNoteIsGone).Must().SequenceEqual(["家に帰る"]);
         [XF] public void it_does_not_report_it_as_one_whose_study_note_had_changed() => QuestionsOf(_report.NotesWhoseStudyNoteHadChanged).Must().BeEmpty();
      }

      /// <summary>As when the note was changed in JAStudio, or the corpus checkout fast-forwarded, since JAStudio last wrote the study-note.</summary>
      public class with_the_study_note_of_the_sentence_about_home_having_another_question_in_Anki_than_JAStudio_writes : in_a_collection_loaded_as_at_startup
      {
         readonly SRSSyncReport _report;

         public with_the_study_note_of_the_sentence_about_home_having_another_question_in_Anki_than_JAStudio_writes()
         {
            Anki.ChangeTheFieldOfTheStudyNoteOf(TheSentenceAboutHome, AnkiFieldNames.Question, "家");
            _report = Sync.Run();
         }

         [XF] public void it_writes_that_sentences_study_note_only() => QuestionsOf(_report.NotesWhoseStudyNoteWasWritten).Must().SequenceEqual(["家に帰る"]);
         [XF] public void the_question_in_Anki_is_the_one_JAStudio_writes() => Anki.FieldsOfTheStudyNoteOf(TheSentenceAboutHome)[AnkiFieldNames.Question].Must().Be("家に帰る");
         [XF] public void it_does_not_report_the_sentence_as_one_whose_study_note_had_changed() => QuestionsOf(_report.NotesWhoseStudyNoteHadChanged).Must().BeEmpty();

         public class and_then_syncing_again : with_the_study_note_of_the_sentence_about_home_having_another_question_in_Anki_than_JAStudio_writes
         {
            readonly SRSSyncReport _secondReport;

            public and_then_syncing_again() => _secondReport = Sync.Run();

            [XF] public void it_writes_no_study_note() => QuestionsOf(_secondReport.NotesWhoseStudyNoteWasWritten).Must().BeEmpty();
         }
      }

      public class with_an_Anki_note_of_one_of_JAStudios_note_types_that_holds_no_corpus_note_id : in_a_collection_loaded_as_at_startup
      {
         readonly long _itsAnkiNoteId;
         readonly SRSSyncReport _report;

         public with_an_Anki_note_of_one_of_JAStudios_note_types_that_holds_no_corpus_note_id()
         {
            _itsAnkiNoteId = Anki.AddAStudyNoteThatHoldsNoCorpusNoteId(new Dictionary<string, string> { [AnkiFieldNames.Question] = "孤児", [AnkiFieldNames.JasNoteId] = "" });
            _report = Sync.Run();
         }

         [XF] public void it_reads_it_with_the_other_two() => _report.StudyNotesRead.Must().Be(3);
         [XF] public void it_reports_it_as_a_study_note_that_holds_no_notes_id() => _report.StudyNotesThatHoldNoNotesId.Select(it => it.StudyNoteId).Must().SequenceEqual([_itsAnkiNoteId]);
         [XF] public void it_writes_no_study_note() => QuestionsOf(_report.NotesWhoseStudyNoteWasWritten).Must().BeEmpty();
         [XF] public void no_note_is_made_of_it() => Collection.Sentences.All().Count.Must().Be(2);
      }

      public class after_the_sentence_about_the_station_was_deleted_in_JAStudio_and_its_study_note_kept : in_a_collection_loaded_as_at_startup
      {
         readonly SRSSyncReport _report;

         public after_the_sentence_about_the_station_was_deleted_in_JAStudio_and_its_study_note_kept()
         {
            Collection.Delete(TheSentenceAboutTheStation, ItsStudyNote.IsKept);
            _report = Sync.Run();
         }

         [XF] public void it_reports_its_study_note_as_one_of_a_note_the_collection_has_not() =>
            _report.StudyNotesWhoseNoteTheCollectionHasNot.Select(it => it.Fields[AnkiFieldNames.Question]).Must().SequenceEqual(["駅に行く"]);
         [XF] public void it_does_not_report_it_as_a_study_note_that_holds_no_notes_id() => _report.StudyNotesThatHoldNoNotesId.Count.Must().Be(0);
         [XF] public void the_study_note_stays_in_Anki() => _report.StudyNotesRead.Must().Be(2);
      }

      public class while_Ankis_own_sync_is_under_way : in_a_collection_loaded_as_at_startup
      {
         public while_Ankis_own_sync_is_under_way() => Anki.StartItsOwnSync();

         [XF] public void a_sync_cannot_run_and_says_that_Anki_is_syncing() => Sync.WhyItCannotRunNow.Must().Be("Anki is syncing.");
         [XF] public void running_one_anyway_throws() => Invoking(() => Sync.Run()).Must().Throw<InvalidOperationException>();

         public class and_once_it_has_finished : while_Ankis_own_sync_is_under_way
         {
            public and_once_it_has_finished() => Anki.FinishItsOwnSync();

            [XF] public void a_sync_can_run_again() => Sync.WhyItCannotRunNow.Must().BeNull();
         }
      }

      public class once_Anki_has_opened_another_profile_whose_collection_holds_no_study_notes : in_a_collection_loaded_as_at_startup
      {
         public once_Anki_has_opened_another_profile_whose_collection_holds_no_study_notes() => Anki.OpenAnotherProfileWhoseCollectionHoldsNoStudyNotes();

         [XF] public void a_sync_cannot_run_and_says_that_Anki_has_another_collection_open() =>
            Sync.WhyItCannotRunNow.Must().Be("Anki has another collection open than the one JAStudio works with.");

         [XF] public void the_sync_says_that_JAStudio_can_work_with_that_collection_instead() => Sync.TheSRSHasAnotherCollectionOpenThanJAStudioWorksWith.Must().BeTrue();

         [XF] public void both_sentences_still_have_the_study_notes_of_the_collection_JAStudio_works_with() =>
            new { Home = TheSentenceAboutHome.HasAStudyNote(), TheStation = TheSentenceAboutTheStation.HasAStudyNote() }.Must().Satisfy(it => it.Home && it.TheStation);

         public class after_JAStudio_is_told_to_work_with_the_collection_Anki_has_open : once_Anki_has_opened_another_profile_whose_collection_holds_no_study_notes
         {
            public after_JAStudio_is_told_to_work_with_the_collection_Anki_has_open() => Sync.WorkWithTheCollectionTheSRSHasOpen();

            [XF] public void neither_sentence_has_a_study_note() =>
               new { Home = TheSentenceAboutHome.HasAStudyNote(), TheStation = TheSentenceAboutTheStation.HasAStudyNote() }.Must().Satisfy(it => !it.Home && !it.TheStation);

            [XF] public void a_sync_can_run_once_more() => Sync.WhyItCannotRunNow.Must().BeNull();
            [XF] public void Anki_no_longer_has_another_collection_open_than_JAStudio_works_with() => Sync.TheSRSHasAnotherCollectionOpenThanJAStudioWorksWith.Must().BeFalse();
         }
      }
   }
}
