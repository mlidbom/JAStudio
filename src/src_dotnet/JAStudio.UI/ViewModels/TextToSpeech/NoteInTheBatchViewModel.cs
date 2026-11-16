using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.TextToSpeech;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// One note in the batch TTS dialog, a row of its list: its text, its TTS audio now, and how far the batch has come with it (<see cref="Progress"/>).<br/>
/// Selected, it shows its <see cref="TextToSpeech"/>, the text-to-speech for the note alone, to try other voices on it, edit what's spoken, or use another attempt.
/// </summary>
/// <remarks>
/// Its <see cref="TextToSpeech"/> is made the first time it's shown, and kept, with its attempts, while the dialog is open. Most notes are never<br/>
/// selected, and making one reads the note's TTS audio and nothing more: nothing is spoken.
/// </remarks>
partial class NoteInTheBatchViewModel : ObservableObject
{
   readonly MediaFileIndex _media;
   readonly Func<TextToSpeechForANoteViewModel> _makeItsTextToSpeech;
   TextToSpeechForANoteViewModel? _textToSpeech;
   TextToSpeechVoice? _batchVoice;

   /// <param name="note">The note.</param>
   /// <param name="media">The media layer, where its TTS audio is found.</param>
   /// <param name="makeItsTextToSpeech">Makes the text-to-speech for the note alone, offering the voices the dialog offers at the time.</param>
   public NoteInTheBatchViewModel(IHasTextToSpeechAudio note, MediaFileIndex media, Func<TextToSpeechForANoteViewModel> makeItsTextToSpeech)
   {
      _note = note.Reference();
      _media = media;
      _makeItsTextToSpeech = makeItsTextToSpeech;
      CurrentAudio = CurrentAudioViewModel.Of(note, media);
   }

   readonly ITaggregateReference<IHasTextToSpeechAudio> _note;

   /// <summary>The note as it is now.</summary>
   IHasTextToSpeechAudio Note => _note.Current;

   /// <summary>The note's text as TTS speaks it, as the list shows it (<see cref="IHasTextToSpeechAudio.TextToSpeak"/>).</summary>
   public string Text => Note.TextToSpeak;

   /// <summary>The note's TTS audio as it is now, which selecting the note plays.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(CurrentAudioDescription), nameof(HasASpeechByTheBatchVoice), nameof(TheBatchVoicesButton))]
   public partial CurrentAudioViewModel CurrentAudio { get; private set; }

   /// <summary>The note's TTS audio now as the list shows it: the voice that made it, when the note knows it, else what <see cref="CurrentAudio"/> says.</summary>
   public string CurrentAudioDescription => CurrentAudio.CanPlay && Note.Audio.TextToSpeech is [{ TextToSpeechVoice: {} voice }, ..] ? voice : CurrentAudio.Description;

   /// <summary>How far the batch has come with the note.</summary>
   [ObservableProperty] public partial NoteInTheBatchProgress Progress { get; private set; }

   /// <summary>The <see cref="Progress"/> as the list shows it, such as "waiting", "used Azure DaichiNeural", or what the tts-provider said when it refused.</summary>
   [ObservableProperty] public partial string ProgressDescription { get; private set; } = "";

   /// <summary>
   /// Whether the note has a speech by the batch's voice: an attempt with it in its own text-to-speech, or TTS audio it made<br/>
   /// (<see cref="TriedAttemptWith"/>, <see cref="HasTextToSpeechAudioOf"/>). Trying the voice on the note then plays it, and speaks nothing.
   /// </summary>
   public bool HasASpeechByTheBatchVoice => _batchVoice is {} voice && (TriedAttemptWith(voice) is not null || HasTextToSpeechAudioOf(voice));

   /// <summary>The row's button for the batch's voice: ▶ when the note <see cref="HasASpeechByTheBatchVoice"/>, else Generate.</summary>
   public string TheBatchVoicesButton => HasASpeechByTheBatchVoice ? "▶" : "Generate";

   /// <summary>Makes <paramref name="voice"/> the batch's voice, whose speech the row's button plays or generates.</summary>
   internal void TheBatchVoiceIs(TextToSpeechVoice? voice)
   {
      _batchVoice = voice;
      TheSpeechesByTheBatchVoiceMayHaveChanged();
   }

   /// <summary>Whether running the batch does the note: it hasn't been given TTS audio in this dialog, or the batch failed for it.</summary>
   public bool IsLeftForTheBatch => Progress is NoteInTheBatchProgress.NotGivenAudio or NoteInTheBatchProgress.Failed;

