using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.Audio;
using JAStudio.UI.ViewModels.TextToSpeech;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.TextToSpeech;

/// <summary>
/// The TTS dialog for one note, showing a <see cref="TextToSpeechDialogViewModel"/>: generate the note's TTS audio with the chosen voices,<br/>
/// listen to the attempts, and use one. It opens where it was last, with the keyboard on the attempts, and the default voice speaking.
/// </summary>
/// <remarks>
/// The keys 1 to 9 play attempts 1 to 9, 0 plays the note's TTS audio as it is now, and Enter uses the selected attempt. They do nothing while<br/>
/// the keyboard is in the text to speak, where they're typed. Using one closes the dialog. Closing it otherwise changes nothing.<br/>
/// It plays through a <see cref="SoundFlowAudioPlayer"/> of its own, closed with the dialog.
/// </remarks>
partial class TextToSpeechDialog : Window
{
   readonly TextToSpeechDialogViewModel _dialog;
   readonly IReadOnlyList<ITextToSpeechProvider> _providers;
   readonly IFatalErrorHandler _fatalErrorHandler;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public TextToSpeechDialog() => InitializeComponent();
#pragma warning restore CS8618

   TextToSpeechDialog(IHasTextToSpeechAudio note, TemporaryServiceCollection services)
   {
      var settings = TextToSpeechSettings.ReadFromTheUserProfile();
      _providers = settings.ProvidersWithKeys();
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      var player = new SoundFlowAudioPlayer();
      _dialog = new TextToSpeechDialogViewModel(note, settings.ChosenVoicesOf(_providers), player, services);
      DataContext = _dialog;
      InitializeComponent();
      KeyDown += OnKeyDown;
      Opened += (_, _) =>
      {
         Dispatcher.UIThread.Post(() => AttemptList.Focus(), DispatcherPriority.Loaded);
         _dialog.GenerateWithTheDefaultVoice();
      };
      Closed += (_, _) => player.Dispose();
   }

   /// <summary>Opens the TTS dialog for <paramref name="note"/>, where the dialog was last.</summary>
   internal static void OpenFor(IHasTextToSpeechAudio note, TemporaryServiceCollection services) => new TextToSpeechDialog(note, services).ShowWhereItWasLast(services);

   void OnKeyDown(object? sender, KeyEventArgs e)
   {
      if(TextToSpeakBox.IsKeyboardFocusWithin) return;

      switch(e.Key)
      {
         case >= Key.D1 and <= Key.D9:
            _dialog.PlayTheAttemptNumbered(e.Key - Key.D0);
            break;
         case Key.D0 when _dialog.CurrentAudio.CanPlay:
            _dialog.PlayTheCurrentAudio();
            break;
         case Key.Enter when _dialog.CanUse:
            Use();
            break;
         default:
            return;
      }

      e.Handled = true;
   }

   void OnPlayTheCurrentAudio(object? sender, RoutedEventArgs e) => _dialog.PlayTheCurrentAudio();

   void OnPlayAnAttempt(object? sender, RoutedEventArgs e)
   {
      if(sender is not Control { DataContext: SpeechAttemptViewModel attempt })
         throw new InvalidOperationException("An attempt's ▶ shows something other than a SpeechAttemptViewModel.");

      _dialog.Play(attempt);
   }

   void OnGenerateWithAVoice(object? sender, RoutedEventArgs e)
   {
      if(sender is not Control { DataContext: TextToSpeechVoice voice })
         throw new InvalidOperationException("A voice's button shows something other than a TextToSpeechVoice.");

      _dialog.GenerateWith(voice);
   }

   void OnGenerateWithEveryVoice(object? sender, RoutedEventArgs e) => _dialog.GenerateWithEveryChosenVoice();

   /// <summary>The voices chosen are offered at once. The dialog keeps its own tts-providers, so that a voice chosen again is still the same voice to compare attempts by.</summary>
   void OnChooseTheVoices(object? sender, RoutedEventArgs e) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await ChosenVoicesDialog.ShowOverAsync(this, _fatalErrorHandler) is {} chosen)
            _dialog.Offer(chosen.ChosenVoicesOf(_providers));
      });

   void OnUse(object? sender, RoutedEventArgs e) => Use();

   void Use()
   {
      _dialog.UseTheSelectedAttempt();
      Close();
   }

   void OnClose(object? sender, RoutedEventArgs e) => Close();
}
