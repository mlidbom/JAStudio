using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
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
   readonly SentenceNote[] _sentences;
   readonly BatchTextToSpeechDialogViewModel _dialog;

   public When_generating_tts_audio_for_many_notes()
   {
      _sentences = [CreateTestSentence("家に帰る", "Go home"), CreateTestSentence("明日学校に行く", "I'll go to school tomorrow"), CreateTestSentence("何でもない", "It's nothing")];
      _dialog = new BatchTextToSpeechDialogViewModel(_sentences, [_provider.Steady, _provider.Varying], _player, GetService<TemporaryServiceCollection>());
   }

   NoteInTheBatchViewModel Note(int index) => _dialog.Notes[index];

   string[] TheProgressOfEveryNote() => [.._dialog.Notes.Select(it => it.ProgressDescription)];

   string?[] TheVoiceOfEverySentencesTtsAudio() => [.._sentences.Select(it => it.Audio.TextToSpeech.SingleOrDefault()?.TextToSpeechVoice)];

   [XF] public void nothing_is_spoken_as_it_opens() => _provider.Spoken.Must().BeEmpty();
   [XF] public void the_batch_voice_is_the_default_voice() => (_dialog.BatchVoice?.ToString() ?? "").Must().Be("Fake Steady");
   [XF] public void the_run_button_counts_the_notes_and_their_characters() => _dialog.RunTheBatchText.Must().Be("Run batch: 3 notes, 16 characters");
   [XF] public void no_note_shows_any_progress() => TheProgressOfEveryNote().Must().SequenceEqual(["", "", ""]);

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
