using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using JAStudio.Core;
using JAStudio.Core.TaskRunners;
using JAStudio.TextToSpeech;
using JAStudio.Audio;
using JAStudio.UI.ViewModels.TextToSpeech;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.TextToSpeech;

/// <summary>
/// The dialog for choosing the voices the TTS dialogs offer, showing a <see cref="ChosenVoicesDialogViewModel"/>. Saving writes the chosen voices,<br/>
/// in their order, into the text-to-speech settings file (<see cref="IEnvironmentPaths.TextToSpeechSettingsFile"/>).
/// </summary>
/// <remarks>It plays the samples through a <see cref="SoundFlowAudioPlayer"/> of its own, closed with the dialog.</remarks>
partial class ChosenVoicesDialog : Window
{
   readonly ChosenVoicesDialogViewModel _dialog;
   readonly string _settingsFile;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public ChosenVoicesDialog() => InitializeComponent();
#pragma warning restore CS8618

   ChosenVoicesDialog(TemporaryServiceCollection services)
   {
      _settingsFile = services.Resolve<IEnvironmentPaths>().TextToSpeechSettingsFile;
      var settings = TextToSpeechSettings.ReadFrom(_settingsFile);
      var player = new SoundFlowAudioPlayer();
      _dialog = new ChosenVoicesDialogViewModel(settings, _settingsFile, settings.ProvidersWithKeys(), player, services.Resolve<IFatalErrorHandler>());
      DataContext = _dialog;
      InitializeComponent();
      Opened += (_, _) => _dialog.ListTheVoices();
      Closed += (_, _) => player.Dispose();
   }

   /// <summary>Opens the dialog where it was last, as from the workspace's Config menu, and returns it.</summary>
   internal static ChosenVoicesDialog Open(TemporaryServiceCollection services) => new ChosenVoicesDialog(services).ShowWhereItWasLast(services);

   /// <summary>Shows the dialog modally over <paramref name="owner"/>, and returns the settings saved, or null when it was cancelled.</summary>
   internal static Task<TextToSpeechSettings?> ShowOverAsync(Window owner, TemporaryServiceCollection services) =>
      new ChosenVoicesDialog(services) { WindowStartupLocation = WindowStartupLocation.CenterOwner }.ShowDialog<TextToSpeechSettings?>(owner);

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
      chosen.WriteTo(_settingsFile);
      Close(chosen);
   }

   void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
