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
/// The TTS dialog for one note, showing a <see cref="TextToSpeechForANoteViewModel"/> in a <see cref="TextToSpeechForANoteView"/>: generate the note's TTS audio<br/>
/// with the chosen voices, listen to the attempts, and use one. It opens where it was last, with the keyboard on the attempts, and the default voice speaking.
/// </summary>
/// <remarks>
/// The keys 1 to 9 play attempts 1 to 9, 0 plays the note's TTS audio as it is now, and Enter uses the selected attempt. They do nothing while<br/>
/// the keyboard is in the text to speak, where they're typed. Using one closes the dialog. Closing it otherwise changes nothing.<br/>
/// It plays through a <see cref="SoundFlowAudioPlayer"/> of its own, closed with the dialog.
/// </remarks>
partial class TextToSpeechDialog : Window
{
   readonly TextToSpeechForANoteViewModel _forTheNote;
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
      _forTheNote = new TextToSpeechForANoteViewModel(note, settings.ChosenVoicesOf(_providers), player, services);
      DataContext = _forTheNote;
      InitializeComponent();
      ForTheNote.ChoosingTheVoicesAsked += ChooseTheVoices;
      _forTheNote.UsedAnAttempt += _ => Close();
      KeyDown += OnKeyDown;
      Opened += (_, _) =>
      {
         Dispatcher.UIThread.Post(ForTheNote.PutTheKeyboardOnTheAttempts, DispatcherPriority.Loaded);
         _forTheNote.GenerateWithTheDefaultVoice();
      };
      Closed += (_, _) => player.Dispose();
   }

   /// <summary>Opens the TTS dialog for <paramref name="note"/>, where the dialog was last.</summary>
   internal static void OpenFor(IHasTextToSpeechAudio note, TemporaryServiceCollection services) => new TextToSpeechDialog(note, services).ShowWhereItWasLast(services);

   void OnKeyDown(object? sender, KeyEventArgs e)
   {
      if(ForTheNote.IsTheKeyboardInTheTextToSpeak) return;

      switch(e.Key)
      {
         case >= Key.D1 and <= Key.D9:
            _forTheNote.PlayTheAttemptNumbered(e.Key - Key.D0);
            break;
         case Key.D0 when _forTheNote.CurrentAudio.CanPlay:
            _forTheNote.PlayTheCurrentAudio();
            break;
         case Key.Enter when _forTheNote.CanUse:
            _forTheNote.UseTheSelectedAttempt();
            break;
         default:
            return;
      }

      e.Handled = true;
   }

   /// <summary>The voices chosen are offered at once. The dialog keeps its own tts-providers, so that a voice chosen again is still the same voice to compare attempts by.</summary>
   void ChooseTheVoices() =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await ChosenVoicesDialog.ShowOverAsync(this, _fatalErrorHandler) is {} chosen)
            _forTheNote.Offer(chosen.ChosenVoicesOf(_providers));
      });

   void OnClose(object? sender, RoutedEventArgs e) => Close();
}
