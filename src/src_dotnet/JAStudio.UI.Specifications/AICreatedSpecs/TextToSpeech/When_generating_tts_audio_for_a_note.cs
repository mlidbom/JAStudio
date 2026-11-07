using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.Note.Sentences;
using JAStudio.TextToSpeech;
using JAStudio.UI.ViewModels.TextToSpeech;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.TextToSpeech;

/// <summary>
/// The TTS dialog for a sentence, 家に帰る, offering the fake tts-provider's Steady voice, the default, and its Varying voice.<br/>
/// It's opened as the TTS dialog opens: with the default voice speaking at once. Nothing plays: a recording player stands in.
/// </summary>
public class When_generating_tts_audio_for_a_note : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly FakeTextToSpeechProvider _provider = new();
   readonly RecordingAudioPlayer _player = new();
   readonly SentenceNote _sentence;
   readonly TextToSpeechDialogViewModel _dialog;

   public When_generating_tts_audio_for_a_note()
   {
      _sentence = CreateTestSentence("家に帰る", "Go home");
      _dialog = DialogOffering(_provider.Steady, _provider.Varying);
      _dialog.GenerateWithTheDefaultVoice();
   }

   TextToSpeechDialogViewModel DialogOffering(params TextToSpeechVoice[] voices) => new(_sentence, voices, _player, GetService<TemporaryServiceCollection>());

   SpeechAttemptViewModel Attempt(int number) => _dialog.Attempts.Single(it => it.Number == number);

   string[] TheAttempts() => [.._dialog.Attempts.Select(it => $"{it.Description} {it.Comparison}".Trim())];

   [XF] public void the_text_to_speak_is_the_notes() => _dialog.TextToSpeak.Must().Be(_sentence.TextToSpeak);
   [XF] public void the_default_voice_spoke_it_as_the_dialog_opened() => _provider.Spoken.Must().SequenceEqual(["Steady: 家に帰る"]);
   [XF] public void its_speech_is_the_first_attempt() => TheAttempts().Must().SequenceEqual(["#1 Fake Steady"]);
   [XF] public void the_new_attempt_is_selected_and_played() => (_dialog.SelectedAttempt == Attempt(1), _player.Played.Single() == Attempt(1).Speech.Mp3).Must().Be((true, true));
   [XF] public void the_attempt_says_how_long_it_lasts() => Attempt(1).Length.Must().Be("1.00 s");
   [XF] public void the_note_has_no_tts_audio_yet() => (_dialog.CurrentAudio, _dialog.CanPlayTheCurrentAudio).Must().Be(("none", false));
   [XF] public void the_note_is_unchanged() => _sentence.Audio.RawValue().Must().Be("");

   public class after_generating_with_the_default_voice_again : When_generating_tts_audio_for_a_note
   {
      public after_generating_with_the_default_voice_again() => _dialog.GenerateWith(_provider.Steady);

      [XF] public void the_new_attempt_says_it_sounds_the_same_as_the_first() => TheAttempts().Must().SequenceEqual(["#1 Fake Steady", "#2 Fake Steady same as #1"]);
   }

   public class after_generating_with_the_varying_voice_twice : When_generating_tts_audio_for_a_note
   {
      public after_generating_with_the_varying_voice_twice()
      {
         _dialog.GenerateWith(_provider.Varying);
         _dialog.GenerateWith(_provider.Varying);
      }

      [XF] public void only_the_second_is_compared_since_the_first_is_the_first_with_its_voice() =>
         TheAttempts().Must().SequenceEqual(["#1 Fake Steady", "#2 Fake Varying", "#3 Fake Varying differs from #2: 0.25 s longer"]);

      public class and_playing_attempt_1_by_its_number : after_generating_with_the_varying_voice_twice
      {
         public and_playing_attempt_1_by_its_number() => _dialog.PlayAttempt(1);

         [XF] public void attempt_1_is_selected_and_played_again() => (_dialog.SelectedAttempt == Attempt(1), _player.Played.Last() == Attempt(1).Speech.Mp3).Must().Be((true, true));
         [XF] public void nothing_was_spoken_again() => _provider.Spoken.Count.Must().Be(3);
      }
   }

   public class after_editing_the_text_and_generating_with_the_default_voice : When_generating_tts_audio_for_a_note
   {
      public after_editing_the_text_and_generating_with_the_default_voice()
      {
         _dialog.TextToSpeak = "いえにかえる";
         _dialog.GenerateWith(_provider.Steady);
      }

      [XF] public void the_edited_text_is_spoken() => _provider.Spoken.Last().Must().Be("Steady: いえにかえる");
      [XF] public void the_new_attempt_is_not_compared_with_one_of_another_text() => TheAttempts().Must().SequenceEqual(["#1 Fake Steady", "#2 Fake Steady"]);
      [XF] public void the_notes_question_is_unchanged() => _sentence.GetQuestion().Must().Be("家に帰る");
   }

   public class after_generating_with_a_voice_the_service_refuses : When_generating_tts_audio_for_a_note
   {
      public after_generating_with_a_voice_the_service_refuses() => _dialog.GenerateWith(_provider.Refused);

      [XF] public void the_dialog_says_why() => _dialog.Status.Must().Be("Fake Refused couldn't speak: Fake refused the request: 400 Bad Request.");
      [XF] public void no_attempt_is_added() => TheAttempts().Must().SequenceEqual(["#1 Fake Steady"]);
      [XF] public void it_is_no_longer_generating() => _dialog.IsGenerating.Must().BeFalse();
   }

   public class after_keeping_attempt_1_when_attempt_2_was_selected : When_generating_tts_audio_for_a_note
   {
      readonly string _fileName;

      public after_keeping_attempt_1_when_attempt_2_was_selected()
      {
         _dialog.GenerateWith(_provider.Varying);
         _dialog.PlayAttempt(1);
         _fileName = _dialog.KeepTheSelectedAttempt();
      }

      string KeptFile => Path.Combine(GetService<IEnvironmentPaths>().MediaDir, "free", "TTS", "sentences", "jastudio", _fileName);

      [XF] public void the_file_is_named_after_the_voice_and_made_unique() => Regex.IsMatch(_fileName, "^jastudio-tts-fake-steady-[0-9a-f]{32}\\.mp3$").Must().BeTrue();
      [XF] public void the_notes_audio_references_it() => _sentence.Audio.RawValue().Must().Be($"[sound:{_fileName}]");
      [XF] public void it_is_in_the_media_layers_folder_for_the_tts_audio_of_sentences() => File.ReadAllBytes(KeptFile).SequenceEqual(Attempt(1).Speech.Mp3).Must().BeTrue();
      [XF] public void the_note_finds_it_in_the_media_layer() => (_sentence.PreferredAudio?.FilePath ?? "").Must().Be(KeptFile);
   }

   public class with_no_voices_chosen : When_generating_tts_audio_for_a_note
   {
      readonly TextToSpeechDialogViewModel _dialogWithoutVoices;

      public with_no_voices_chosen()
      {
         _dialogWithoutVoices = DialogOffering();
         _dialogWithoutVoices.GenerateWithTheDefaultVoice();
      }

      [XF] public void the_dialog_says_to_choose_them() => _dialogWithoutVoices.Status.Must().Be("No voices are chosen yet: choose them with Voices...");
      [XF] public void nothing_is_generated() => _dialogWithoutVoices.Attempts.Must().BeEmpty();
   }
}
