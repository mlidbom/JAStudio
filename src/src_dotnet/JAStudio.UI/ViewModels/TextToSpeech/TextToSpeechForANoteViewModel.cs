using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.Audio;
using JAStudio.UI.Views.TextToSpeech;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// Text-to-speech for one note: generating its TTS audio with the chosen voices, listening to the attempts, as often as wanted and without generating<br/>
/// them again, and using one, which is the only step that changes the note. The TTS dialog shows it (<see cref="TextToSpeechDialog"/>), and so does<br/>
/// the batch TTS dialog, for the note selected in it (<see cref="BatchTextToSpeechDialogViewModel"/>). See <c>dev_docs/text-to-speech.md</c>.
/// </summary>
/// <remarks>
/// The TTS dialog has the default voice, the first chosen, speak as soon as it opens (<see cref="GenerateWithTheDefaultVoice"/>). Each new attempt says whether<br/>
/// it sounds the same as the attempt before it with the same voice and text, and as the note's <see cref="CurrentAudio"/> as it was when the attempt was made.<br/>
/// The text to speak can be edited, such as to write a word in kana to force its reading.<br/>
/// The attempts live only in memory: closing the dialog without using one changes nothing.
/// </remarks>
partial class TextToSpeechForANoteViewModel : ObservableObject
{
   readonly IHasTextToSpeechAudio _note;
   readonly IAudioPlayer _player;
   readonly UsingASpeech _usingASpeech;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly MediaFileIndex _media;
   int _generationsUnderWay;

   /// <param name="note">The note to generate TTS audio for.</param>
   /// <param name="chosenVoices">The voices to offer, in priority order. The first is the default.</param>
   /// <param name="player">Plays the attempts and the note's current audio.</param>
   /// <param name="services">The application's services.</param>
   public TextToSpeechForANoteViewModel(IHasTextToSpeechAudio note, IReadOnlyList<TextToSpeechVoice> chosenVoices, IAudioPlayer player, TemporaryServiceCollection services)
   {
      _note = note;
      _player = player;
      _usingASpeech = new UsingASpeech(services);
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      _media = services.Resolve<MediaFileIndex>();
      ChosenVoices = chosenVoices;
      TextToSpeak = note.TextToSpeak;
      CurrentAudio = CurrentAudioViewModel.Of(note, _media);
      Status = WhyThereIsNothingToGenerateWith(chosenVoices);
   }

   /// <summary>Raised when one of the attempts is used as the note's TTS audio (<see cref="UseTheSelectedAttempt"/>), with the attempt used.</summary>
   public event Action<SpeechAttemptViewModel>? UsedAnAttempt;

   public string Title => $"Text-to-speech audio for {_note.TextToSpeak}";

   /// <summary>The text the voices speak: the note's text to speak at first, and editable. Editing it changes only what's spoken, not the note.</summary>
   [ObservableProperty] public partial string TextToSpeak { get; set; }

   /// <summary>The voices offered, in priority order. The first is the default.</summary>
   [ObservableProperty] public partial IReadOnlyList<TextToSpeechVoice> ChosenVoices { get; set; }

   /// <summary>Offers <paramref name="chosenVoices"/> from now on, as after choosing the voices again from the dialog. The attempts stay.</summary>
   internal void Offer(IReadOnlyList<TextToSpeechVoice> chosenVoices)
   {
      ChosenVoices = chosenVoices;
      Status = WhyThereIsNothingToGenerateWith(chosenVoices);
   }

   static string WhyThereIsNothingToGenerateWith(IReadOnlyList<TextToSpeechVoice> chosenVoices) => chosenVoices.Count == 0 ? "No voices are chosen yet: choose them with Voices..." : "";

   /// <summary>The attempts, in the order generated.</summary>
   public ObservableCollection<SpeechAttemptViewModel> Attempts { get; } = [];

   /// <summary>The selected attempt, which the "Use" button uses. Selecting one plays it.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(CanUse), nameof(UseText))]
   public partial SpeechAttemptViewModel? SelectedAttempt { get; set; }

   /// <summary>What went wrong with the last generation, or why there's nothing to generate with. Empty when all is well.</summary>
   [ObservableProperty] public partial string Status { get; set; }

   /// <summary>Whether a voice is speaking the text right now. Several may be at once.</summary>
   [ObservableProperty] public partial bool IsGenerating { get; set; }

   /// <summary>
   /// The note's TTS audio as it is now, to listen to and to compare new attempts with: as it was when the dialog opened, until an attempt<br/>
   /// is used, or the batch TTS dialog uses a speech for the note (<see cref="ReadTheCurrentAudioAgain"/>).
   /// </summary>
   [ObservableProperty] public partial CurrentAudioViewModel CurrentAudio { get; private set; }

