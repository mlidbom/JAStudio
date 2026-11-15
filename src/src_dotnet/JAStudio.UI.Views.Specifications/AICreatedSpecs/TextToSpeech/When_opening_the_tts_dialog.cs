using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.UI.ViewModels.TextToSpeech;
using JAStudio.UI.Views.TextToSpeech;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.TextToSpeech;

/// <summary>
/// The TTS dialog for a sentence, opened as from a note's menu, and rendered: <c>TextToSpeechDialog.png</c>. A view that doesn't load, such as one<br/>
/// using a resource it doesn't define, throws when the window opens, so it fails here rather than in Anki. The specifications' environment has no<br/>
/// text-to-speech settings file, so no tts-provider has a key and no voice is chosen: the default voice that speaks as the dialog opens is none,<br/>
/// and no service is called.
/// </summary>
public class When_opening_the_tts_dialog : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly TextToSpeechDialog _dialog;

   public When_opening_the_tts_dialog()
   {
      var sentence = CreateTestSentence("家に帰る", "Go home");
      _dialog = Open(() => TextToSpeechDialog.OpenFor(sentence, Services));
      Render(_dialog, "TextToSpeechDialog");
   }

   [XF] public void the_dialog_is_shown() => OnTheUIThread(() => _dialog.IsVisible).Must().BeTrue();
   [XF] public void it_says_no_voices_are_chosen() => OnTheUIThread(() => ViewModelOf<TextToSpeechForANoteViewModel>(_dialog).Status).Must().Be("No voices are chosen yet: choose them with Voices...");
}
