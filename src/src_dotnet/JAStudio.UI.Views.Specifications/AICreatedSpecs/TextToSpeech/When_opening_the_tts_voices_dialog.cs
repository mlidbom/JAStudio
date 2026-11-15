using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.UI.ViewModels.TextToSpeech;
using JAStudio.UI.Views.TextToSpeech;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.TextToSpeech;

/// <summary>
/// The dialog for choosing the TTS voices, opened as from the main menu's Config, and rendered: <c>ChosenVoicesDialog.png</c>. A view that doesn't<br/>
/// load throws when the window opens, so it fails here rather than in Anki. The specifications' environment has no text-to-speech settings file,<br/>
/// so no tts-provider has a key, and none is asked for its voices.
/// </summary>
public class When_opening_the_tts_voices_dialog : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly ChosenVoicesDialog _dialog;

   public When_opening_the_tts_voices_dialog()
   {
      _dialog = Open(() => ChosenVoicesDialog.Open(Services));
      Render(_dialog, "ChosenVoicesDialog");
   }

   [XF] public void the_dialog_is_shown() => OnTheUIThread(() => _dialog.IsVisible).Must().BeTrue();

   [XF] public void it_says_where_the_keys_go() =>
      OnTheUIThread(() => ViewModelOf<ChosenVoicesDialogViewModel>(_dialog).Status).Must().Be($"No tts-provider has a key: put them in {GetService<IEnvironmentPaths>().TextToSpeechSettingsFile}.");
}
