using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using JAStudio.Audio;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.UI.ViewModels.TextToSpeech;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.TextToSpeech;

/// <summary>
/// The batch TTS dialog, showing a <see cref="BatchTextToSpeechDialogViewModel"/>: TTS audio for many notes at once, such as the selected search results.<br/>
/// Above, the batch's voice, to try on a note and to run the batch with; below, the notes, each playing its TTS audio as it's selected, and beside them the<br/>
/// selected note's own text-to-speech (<see cref="TextToSpeechForANoteView"/>). It opens where it was last, with the keyboard on the notes, and speaks nothing.
/// </summary>
/// <remarks>
/// Its keys act on the selected note, wherever the keyboard is but in the text to speak and the voice's list: 0 and Space play its TTS audio again,<br/>
/// P tries the batch's voice on it, 1 to 9 play its attempts, and Enter uses the selected attempt. They're seen before the control with the keyboard sees<br/>
/// them, so that Space plays rather than selects. Closing the dialog stops the batch. It plays through a <see cref="SoundFlowAudioPlayer"/> of its own,<br/>
/// closed with the dialog.
/// </remarks>
partial class BatchTextToSpeechDialog : Window
{
   readonly BatchTextToSpeechDialogViewModel _dialog;
   readonly IReadOnlyList<ITextToSpeechProvider> _providers;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly TemporaryServiceCollection _services;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public BatchTextToSpeechDialog() => InitializeComponent();
#pragma warning restore CS8618

   BatchTextToSpeechDialog(IReadOnlyList<IHasTextToSpeechAudio> notes, TemporaryServiceCollection services)
   {
      _services = services;
      var settings = TextToSpeechSettings.ReadFrom(services.Resolve<IEnvironmentPaths>().TextToSpeechSettingsFile);
      _providers = settings.ProvidersWithKeys();
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      var player = new SoundFlowAudioPlayer();
      _dialog = new BatchTextToSpeechDialogViewModel(notes, settings.ChosenVoicesOf(_providers), player, services);
      DataContext = _dialog;
      InitializeComponent();
      ForTheSelectedNote.ChoosingTheVoicesAsked += ChooseTheVoices;
      AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
      Opened += (_, _) => Dispatcher.UIThread.Post(() => NoteList.Focus(), DispatcherPriority.Loaded);
      Closed += (_, _) =>
      {
         _dialog.StopTheBatch();
         player.Dispose();
      };
   }

   /// <summary>Opens the batch TTS dialog for <paramref name="notes"/>, where the dialog was last, and returns it.</summary>
   internal static BatchTextToSpeechDialog OpenFor(IReadOnlyList<IHasTextToSpeechAudio> notes, TemporaryServiceCollection services) => new BatchTextToSpeechDialog(notes, services).ShowWhereItWasLast(services);

   void OnKeyDown(object? sender, KeyEventArgs e)
   {
      if(ForTheSelectedNote.IsTheKeyboardInTheTextToSpeak || IsInTheVoicesList(e.Source)) return;

      switch(e.Key)
      {
         case >= Key.D1 and <= Key.D9 when _dialog.SelectedNote is {} selected:
            selected.TextToSpeech.PlayTheAttemptNumbered(e.Key - Key.D0);
            break;
         case Key.D0 or Key.Space:
            _dialog.PlayTheSelectedNotesAudio();
            break;
         case Key.P when _dialog.CanTryTheBatchVoice:
            _dialog.TryTheBatchVoiceOnTheSelectedNote();
            break;
         case Key.Enter when _dialog.CanUseTheSelectedNotesAttempt:
            _dialog.UseTheSelectedNotesAttempt();
            break;
         default:
            return;
      }

      e.Handled = true;
   }

   static bool IsInTheVoicesList(object? source) => (source as Visual)?.FindAncestorOfType<ComboBox>(includeSelf: true) is not null;

   void OnPlayANotesAudio(object? sender, RoutedEventArgs e)
   {
      if(sender is not Control { DataContext: NoteInTheBatchViewModel note })
         throw new InvalidOperationException("A note's ▶ shows something other than a NoteInTheBatchViewModel.");

      _dialog.Play(note);
   }

   void OnTryTheBatchVoice(object? sender, RoutedEventArgs e) => _dialog.TryTheBatchVoiceOnTheSelectedNote();

   void OnTryTheBatchVoiceOnANote(object? sender, RoutedEventArgs e)
   {
      if(sender is not Control { DataContext: NoteInTheBatchViewModel note })
         throw new InvalidOperationException("A note's Generate or ▶ shows something other than a NoteInTheBatchViewModel.");

      _dialog.TryTheBatchVoiceOn(note);
   }

   void OnRunTheBatch(object? sender, RoutedEventArgs e) => _dialog.StartTheBatch();

   void OnStopTheBatch(object? sender, RoutedEventArgs e) => _dialog.StopTheBatch();

   void OnChooseTheVoices(object? sender, RoutedEventArgs e) => ChooseTheVoices();

   /// <summary>The voices chosen are offered at once. The dialog keeps its own tts-providers, so that a voice chosen again is still the same voice to compare attempts by.</summary>
   void ChooseTheVoices() =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await ChosenVoicesDialog.ShowOverAsync(this, _services) is {} chosen)
            _dialog.Offer(chosen.ChosenVoicesOf(_providers));
      });

   void OnClose(object? sender, RoutedEventArgs e) => Close();
}
