using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.UI.ViewModels.TextToSpeech;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.TextToSpeech;

/// <summary>
/// The dialog for choosing the voices the TTS dialog offers, with the fake tts-provider, whose voices are Steady, Varying and Refused,<br/>
/// and settings that have Varying chosen. It's opened as the dialog opens: with the voices listed.
/// </summary>
public class When_choosing_the_tts_voices : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly FakeTextToSpeechProvider _provider = new();
   readonly RecordingAudioPlayer _player = new();
   readonly ChosenVoicesDialogViewModel _dialog;

   public When_choosing_the_tts_voices()
   {
      var settings = new TextToSpeechSettings().WithChosenVoices([new ChosenVoice("Fake", "ja-JP-Varying", TextToSpeechVoiceGender.Male)]);
      _dialog = new ChosenVoicesDialogViewModel(settings, [_provider], _player, GetService<IFatalErrorHandler>());
      _dialog.ListTheVoices();
   }

   string[] TheChosen() => [.._dialog.Chosen.Select(it => it.Description)];

   string[] TheAvailable() => [.._dialog.Available.Select(it => it.Description)];

   VoiceChoiceViewModel AvailableVoice(string shortName) => _dialog.Available.Single(it => it.Voice.ShortName == shortName);

   VoiceChoiceViewModel ChosenVoice(string shortName) => _dialog.Chosen.Single(it => it.Voice.ShortName == shortName);

   [XF] public void the_chosen_voice_is_listed_as_chosen() => TheChosen().Must().SequenceEqual(["Fake Varying (Male)"]);
   [XF] public void the_other_voices_are_available_in_the_order_listed() => TheAvailable().Must().SequenceEqual(["Fake Steady (Female)", "Fake Refused (Female)"]);

   public class after_choosing_steady_and_moving_it_up : When_choosing_the_tts_voices
   {
      public after_choosing_steady_and_moving_it_up()
      {
         _dialog.SelectedAvailable = AvailableVoice("Steady");
         _dialog.ChooseTheSelected();
         _dialog.MoveTheSelected(-1);
      }

      [XF] public void it_is_chosen_first() => TheChosen().Must().SequenceEqual(["Fake Steady (Female)", "Fake Varying (Male)"]);
      [XF] public void it_is_no_longer_available() => TheAvailable().Must().SequenceEqual(["Fake Refused (Female)"]);
      [XF] public void the_settings_chosen_have_it_first_which_makes_it_the_default() =>
         _dialog.TheSettingsChosen().ChosenVoices.Select(it => it.Name).Must().SequenceEqual(["ja-JP-Steady", "ja-JP-Varying"]);

      public class and_removing_it_again : after_choosing_steady_and_moving_it_up
      {
         public and_removing_it_again()
         {
            _dialog.SelectedChosen = ChosenVoice("Steady");
            _dialog.UnchooseTheSelected();
         }

         [XF] public void it_is_available_again_where_the_tts_provider_listed_it() => TheAvailable().Must().SequenceEqual(["Fake Steady (Female)", "Fake Refused (Female)"]);
      }
   }

   public class after_playing_a_voices_sample_twice : When_choosing_the_tts_voices
   {
      public after_playing_a_voices_sample_twice()
      {
         _dialog.PlayTheSampleOf(AvailableVoice("Steady"));
         _dialog.PlayTheSampleOf(AvailableVoice("Steady"));
      }

      [XF] public void the_sample_is_spoken_once() => _provider.Spoken.Must().SequenceEqual(["Steady: 明日は学校に行きます。"]);
      [XF] public void it_is_played_both_times() => _player.Played.Count.Must().Be(2);
   }
}
