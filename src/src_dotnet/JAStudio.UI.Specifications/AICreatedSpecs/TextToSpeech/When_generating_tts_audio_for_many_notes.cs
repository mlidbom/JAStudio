using System;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.UI.ViewModels.TextToSpeech;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.TextToSpeech;

/// <summary>
/// The batch TTS dialog for three sentences, 家に帰る, 明日学校に行く and 何でもない, none with audio, offering the fake tts-provider's Steady voice, the default,<br/>
/// and its Varying voice. Nothing plays: a recording player stands in.
/// </summary>
public class When_generating_tts_audio_for_many_notes : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly FakeTextToSpeechProvider _provider = new();
   readonly RecordingAudioPlayer _player = new();
   readonly ITaggregateReference<SentenceNote>[] _sentenceReferences;
   SentenceNote[] _sentences => [.. _sentenceReferences.Select(it => it.Current)];
   readonly BatchTextToSpeechDialogViewModel _dialog;

   public When_generating_tts_audio_for_many_notes()
   {
      _sentenceReferences = [CreateTestSentence("家に帰る", "Go home").Reference(), CreateTestSentence("明日学校に行く", "I'll go to school tomorrow").Reference(), CreateTestSentence("何でもない", "It's nothing").Reference()];
      _dialog = new BatchTextToSpeechDialogViewModel(_sentences, [_provider.Steady, _provider.Varying], _player, GetService<TemporaryServiceCollection>());
   }

   NoteInTheBatchViewModel Note(int index) => _dialog.Notes[index];

   string[] TheProgressOfEveryNote() => [.._dialog.Notes.Select(it => it.ProgressDescription)];

   string?[] TheVoiceOfEverySentencesTtsAudio() => [.._sentences.Select(it => it.Audio.TextToSpeech.SingleOrDefault()?.TextToSpeechVoice)];

   string[] TheBatchVoicesButtonOfEveryNote() => [.._dialog.Notes.Select(it => it.TheBatchVoicesButton)];

   [XF] public void nothing_is_spoken_as_it_opens() => _provider.Spoken.Must().BeEmpty();
   [XF] public void the_batch_voice_is_the_default_voice() => (_dialog.BatchVoice?.ToString() ?? "").Must().Be("Fake Steady");
   [XF] public void the_run_button_counts_the_notes_and_their_characters() => _dialog.RunTheBatchText.Must().Be("Run batch: 3 notes, 16 characters");
   [XF] public void no_note_shows_any_progress() => TheProgressOfEveryNote().Must().SequenceEqual(["", "", ""]);
   [XF] public void every_notes_button_is_generate() => TheBatchVoicesButtonOfEveryNote().Must().SequenceEqual(["Generate", "Generate", "Generate"]);

   public class after_selecting_a_note_without_audio : When_generating_tts_audio_for_many_notes
   {
      public after_selecting_a_note_without_audio() => _dialog.SelectedNote = Note(0);

      [XF] public void nothing_is_spoken_and_nothing_plays() => (_provider.Spoken.Count, _player.Played.Count).Must().Be((0, 0));
   }

   public class after_trying_the_batch_voice_on_the_first_note : When_generating_tts_audio_for_many_notes
   {
      public after_trying_the_batch_voice_on_the_first_note()
      {
         _dialog.SelectedNote = Note(0);
         _dialog.TryTheBatchVoiceOnTheSelectedNote();
      }

      [XF] public void the_batch_voice_spoke_its_text() => _provider.Spoken.Must().SequenceEqual(["Steady: 家に帰る"]);
      [XF] public void the_speech_is_an_attempt_in_the_notes_own_text_to_speech() => Note(0).TextToSpeech.Attempts.Select(it => it.Description).Must().SequenceEqual(["#1 Fake Steady"]);
      [XF] public void the_attempt_is_played() => _player.Played.Single().Must().SequenceEqual(Note(0).TextToSpeech.Attempts.Single().Speech.Mp3);
      [XF] public void the_note_is_unchanged() => _sentences[0].AllAudio.InTheOrderPreferred.Must().BeEmpty();
      [XF] public void the_notes_button_plays_its_speech_by_the_voice() => TheBatchVoicesButtonOfEveryNote().Must().SequenceEqual(["▶", "Generate", "Generate"]);

      public class and_choosing_the_varying_voice_for_the_batch : after_trying_the_batch_voice_on_the_first_note
      {
         public and_choosing_the_varying_voice_for_the_batch() => _dialog.BatchVoice = _provider.Varying;

         [XF] public void every_notes_button_is_generate_since_none_has_a_speech_by_that_voice() =>
            TheBatchVoicesButtonOfEveryNote().Must().SequenceEqual(["Generate", "Generate", "Generate"]);
      }

      public class and_trying_it_on_the_first_note_again : after_trying_the_batch_voice_on_the_first_note
      {
         public and_trying_it_on_the_first_note_again() => _dialog.TryTheBatchVoiceOn(Note(0));

         [XF] public void nothing_is_spoken_again() => _provider.Spoken.Count.Must().Be(1);
         [XF] public void the_attempt_is_played_again() =>
            (_player.Played.Count, _player.Played.Last().AsSpan().SequenceEqual(Note(0).TextToSpeech.Attempts.Single().Speech.Mp3)).Must().Be((2, true));
      }

      public class and_trying_it_on_the_third_note_from_its_row : after_trying_the_batch_voice_on_the_first_note
      {
         public and_trying_it_on_the_third_note_from_its_row() => _dialog.TryTheBatchVoiceOn(Note(2));

         [XF] public void the_third_note_is_selected() => (_dialog.SelectedNote == Note(2)).Must().BeTrue();
         [XF] public void the_batch_voice_spoke_the_third_notes_text_too() => _provider.Spoken.Must().SequenceEqual(["Steady: 家に帰る", "Steady: 何でもない"]);
      }

      public class and_running_the_batch : after_trying_the_batch_voice_on_the_first_note
      {
         public and_running_the_batch() => _dialog.RunTheBatchAsync().GetAwaiter().GetResult();

         [XF] public void the_batch_voice_spoke_only_the_texts_of_the_notes_it_was_not_tried_on() =>
            _provider.Spoken.Must().SequenceEqual(["Steady: 家に帰る", "Steady: 明日学校に行く", "Steady: 何でもない"]);

         [XF] public void every_note_has_tts_audio_of_the_batch_voice() => TheVoiceOfEverySentencesTtsAudio().Must().SequenceEqual(["Fake ja-JP-Steady", "Fake ja-JP-Steady", "Fake ja-JP-Steady"]);
         [XF] public void every_note_says_the_batch_voice_was_used() => TheProgressOfEveryNote().Must().SequenceEqual(["used Fake Steady", "used Fake Steady", "used Fake Steady"]);
         [XF] public void the_summary_says_every_note_has_been_given_audio() => _dialog.Summary.Must().Be("3 of 3 notes have been given audio");
         [XF] public void no_note_is_left_for_the_batch() => (_dialog.RunTheBatchText, _dialog.CanRunTheBatch).Must().Be(("Run batch: 0 notes, 0 characters", false));
         [XF] public void the_tried_notes_own_text_to_speech_has_the_used_audio_as_its_current_audio() => Note(0).TextToSpeech.CurrentAudio.CanPlay.Must().BeTrue();
         [XF] public void it_is_no_longer_running() => _dialog.IsRunning.Must().BeFalse();
         [XF] public void every_notes_button_plays_its_speech() => TheBatchVoicesButtonOfEveryNote().Must().SequenceEqual(["▶", "▶", "▶"]);

         public class and_trying_the_batch_voice_on_the_second_note : and_running_the_batch
         {
            public and_trying_the_batch_voice_on_the_second_note() => _dialog.TryTheBatchVoiceOn(Note(1));

            [XF] public void nothing_is_spoken_since_its_tts_audio_is_of_the_voice() => _provider.Spoken.Count.Must().Be(3);
            [XF] public void its_tts_audio_is_played() => _player.Played.Last().Must().SequenceEqual(Note(0).TextToSpeech.Attempts.Single().Speech.Mp3);
         }

         public class and_discarding_the_tts_audio_of_the_second_note : and_running_the_batch
         {
            readonly string _itsFile;

            public and_discarding_the_tts_audio_of_the_second_note()
            {
               _itsFile = _sentences[1].PreferredAudio?.FilePath ?? throw new InvalidOperationException("The batch gave the second sentence no audio.");
               _dialog.SelectedNote = Note(1);
               Note(1).TextToSpeech.DiscardTheTextToSpeechAudio();
            }

            [XF] public void its_file_is_deleted_from_the_media_layer() => File.Exists(_itsFile).Must().BeFalse();
            [XF] public void its_button_is_generate_again() => Note(1).TheBatchVoicesButton.Must().Be("Generate");

            [XF] public void the_note_has_no_tts_audio() => TheVoiceOfEverySentencesTtsAudio().Must().SequenceEqual(["Fake ja-JP-Steady", null, "Fake ja-JP-Steady"]);
            [XF] public void the_list_says_it_was_discarded() => (Note(1).ProgressDescription, Note(1).CurrentAudioDescription, Note(1).CurrentAudio.CanPlay).Must().Be(("TTS audio discarded", "none", false));
            [XF] public void the_batch_leaves_it() => _dialog.RunTheBatchText.Must().Be("Run batch: 0 notes, 0 characters");
            [XF] public void there_is_nothing_more_to_discard() => Note(1).TextToSpeech.CanDiscardTheTextToSpeechAudio.Must().BeFalse();
         }

         public class and_running_the_batch_again_in_a_dialog_opened_again : and_running_the_batch
         {
            readonly BatchTextToSpeechDialogViewModel _dialogOpenedAgain;

            public and_running_the_batch_again_in_a_dialog_opened_again()
            {
               _dialogOpenedAgain = new BatchTextToSpeechDialogViewModel(_sentences, [_provider.Steady, _provider.Varying], _player, GetService<TemporaryServiceCollection>());
               _dialogOpenedAgain.RunTheBatchAsync().GetAwaiter().GetResult();
            }

            [XF] public void nothing_is_spoken_since_every_note_has_tts_audio_of_the_voice() => _provider.Spoken.Count.Must().Be(3);
            [XF] public void every_note_says_it_had_the_voice_already() =>
               _dialogOpenedAgain.Notes.Select(it => it.ProgressDescription).Must().SequenceEqual(Enumerable.Repeat("had Fake Steady already", 3));
         }

         public class and_selecting_the_second_note : and_running_the_batch
         {
            public and_selecting_the_second_note() => _dialog.SelectedNote = Note(1);

            [XF] public void its_new_tts_audio_plays() => _player.Played.Last().Must().SequenceEqual(Note(0).TextToSpeech.Attempts.Single().Speech.Mp3);
            [XF] public void nothing_is_spoken() => _provider.Spoken.Count.Must().Be(3);
         }

         public class and_using_an_attempt_of_the_varying_voice_on_the_second_note : and_running_the_batch
         {
            public and_using_an_attempt_of_the_varying_voice_on_the_second_note()
            {
               _dialog.SelectedNote = Note(1);
               Note(1).TextToSpeech.GenerateWith(_provider.Varying);
               _dialog.UseTheSelectedNotesAttempt();
            }

            [XF] public void it_replaces_the_tts_audio_of_the_batch_voice() => TheVoiceOfEverySentencesTtsAudio().Must().SequenceEqual(["Fake ja-JP-Steady", "Fake ja-JP-Varying", "Fake ja-JP-Steady"]);
            [XF] public void the_list_shows_the_voice_used() => (Note(1).ProgressDescription, Note(1).CurrentAudioDescription).Must().Be(("used Fake Varying", "Fake ja-JP-Varying"));
         }
      }
   }

   public class after_running_the_batch_with_a_voice_the_service_refuses : When_generating_tts_audio_for_many_notes
   {
      public after_running_the_batch_with_a_voice_the_service_refuses()
      {
         _dialog.BatchVoice = _provider.Refused;
         _dialog.RunTheBatchAsync().GetAwaiter().GetResult();
      }

      [XF] public void every_note_failed_and_says_why() =>
         TheProgressOfEveryNote().Must().SequenceEqual(Enumerable.Repeat("failed: Fake refused the request: 400 Bad Request.", 3));

      [XF] public void no_note_was_given_audio() => TheVoiceOfEverySentencesTtsAudio().Must().SequenceEqual([null, null, null]);
      [XF] public void the_summary_counts_the_failures() => _dialog.Summary.Must().Be("0 of 3 notes have been given audio, 3 failed");
      [XF] public void running_the_batch_again_would_try_them_again() => (_dialog.RunTheBatchText, _dialog.CanRunTheBatch).Must().Be(("Run batch: 3 notes, 16 characters", true));
   }
}