   /// <summary>The text-to-speech for this note alone, shown beside the list while the note is selected. Made the first time it's asked for.</summary>
   public TextToSpeechForANoteViewModel TextToSpeech => _textToSpeech ??= ItsTextToSpeech();

   /// <summary>The text the batch has its voice speak for the note: what its <see cref="TextToSpeech"/> speaks, which may be edited, else its <see cref="Text"/>.</summary>
   public string TextToSpeak => _textToSpeech?.TextToSpeak ?? Note.TextToSpeak;

   /// <summary>
   /// The latest attempt in the note's <see cref="TextToSpeech"/> with <paramref name="voice"/> speaking its text as it is now, or null when there's none, as when<br/>
   /// the batch's voice was never tried on the note. The batch uses such an attempt, which was listened to, rather than a new speech.
   /// </summary>
   public SpeechAttemptViewModel? TriedAttemptWith(TextToSpeechVoice voice) => _textToSpeech?.LatestAttemptWith(voice);

   /// <summary>
   /// Whether the note's TTS audio now, playable, was made by <paramref name="voice"/>, speaking the note's text: its text to speak isn't edited.<br/>
   /// Trying the voice then plays it, and the batch leaves it, rather than having the voice speak the text again.
   /// </summary>
   public bool HasTextToSpeechAudioOf(TextToSpeechVoice voice) =>
      CurrentAudio.CanPlay && Note.Audio.TextToSpeech is [{ TextToSpeechVoice: {} madeBy }, ..] && madeBy == voice.NameWithItsProvider && TextToSpeak == Note.TextToSpeak;

   /// <summary>Offers <paramref name="chosenVoices"/> in the note's <see cref="TextToSpeech"/> from now on, as after the voices were chosen again.</summary>
   internal void Offer(IReadOnlyList<TextToSpeechVoice> chosenVoices) => _textToSpeech?.Offer(chosenVoices);

   internal void WaitsForTheBatch() => Show(NoteInTheBatchProgress.Waiting, "waiting");

   internal void IsBeingGenerated() => Show(NoteInTheBatchProgress.Generating, "generating...");

   /// <summary>The batch was stopped before it came to the note.</summary>
   internal void WasNotReached() => Show(NoteInTheBatchProgress.NotGivenAudio, "");

   /// <summary>The batch found the note already had TTS audio of <paramref name="voice"/>, and left it.</summary>
   internal void AlreadyHadTheAudioOf(TextToSpeechVoice voice) => Show(NoteInTheBatchProgress.GivenAudio, $"had {voice} already");

   /// <summary>The batch's request for the note was refused, or didn't reach the tts-provider, as <paramref name="why"/> says.</summary>
   internal void Failed(string why) => Show(NoteInTheBatchProgress.Failed, $"failed: {why}");

   /// <summary>Uses <paramref name="speech"/> as the note's TTS audio, through <paramref name="usingASpeech"/>.</summary>
   internal void Use(SynthesizedSpeech speech, UsingASpeech usingASpeech)
   {
      usingASpeech.Use(Note, speech);
      _textToSpeech?.ReadTheCurrentAudioAgain();
      WasGivenTheAudioOf(speech);
   }

   TextToSpeechForANoteViewModel ItsTextToSpeech()
   {
      var textToSpeech = _makeItsTextToSpeech();
      textToSpeech.Attempts.CollectionChanged += (_, _) => TheSpeechesByTheBatchVoiceMayHaveChanged();
      textToSpeech.PropertyChanged += (_, e) =>
      {
         if(e.PropertyName == nameof(TextToSpeechForANoteViewModel.TextToSpeak)) TheSpeechesByTheBatchVoiceMayHaveChanged();
      };
      textToSpeech.UsedAnAttempt += attempt => WasGivenTheAudioOf(attempt.Speech);
      textToSpeech.DiscardedTheTextToSpeechAudio += () =>
      {
         CurrentAudio = CurrentAudioViewModel.Of(Note, _media);
         Show(NoteInTheBatchProgress.AudioDiscarded, "TTS audio discarded");
      };
      return textToSpeech;
   }

   void WasGivenTheAudioOf(SynthesizedSpeech speech)
   {
      CurrentAudio = CurrentAudioViewModel.Of(Note, _media);
      Show(NoteInTheBatchProgress.GivenAudio, $"used {speech.Voice}");
   }

   void TheSpeechesByTheBatchVoiceMayHaveChanged()
   {
      OnPropertyChanged(nameof(HasASpeechByTheBatchVoice));
      OnPropertyChanged(nameof(TheBatchVoicesButton));
   }

   void Show(NoteInTheBatchProgress progress, string description)
   {
      ProgressDescription = description;
      Progress = progress;
   }
}
