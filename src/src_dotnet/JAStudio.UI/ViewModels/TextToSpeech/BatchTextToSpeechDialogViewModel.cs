using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Audio;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.UI.Views.TextToSpeech;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// The batch TTS dialog (<see cref="BatchTextToSpeechDialog"/>): TTS audio for many notes at once, such as the selected search results. The batch's voice<br/>
/// is tried on a few of the notes, then runs for all of them, using each speech as its note's TTS audio as soon as it arrives. Then the notes are gone<br/>
/// through, each playing its TTS audio as it's selected, and one that sounds wrong is fixed in its own text-to-speech, shown beside the list<br/>
/// (<see cref="NoteInTheBatchViewModel.TextToSpeech"/>). See <c>dev_docs/text-to-speech.md</c>.
/// </summary>
/// <remarks>
/// Unlike the TTS dialog, nothing is spoken when the dialog opens, nor when a note is selected: selecting one plays its TTS audio as it is now.<br/>
/// The batch's voice speaks a note's text only when there's no speech of it by that voice: an attempt tried on the note is used, and TTS audio<br/>
/// it already has of the voice is kept (<see cref="NoteInTheBatchViewModel.TriedAttemptWith"/>, <see cref="NoteInTheBatchViewModel.HasTextToSpeechAudioOf"/>).<br/>
/// The batch makes <see cref="RequestsAtATime"/> requests at a time, and a request refused for the rate is made again after a wait<br/>
/// (<see cref="TextToSpeechVoice.SpeakAsync"/>). Its work on each answer, normalizing and using it, runs where it was started, on the UI thread, as the<br/>
/// TTS dialog's does: normalizing took about 10 ms for a speech of three seconds (2026-11-15).
/// </remarks>
partial class BatchTextToSpeechDialogViewModel : ObservableObject
{
   /// <summary>
   /// How many requests the batch makes at a time: well below Azure's 30 a second, and about Google's 200 Chirp 3 HD requests a minute, at a second<br/>
   /// or so for each answer. Faster than anyone listens to them.
   /// </summary>
   const int RequestsAtATime = 4;

   readonly IAudioPlayer _player;
   readonly UsingASpeech _usingASpeech;
   readonly IFatalErrorHandler _fatalErrorHandler;
   bool _stopIsAsked;

   /// <param name="notes">The notes to give TTS audio.</param>
   /// <param name="chosenVoices">The voices to offer, in priority order. The first is the batch's voice at first.</param>
   /// <param name="player">Plays the notes' TTS audio and the attempts.</param>
   /// <param name="services">The application's services.</param>
   public BatchTextToSpeechDialogViewModel(IReadOnlyList<IHasTextToSpeechAudio> notes, IReadOnlyList<TextToSpeechVoice> chosenVoices, IAudioPlayer player, TemporaryServiceCollection services)
   {
      _player = player;
      _usingASpeech = new UsingASpeech(services);
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      var media = services.Resolve<MediaFileIndex>();
      ChosenVoices = chosenVoices;
      Status = WhyThereIsNothingToGenerateWith(chosenVoices);
      Notes = [..notes.Select(note => new NoteInTheBatchViewModel(note, media, () => new TextToSpeechForANoteViewModel(note, ChosenVoices, player, services)))];
      BatchVoice = chosenVoices.FirstOrDefault();
      foreach(var note in Notes)
         note.PropertyChanged += WhenTheProgressOfANoteChanges;
   }

   public string Title => $"Text-to-speech audio for {Notes.Count} notes";

   /// <summary>The notes, in the order given, as the list shows them.</summary>
   public IReadOnlyList<NoteInTheBatchViewModel> Notes { get; }

   /// <summary>The voices offered, in priority order: as the batch's voice, and in the selected note's own text-to-speech.</summary>
   [ObservableProperty] public partial IReadOnlyList<TextToSpeechVoice> ChosenVoices { get; set; }

   /// <summary>The voice the batch runs with, and that trying a voice on the selected note tries: the first chosen at first. Null with no voices chosen.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(CanRunTheBatch), nameof(CanTryTheBatchVoice))]
   public partial TextToSpeechVoice? BatchVoice { get; set; }

