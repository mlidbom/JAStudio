using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.Audio;
using JAStudio.UI.Views.TextToSpeech;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// The dialog for choosing the voices the TTS dialog offers (<see cref="ChosenVoicesDialog"/>): every voice of every tts-provider with a key,<br/>
/// the chosen ones in priority order, the first being the default, and a sample of any voice to listen to while choosing.
/// Saving writes the order into the <see cref="TextToSpeechSettings"/>.
/// </summary>
/// <remarks>The tts-providers are asked for their voices when the dialog opens. A voice's sample is generated once, and replayed from memory after that.</remarks>
partial class ChosenVoicesDialogViewModel : ObservableObject
{
   /// <summary>What every voice says as its sample.</summary>
   const string SampleText = "明日は学校に行きます。";

   readonly TextToSpeechSettings _settings;
   readonly IReadOnlyList<ITextToSpeechProvider> _providers;
   readonly IAudioPlayer _player;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Dictionary<TextToSpeechVoice, SynthesizedSpeech> _samples = [];

   /// <summary>Every voice the tts-providers listed, in the order they listed them, which <see cref="Available"/> keeps.</summary>
   readonly List<TextToSpeechVoice> _listed = [];

   /// <param name="settings">The settings to choose in, with the voices chosen so far.</param>
   /// <param name="providers">The tts-providers to offer the voices of.</param>
   /// <param name="player">Plays the samples.</param>
   /// <param name="fatalErrorHandler">Reports a failure no one expected.</param>
   public ChosenVoicesDialogViewModel(TextToSpeechSettings settings, IReadOnlyList<ITextToSpeechProvider> providers, IAudioPlayer player, IFatalErrorHandler fatalErrorHandler)
   {
      _settings = settings;
      _providers = providers;
      _player = player;
      _fatalErrorHandler = fatalErrorHandler;
      Chosen = [..settings.ChosenVoicesOf(providers).Select(it => new VoiceChoiceViewModel(it))];
      _status = providers.Count == 0 ? $"No tts-provider has a key: put them in {TextToSpeechSettings.FileInTheUserProfile}." : "";
   }

   /// <summary>The chosen voices, in priority order. The first is the default.</summary>
   public ObservableCollection<VoiceChoiceViewModel> Chosen { get; }

   /// <summary>The voices not chosen, of every tts-provider with a key, in the order the tts-providers list them.</summary>
   public ObservableCollection<VoiceChoiceViewModel> Available { get; } = [];

   [ObservableProperty] VoiceChoiceViewModel? _selectedChosen;

   [ObservableProperty] VoiceChoiceViewModel? _selectedAvailable;

   /// <summary>What went wrong, such as a tts-provider refusing to list its voices. Empty when all is well.</summary>
   [ObservableProperty] string _status;

   /// <summary>Asks every tts-provider for its voices, and offers those not chosen. The dialog does this as it opens.</summary>
   internal void ListTheVoices() =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         foreach(var provider in _providers)
         {
            try
            {
               foreach(var voice in await provider.JapaneseVoicesAsync(CancellationToken.None))
               {
                  _listed.Add(voice);
                  if(Chosen.All(it => it.Voice != voice))
                     Available.Add(new VoiceChoiceViewModel(voice));
               }
            }
            // A refused request and a lost connection are conditions of the service, not bugs: they're reported, and the other tts-providers still listed.
            catch(Exception failure) when(failure is TextToSpeechRequestFailedException or HttpRequestException)
            {
               Status = $"{provider.Name} couldn't list its voices: {failure.Message}";
            }
         }
      });

   /// <summary>Moves the selected available voice to the end of the chosen ones.</summary>
   internal void ChooseTheSelected()
   {
      if(SelectedAvailable is not {} voice) return;

      Available.Remove(voice);
      Chosen.Add(voice);
      SelectedChosen = voice;
   }

   /// <summary>Moves the selected chosen voice back to the available ones, where its tts-provider listed it.</summary>
   internal void UnchooseTheSelected()
   {
      if(SelectedChosen is not {} voice) return;

      Chosen.Remove(voice);
      Available.Insert(Available.Count(it => _listed.IndexOf(it.Voice) < _listed.IndexOf(voice.Voice)), voice);
      SelectedAvailable = voice;
   }

   /// <summary>Moves the selected chosen voice one place up, or down with <paramref name="steps"/> 1, staying selected.</summary>
   internal void MoveTheSelected(int steps)
   {
      if(SelectedChosen is not {} voice) return;

      var to = Math.Clamp(Chosen.IndexOf(voice) + steps, 0, Chosen.Count - 1);
      Chosen.Move(Chosen.IndexOf(voice), to);
      SelectedChosen = voice;
   }

   /// <summary>Plays <paramref name="voice"/>'s sample, generating it the first time.</summary>
   internal void PlayTheSampleOf(VoiceChoiceViewModel voice) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         try
         {
            if(!_samples.TryGetValue(voice.Voice, out var sample))
               _samples[voice.Voice] = sample = await voice.Voice.SpeakAsync(SampleText, CancellationToken.None);
            _player.Play(sample.Mp3);
         }
         // As when listing the voices: a condition of the service, reported.
         catch(Exception failure) when(failure is TextToSpeechRequestFailedException or HttpRequestException)
         {
            Status = $"{voice.Voice} couldn't speak: {failure.Message}";
         }
      });

   /// <summary>The settings with the chosen voices in their order, for the dialog to save when it's accepted.</summary>
   internal TextToSpeechSettings TheSettingsChosen() => _settings.WithChosenVoices([..Chosen.Select(it => ChosenVoice.Of(it.Voice))]);
}