   /// <summary>Reads the note's TTS audio again into <see cref="CurrentAudio"/>, as after a speech was used for it.</summary>
   internal void ReadTheCurrentAudioAgain() => CurrentAudio = CurrentAudioViewModel.Of(_note, _media);

   /// <summary>The latest attempt with <paramref name="voice"/> speaking the <see cref="TextToSpeak"/> as it is now, or null when there's none.</summary>
   internal SpeechAttemptViewModel? LatestAttemptWith(TextToSpeechVoice voice) => Attempts.LastOrDefault(it => it.IsOf(voice, TextToSpeak));

   public bool CanUse => SelectedAttempt is not null;

   public string UseText => SelectedAttempt is {} selected ? $"Use #{selected.Number}" : "Use";

   partial void OnSelectedAttemptChanged(SpeechAttemptViewModel? value)
   {
      if(value is not null)
         _player.Play(value.Speech.Mp3);
   }

   /// <summary>Plays the note's <see cref="CurrentAudio"/>. Only when it <see cref="CurrentAudioViewModel.CanPlay"/>.</summary>
   internal void PlayTheCurrentAudio() => CurrentAudio.PlayWith(_player);

   /// <summary>Plays the attempt numbered <paramref name="number"/>, and selects it. Does nothing when there's no such attempt.</summary>
   internal void PlayTheAttemptNumbered(int number)
   {
      if(Attempts.FirstOrDefault(it => it.Number == number) is {} attempt)
         Play(attempt);
   }

   /// <summary>
   /// Plays <paramref name="attempt"/>, and selects it. It plays even when it's selected already, as when replaying it.<br/>
   /// Selecting an attempt plays it, so one not selected yet is played by selecting it.
   /// </summary>
   internal void Play(SpeechAttemptViewModel attempt)
   {
      if(attempt == SelectedAttempt) _player.Play(attempt.Speech.Mp3);
      else SelectedAttempt = attempt;
   }

   /// <summary>Has the default voice, the first chosen, speak the text. The dialog does this as soon as it opens. Does nothing with no voices chosen.</summary>
   internal void GenerateWithTheDefaultVoice()
   {
      if(ChosenVoices is [var defaultVoice, ..])
         GenerateWith(defaultVoice);
   }

   /// <summary>Has each chosen voice speak the text, all at once.</summary>
   internal void GenerateWithEveryChosenVoice()
   {
      foreach(var voice in ChosenVoices)
         GenerateWith(voice);
   }

   /// <summary>
   /// Has <paramref name="voice"/> speak the text, in the background, and adds the result as a new attempt, selected, so that it plays.<br/>
   /// A request the tts-provider refuses, or that doesn't reach it, is reported in the <see cref="Status"/>, and nothing is added.
   /// </summary>
   internal void GenerateWith(TextToSpeechVoice voice) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         var text = TextToSpeak;
         StartedGenerating();
         try
         {
            AddTheAttempt(await voice.SpeakAsync(text, CancellationToken.None));
            Status = "";
         }
         // A refused request, such as when the free characters are used up, and a lost connection are conditions of the service,
         // not bugs: they're reported, and generating can be tried again.
         catch(Exception failure) when(failure is TextToSpeechRequestFailedException or HttpRequestException)
         {
            Status = $"{voice} couldn't speak: {failure.Message}";
         }
         finally
         {
            FinishedGenerating();
         }
      });

   /// <summary>
   /// Uses the selected attempt as the note's TTS audio (<see cref="UsingASpeech"/>), which then becomes the <see cref="CurrentAudio"/>, and raises<br/>
   /// <see cref="UsedAnAttempt"/>. Only when <see cref="CanUse"/>.
   /// </summary>
   /// <returns>The name of the file it's stored in.</returns>
   internal string UseTheSelectedAttempt()
   {
      var attempt = SelectedAttempt ?? throw new InvalidOperationException("No attempt is selected to use.");
      var fileName = _usingASpeech.Use(_note, attempt.Speech);
      ReadTheCurrentAudioAgain();
      UsedAnAttempt?.Invoke(attempt);
      return fileName;
   }

   void AddTheAttempt(SynthesizedSpeech speech)
   {
      var earlier = Attempts.LastOrDefault(it => it.IsOf(speech.Voice, speech.Text));
      var attempt = new SpeechAttemptViewModel(Attempts.Count + 1, speech, earlier, CurrentAudio);
      Attempts.Add(attempt);
      SelectedAttempt = attempt;
   }

   void StartedGenerating()
   {
      _generationsUnderWay++;
      IsGenerating = true;
   }

   void FinishedGenerating()
   {
      _generationsUnderWay--;
      IsGenerating = _generationsUnderWay > 0;
   }
}
