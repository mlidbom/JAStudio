using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JAStudio.TextToSpeech;
using JAStudio.UI.ViewModels.TextToSpeech;

namespace JAStudio.UI.Views.TextToSpeech;

/// <summary>
/// Text-to-speech for one note, showing a <see cref="TextToSpeechForANoteViewModel"/>: the text to speak, the note's TTS audio now, a button per chosen voice,<br/>
/// the attempts, and Use. The TTS dialog shows it (<see cref="TextToSpeechDialog"/>), and the batch TTS dialog shows it for its selected note<br/>
/// (<see cref="BatchTextToSpeechDialog"/>). The window showing it handles the keys, and choosing the voices (<see cref="ChoosingTheVoicesAsked"/>).
/// </summary>
/// <remarks>Its buttons don't take the keyboard, which stays on the attempts, or on the batch TTS dialog's notes, so that Enter uses the selected attempt.</remarks>
partial class TextToSpeechForANoteView : UserControl
{
   public TextToSpeechForANoteView() => InitializeComponent();

   /// <summary>Raised by "Voices...": the window showing this chooses the voices again, and offers them from then on.</summary>
   public event Action? ChoosingTheVoicesAsked;

   /// <summary>Whether the keyboard is in the text to speak, where the window's keys are typed rather than acted on.</summary>
   public bool IsTheKeyboardInTheTextToSpeak => TextToSpeakBox.IsKeyboardFocusWithin;

   /// <summary>Puts the keyboard on the attempts.</summary>
   public void PutTheKeyboardOnTheAttempts() => AttemptList.Focus();

   TextToSpeechForANoteViewModel ForTheNote => DataContext as TextToSpeechForANoteViewModel ?? throw new InvalidOperationException("Text-to-speech for a note is shown without a TextToSpeechForANoteViewModel.");

   void OnPlayTheCurrentAudio(object? sender, RoutedEventArgs e) => ForTheNote.PlayTheCurrentAudio();

   void OnPlayAnAttempt(object? sender, RoutedEventArgs e)
   {
      if(sender is not Control { DataContext: SpeechAttemptViewModel attempt })
         throw new InvalidOperationException("An attempt's ▶ shows something other than a SpeechAttemptViewModel.");

      ForTheNote.Play(attempt);
   }

   void OnGenerateWithAVoice(object? sender, RoutedEventArgs e)
   {
      if(sender is not Control { DataContext: TextToSpeechVoice voice })
         throw new InvalidOperationException("A voice's button shows something other than a TextToSpeechVoice.");

      ForTheNote.GenerateWith(voice);
   }

   void OnGenerateWithEveryVoice(object? sender, RoutedEventArgs e) => ForTheNote.GenerateWithEveryChosenVoice();

   void OnChooseTheVoices(object? sender, RoutedEventArgs e) => ChoosingTheVoicesAsked?.Invoke();

   void OnUse(object? sender, RoutedEventArgs e) => ForTheNote.UseTheSelectedAttempt();
}
