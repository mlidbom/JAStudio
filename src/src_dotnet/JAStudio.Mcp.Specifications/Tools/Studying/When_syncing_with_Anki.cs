using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TestUtils;
using JAStudio.Mcp.Tools.Studying;
using JAStudio.Mcp.Tools.StudyNotes;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Studying;

/// <summary>
/// The tool sync_with_anki runs JAStudio's sync with Anki and reports what it did. The vocab 勉強, with the answer "study", and 部屋, "room", each<br/>
/// with a study-note in the specifications' pretend Anki as JAStudio writes it, in a collection loaded as at startup.
/// </summary>
public class When_syncing_with_Anki : McpServerSpecification
{
   protected readonly PretendAnki Anki;
   readonly NoteId _idOfStudy;

   public When_syncing_with_Anki()
   {
      _idOfStudy = WithAStudyNote(CreateVocab("勉強", "study", "べんきょう")).GetId();
      WithAStudyNote(CreateVocab("部屋", "room", "へや"));
      Anki = GetService<PretendAnki>();
      GetService<JPCollection>().ReloadFromBackend();
   }

   protected VocabNote Study => GetService<VocabCollection>().WithIdOrNone(_idOfStudy) ?? throw new InvalidOperationException("The vocab is gone.");

   protected WhatTheSyncDid SyncWithAnki() => ResultOf<WhatTheSyncDid>(CallTool("sync_with_anki", []));

   public class with_nothing_changed_since_the_load : When_syncing_with_Anki
   {
      readonly WhatTheSyncDid _done;

      public with_nothing_changed_since_the_load() => _done = SyncWithAnki();

      [XF] public void it_reports_both_study_notes_as_read() => _done.StudyNotesRead.Must().Be(2);
      [XF] public void it_reports_no_study_note_as_written() => _done.NotesWhoseStudyNoteWasWritten.Vocab.Count.Must().Be(0);
   }

   public class with_the_study_note_of_study_having_another_question_in_Anki_and_its_reading_card_suspended_there : When_syncing_with_Anki
   {
      readonly WhatTheSyncDid _done;

      public with_the_study_note_of_study_having_another_question_in_Anki_and_its_reading_card_suspended_there()
      {
         Anki.ChangeTheFieldOfTheStudyNoteOf(Study, AnkiFieldNames.Question, "勉");
         Anki.SuspendTheCard(Study, CardTypes.Reading);
         _done = SyncWithAnki();
      }

      [XF] public void it_reports_study_as_the_vocab_whose_study_note_was_written() => _done.NotesWhoseStudyNoteWasWritten.Vocab.Notes.Select(it => it.Question).Must().SequenceEqual(["勉強"]);
      [XF] public void it_reports_study_as_the_vocab_whose_study_note_had_changed() => _done.NotesWhoseStudyNoteHadChanged.Vocab.Notes.Select(it => it.Question).Must().SequenceEqual(["勉強"]);
      [XF] public void the_question_in_Anki_is_the_one_JAStudio_writes() => Anki.FieldsOfTheStudyNoteOf(Study)[AnkiFieldNames.Question].Must().Be("勉強");
      [XF] public void study_has_a_suspended_card() => Study.HasSuspendedCards().Must().BeTrue();
   }

   public class with_an_Anki_note_of_one_of_JAStudios_note_types_that_holds_no_corpus_note_id : When_syncing_with_Anki
   {
      readonly long _itsAnkiNoteId;
      readonly WhatTheSyncDid _done;

      public with_an_Anki_note_of_one_of_JAStudios_note_types_that_holds_no_corpus_note_id()
      {
         _itsAnkiNoteId = Anki.AddAStudyNoteThatHoldsNoCorpusNoteId(new Dictionary<string, string> { [AnkiFieldNames.Question] = "孤児", [AnkiFieldNames.JasNoteId] = "" });
         _done = SyncWithAnki();
      }

      [XF] public void it_is_listed_with_its_Anki_note_id_and_question_among_the_study_notes_that_hold_no_notes_id() =>
         _done.StudyNotesThatHoldNoNotesId.Must().SequenceEqual([new StudyNoteOfNoNote(_itsAnkiNoteId, "孤児", "")]);
   }

   public class while_Ankis_own_sync_is_under_way : When_syncing_with_Anki
   {
      public while_Ankis_own_sync_is_under_way() => Anki.StartItsOwnSync();

      [XF] public void the_tool_refuses_saying_that_Anki_is_syncing() =>
         Invoking(SyncWithAnki).Must().Throw<InvalidOperationException>().Which.Message.Must().Contain("The sync with Anki can't run now: Anki is syncing.");
   }
}
