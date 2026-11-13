using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TestUtils;
using JAStudio.Mcp.Tools.StudyNotes;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.StudyNotes;

/// <summary>
/// The vocab 勉強, with the answer "study", whose study-note is in the specifications' pretend Anki as JAStudio writes it. Unlike JAStudio inside<br/>
/// Anki, the pretend Anki takes in only what's written or put into it.
/// </summary>
public class When_comparing_the_study_notes_with_Anki : McpServerSpecification
{
   protected readonly VocabNote Vocab;
   protected readonly InMemoryAnkiStudyNotes StudyNotesInAnki;

   public When_comparing_the_study_notes_with_Anki()
   {
      Vocab = WithAStudyNote(CreateVocab("勉強", "study", "べんきょう"));
      StudyNotesInAnki = GetService<InMemoryAnkiStudyNotes>();
      StudyNotesInAnki.Write(Vocab);
   }

   protected StudyNotesThatDiffer ListTheStudyNotesThatDiffer() => ResultOf<StudyNotesThatDiffer>(CallTool("list_the_study_notes_that_differ", []));

   protected StudyNoteInAnki TheVocabsStudyNote => StudyNotesInAnki.ReadAll().Single();

   public class whose_study_note_is_as_JAStudio_writes_it : When_comparing_the_study_notes_with_Anki
   {
      readonly StudyNotesThatDiffer _found;

      public whose_study_note_is_as_JAStudio_writes_it() => _found = ListTheStudyNotesThatDiffer();

      [XF] public void one_study_note_is_compared() => _found.StudyNotesCompared.Must().Be(1);
      [XF] public void no_vocab_is_listed() => _found.Vocab.Count.Must().Be(0);
   }

   public class whose_study_note_has_another_question_in_Anki : When_comparing_the_study_notes_with_Anki
   {
      readonly StudyNotesThatDiffer _found;

      public whose_study_note_has_another_question_in_Anki()
      {
         StudyNotesInAnki.Put(new StudyNoteInAnki(TheVocabsStudyNote.AnkiNoteId, new Dictionary<string, string>(TheVocabsStudyNote.Fields) { [AnkiFieldNames.Question] = "勉" }));
         _found = ListTheStudyNotesThatDiffer();
      }

      StudyNoteThatDiffers TheVocabListed => _found.Vocab.Notes.Single();

      [XF] public void the_vocab_is_listed() => TheVocabListed.Note.Question.Must().Be("勉強");
      [XF] public void the_question_is_the_only_field_reported_as_differing() => TheVocabListed.Fields.Select(it => it.Field).Must().SequenceEqual([AnkiFieldNames.Question]);
      [XF] public void the_question_is_reported_as_Anki_has_it_and_as_JAStudio_writes_it() => TheVocabListed.Fields.Single().Must().Be(new StudyNoteFieldDifference(AnkiFieldNames.Question, "勉", "勉強"));
      [XF] public void the_question_is_counted_as_differing_in_one_study_note() => _found.Fields.Single().Must().Be(new StudyNoteFieldDifferenceCount(AnkiFieldNames.Question, 1));

      public class then_writing_the_study_notes_that_differ : whose_study_note_has_another_question_in_Anki
      {
         readonly StudyNotesWritten _written;

         public then_writing_the_study_notes_that_differ() => _written = ResultOf<StudyNotesWritten>(CallTool("write_the_study_notes_that_differ", []));

         [XF] public void the_vocab_is_reported_written() => _written.Vocab.Notes.Single().Question.Must().Be("勉強");
         [XF] public void no_study_note_differs_afterwards() => _written.StudyNotesThatDifferAfterwards.Must().Be(0);
         [XF] public void the_question_in_Anki_is_the_one_JAStudio_writes() => TheVocabsStudyNote.Fields[AnkiFieldNames.Question].Must().Be("勉強");
      }
   }

   /// <summary>The vocab given the recordings a.mp3 and b.mp3, which JAStudio writes into Audio_b and Audio_g, and which its study-note has in Anki as given.</summary>
   public class with_two_recordings : When_comparing_the_study_notes_with_Anki
   {
      public with_two_recordings()
      {
         Vocab.Audio.Add(new AudioReference("a.mp3"));
         Vocab.Audio.Add(new AudioReference("b.mp3"));
      }