   /// <summary>The note selected in the list, whose own text-to-speech is shown beside it. Selecting one plays its TTS audio as it is now.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(CanTryTheBatchVoice), nameof(CanUseTheSelectedNotesAttempt))]
   public partial NoteInTheBatchViewModel? SelectedNote { get; set; }

   /// <summary>Whether the batch is running: it makes requests until every note it runs for has TTS audio or failed, or until it's stopped.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(CanRunTheBatch))]
   public partial bool IsRunning { get; private set; }

   /// <summary>Why there's nothing to generate with, when no voices are chosen. Empty otherwise.</summary>
   [ObservableProperty] public partial string Status { get; private set; }

   /// <summary>The run button's text: how many notes the batch would run for, and how many characters they have, such as "Run batch: 26 notes, 840 characters".</summary>
   public string RunTheBatchText => $"Run batch: {NotesLeftForTheBatch.Count} notes, {NotesLeftForTheBatch.Sum(it => it.TextToSpeak.Length):N0} characters";

   public bool CanRunTheBatch => !IsRunning && BatchVoice is not null && NotesLeftForTheBatch.Count > 0;

   public bool CanTryTheBatchVoice => BatchVoice is not null && SelectedNote is not null;

   /// <summary>Whether the selected note's own text-to-speech has an attempt selected, to use as the note's TTS audio.</summary>
   public bool CanUseTheSelectedNotesAttempt => SelectedNote?.TextToSpeech.CanUse ?? false;

   /// <summary>How far the dialog has come with the notes, such as "12 of 37 notes have been given audio, 1 failed".</summary>
   public string Summary
   {
      get
      {
         var givenAudio = Notes.Count(it => it.Progress == NoteInTheBatchProgress.GivenAudio);
         var failed = Notes.Count(it => it.Progress == NoteInTheBatchProgress.Failed);
         return $"{givenAudio} of {Notes.Count} notes have been given audio" + (failed > 0 ? $", {failed} failed" : "");
      }
   }

   List<NoteInTheBatchViewModel> NotesLeftForTheBatch => [..Notes.Where(it => it.IsLeftForTheBatch)];

   partial void OnBatchVoiceChanged(TextToSpeechVoice? value)
   {
      foreach(var note in Notes)
         note.TheBatchVoiceIs(value);
   }

   partial void OnSelectedNoteChanged(NoteInTheBatchViewModel? value)
   {
      if(value is { CurrentAudio.CanPlay: true })
         value.CurrentAudio.PlayWith(_player);
   }

   /// <summary>Offers <paramref name="chosenVoices"/> from now on, as after choosing the voices again. The batch's voice stays, if it's still chosen.</summary>
   internal void Offer(IReadOnlyList<TextToSpeechVoice> chosenVoices)
   {
      ChosenVoices = chosenVoices;
      BatchVoice = BatchVoice is {} batchVoice && chosenVoices.Contains(batchVoice) ? batchVoice : chosenVoices.FirstOrDefault();
      Status = WhyThereIsNothingToGenerateWith(chosenVoices);
      foreach(var note in Notes)
         note.Offer(chosenVoices);
   }

   /// <summary>Plays <paramref name="note"/>'s TTS audio as it is now, and selects it. It plays even when it's selected already, as when replaying it.</summary>
   internal void Play(NoteInTheBatchViewModel note)
   {
      if(note == SelectedNote) PlayTheSelectedNotesAudio();
      else SelectedNote = note;
   }

   /// <summary>Plays the selected note's TTS audio as it is now, when there's one to play.</summary>
   internal void PlayTheSelectedNotesAudio()
   {
      if(SelectedNote is { CurrentAudio.CanPlay: true } selected)
         selected.CurrentAudio.PlayWith(_player);
   }

   /// <summary>Tries the batch's voice on the selected note (<see cref="TryTheBatchVoiceOn"/>). Only when <see cref="CanTryTheBatchVoice"/>.</summary>
   internal void TryTheBatchVoiceOnTheSelectedNote() => TryTheBatchVoiceOn(SelectedNote ?? throw new InvalidOperationException("No note is selected to try the voice on."));

