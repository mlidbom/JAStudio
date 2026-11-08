using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JAStudio.Core;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.Audio;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.TextToSpeech;

namespace JAStudio.UI.Views.TextToSpeech;

/// <summary>
/// The dialog for choosing the voices the TTS dialog offers, showing a <see cref="ChosenVoicesDialogViewModel"/>. Saving writes the chosen voices,<br/>
/// in their order, into the settings file in the user's profile (<see cref="TextToSpeechSettings.FileInTheUserProfile"/>).
/// </summary>
/// <remarks>It plays the samples through a <see cref="SoundFlowAudioPlayer"/> of its own, closed with the dialog.</remarks>
partial class ChosenVoicesDialog : Window
{
   readonly ChosenVoicesDialogViewModel _dialog;
   readonly SoundFlowAudioPlayer _player;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public ChosenVoicesDialog() => InitializeComponent();
#pragma warning restore CS8618

   ChosenVoicesDialog(IFatalErrorHandler fatalErrorHandler)
   {
      var settings = TextToSpeechSettings.ReadFromTheUserProfile();
      _player = new SoundFlowAudioPlayer();
      _dialog = new ChosenVoicesDialogViewModel(settings, settings.ProvidersWithKeys(), _player, fatalErrorHandler);
      DataContext = _dialog;
      InitializeComponent();
      Opened += (_, _) => _dialog.ListTheVoices();
      Closed += (_, _) => _player.Dispose();
   }

   /// <summary>Opens the dialog near the cursor, as from the Config menu.</summary>
   internal static void OpenNearTheCursor(TemporaryServiceCollection services) => new ChosenVoicesDialog(services.Resolve<IFatalErrorHandler>()).ShowNearCursor();

   /// <summary>Shows the dialog modally over <paramref name="owner"/>, and returns the settings saved, or null when it was cancelled.</summary>
   internal static Task<TextToSpeechSettings?> ShowOverAsync(Window owner, IFatalErrorHandler fatalErrorHandler) =>
      new ChosenVoicesDialog(fatalErrorHandler) { WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog<TextToSpeechSettings?>(owner);

   void OnPlayTheSample(object? sender, RoutedEventArgs e)
   {
      if(sender is not Control { DataContext: VoiceChoiceViewModel voice })
         throw new InvalidOperationException("A voice's ▶ button shows something other than a VoiceChoiceViewModel.");

      _dialog.PlayTheSampleOf(voice);
   }

   void OnChoose(object? sender, RoutedEventArgs e) => _dialog.ChooseTheSelected();

   void OnUnchoose(object? sender, RoutedEventArgs e) => _dialog.UnchooseTheSelected();

   void OnMoveUp(object? sender, RoutedEventArgs e) => _dialog.MoveTheSelected(-1);

   void OnMoveDown(object? sender, RoutedEventArgs e) => _dialog.MoveTheSelected(1);

   void OnSave(object? sender, RoutedEventArgs e)
   {
      var chosen = _dialog.TheSettingsChosen();
      chosen.WriteToTheUserProfile();
      Close(chosen);
   }

   void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