      protected StudyNotesThatDiffer ListTheStudyNotesThatDifferWithTheAudioInAnki(string audioB, string audioG, bool leavingOutTheNotesWhereOnlyTheMarkupOfTheAudioDiffers = false)
      {
         StudyNotesInAnki.Put(new StudyNoteInAnki(TheVocabsStudyNote.AnkiNoteId,
                                                  new Dictionary<string, string>(Vocab.StudyNoteFields()) { [AnkiFieldNames.Vocab.AudioB] = audioB, [AnkiFieldNames.Vocab.AudioG] = audioG }));
         return ResultOf<StudyNotesThatDiffer>(CallTool("list_the_study_notes_that_differ",
                                                        new Dictionary<string, object?> { ["leaveOutTheNotesWhereOnlyTheMarkupOfTheAudioDiffers"] = leavingOutTheNotesWhereOnlyTheMarkupOfTheAudioDiffers }));
      }

      /// <summary>Both in Audio_b in Anki, after a space and with a comma between them.</summary>
      public class both_in_Audio_b_with_other_markup_in_Anki : with_two_recordings
      {
         readonly StudyNotesThatDiffer _found;

         public both_in_Audio_b_with_other_markup_in_Anki() => _found = ListTheStudyNotesThatDifferWithTheAudioInAnki(" [sound:a.mp3], [sound:b.mp3]", "");

         [XF] public void the_vocab_is_listed_as_differing_only_in_the_markup_of_its_audio() => _found.Vocab.Notes.Single().OnlyTheMarkupOfItsAudioDiffers.Must().BeTrue();
         [XF] public void it_is_counted_as_differing_only_in_the_markup_of_its_audio() => (_found.Vocab.Count, _found.Vocab.OnlyInTheMarkupOfTheirAudio).Must().Be((1, 1));

         public class then_leaving_those_out_of_the_list : both_in_Audio_b_with_other_markup_in_Anki
         {
            readonly StudyNotesThatDiffer _foundLeavingThemOut;

            public then_leaving_those_out_of_the_list() => _foundLeavingThemOut = ListTheStudyNotesThatDifferWithTheAudioInAnki(" [sound:a.mp3], [sound:b.mp3]", "", leavingOutTheNotesWhereOnlyTheMarkupOfTheAudioDiffers: true);

            [XF] public void the_vocab_is_counted_but_not_listed() => (_foundLeavingThemOut.Vocab.Count, _foundLeavingThemOut.Vocab.Notes.Count).Must().Be((1, 0));
         }
      }

      /// <summary>In the other order in Anki: b.mp3 in Audio_b, a.mp3 in Audio_g.</summary>
      public class in_the_other_order_in_Anki : with_two_recordings
      {
         readonly StudyNotesThatDiffer _found;

         public in_the_other_order_in_Anki() => _found = ListTheStudyNotesThatDifferWithTheAudioInAnki("[sound:b.mp3]", "[sound:a.mp3]");

         [XF] public void the_vocab_is_listed_as_differing_in_more_than_the_markup_of_its_audio() => _found.Vocab.Notes.Single().OnlyTheMarkupOfItsAudioDiffers.Must().BeFalse();
      }
   }

   public class with_a_study_note_in_Anki_that_belongs_to_no_note : When_comparing_the_study_notes_with_Anki
   {
      readonly StudyNotesThatDiffer _found;

      public with_a_study_note_in_Anki_that_belongs_to_no_note()
      {
         StudyNotesInAnki.Put(new StudyNoteInAnki(ankiNoteId: 999_999, new Dictionary<string, string> { [AnkiFieldNames.Question] = "孤児", [AnkiFieldNames.JasNoteId] = "a-deleted-note" }));
         _found = ListTheStudyNotesThatDiffer();
      }

      [XF] public void it_is_listed_with_its_Anki_note_id_question_and_jas_note_id() =>
         _found.StudyNotesOfNoNote.Must().SequenceEqual([new StudyNoteOfNoNote(999_999, "孤児", "a-deleted-note")]);
      [XF] public void only_the_vocabs_study_note_is_compared() => _found.StudyNotesCompared.Must().Be(1);
   }
}