   /// <summary>
   /// Selects <paramref name="note"/>, and plays its speech by the batch's voice: the latest attempt with the voice in its own text-to-speech, else its TTS<br/>
   /// audio, when the voice made it. Only when there's neither does the voice speak the note's text, as a new attempt, which plays. Nothing changes.<br/>
   /// Does nothing with no voice chosen.
   /// </summary>
   internal void TryTheBatchVoiceOn(NoteInTheBatchViewModel note)
   {
      if(BatchVoice is not {} voice) return;

      SelectedNote = note;
      if(note.TriedAttemptWith(voice) is {} tried) note.TextToSpeech.Play(tried);
      else if(note.HasTextToSpeechAudioOf(voice)) note.CurrentAudio.PlayWith(_player);
      else note.TextToSpeech.GenerateWith(voice);
   }

   /// <summary>Uses the attempt selected in the selected note's own text-to-speech as its TTS audio. Only when <see cref="CanUseTheSelectedNotesAttempt"/>.</summary>
   internal void UseTheSelectedNotesAttempt() => (SelectedNote ?? throw new InvalidOperationException("No note is selected.")).TextToSpeech.UseTheSelectedAttempt();

   /// <summary>Runs the batch in the background (<see cref="RunTheBatchAsync"/>). Only when <see cref="CanRunTheBatch"/>.</summary>
   internal void StartTheBatch() => _fatalErrorHandler.StartAndHandleExceptionsFrom(RunTheBatchAsync);

   /// <summary>
   /// Gives every note left for the batch TTS audio from the batch's voice, <see cref="RequestsAtATime"/> at a time, in the list's order: the attempt the voice<br/>
   /// was tried with on the note, else the TTS audio it already has of the voice, kept, else a new speech, used as soon as it arrives. A request the<br/>
   /// tts-provider refuses, or that doesn't reach it, fails that note alone. Only when <see cref="CanRunTheBatch"/>.
   /// </summary>
   internal async Task RunTheBatchAsync()
   {
      var voice = BatchVoice ?? throw new InvalidOperationException("The batch has no voice to run with.");
      var waiting = new Queue<NoteInTheBatchViewModel>(NotesLeftForTheBatch);
      foreach(var note in waiting)
         note.WaitsForTheBatch();

      _stopIsAsked = false;
      IsRunning = true;
      try
      {
         await Task.WhenAll(Enumerable.Range(0, RequestsAtATime).Select(_ => GiveAudioToTheWaiting()));
      }
      finally
      {
         foreach(var notReached in waiting)
            notReached.WasNotReached();
         IsRunning = false;
      }

      return;

      async Task GiveAudioToTheWaiting()
      {
         while(!_stopIsAsked && waiting.TryDequeue(out var note))
            await GiveAudioTo(note, voice);
      }
   }

   /// <summary>Stops the running batch: it makes no more requests, and the notes waiting for it are left for the next run. The answers to requests already made are still used.</summary>
   internal void StopTheBatch() => _stopIsAsked = true;

   async Task GiveAudioTo(NoteInTheBatchViewModel note, TextToSpeechVoice voice)
   {
      if(note.TriedAttemptWith(voice) is {} tried)
      {
         note.Use(tried.Speech, _usingASpeech);
         return;
      }

      if(note.HasTextToSpeechAudioOf(voice))
      {
         note.AlreadyHadTheAudioOf(voice);
         return;
      }

      note.IsBeingGenerated();
      try
      {
         note.Use(await voice.SpeakAsync(note.TextToSpeak, CancellationToken.None), _usingASpeech);
      }
      // A refused request, such as one refused for the rate after waiting, and a lost connection are conditions of the service, not bugs:
      // they fail the note, and running the batch again tries it again.
      catch(Exception failure) when(failure is TextToSpeechRequestFailedException or HttpRequestException)
      {
         note.Failed(failure.Message);
      }
   }

   void WhenTheProgressOfANoteChanges(object? sender, PropertyChangedEventArgs e)
   {
      if(e.PropertyName != nameof(NoteInTheBatchViewModel.Progress)) return;

      OnPropertyChanged(nameof(Summary));
      OnPropertyChanged(nameof(RunTheBatchText));
      OnPropertyChanged(nameof(CanRunTheBatch));
   }

   static string WhyThereIsNothingToGenerateWith(IReadOnlyList<TextToSpeechVoice> chosenVoices) => chosenVoices.Count == 0 ? "No voices are chosen yet: choose them with Voices..." : "";
}
