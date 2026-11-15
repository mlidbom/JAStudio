using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.UI.ViewModels.TextToSpeech;
using JAStudio.UI.Views.TextToSpeech;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.TextToSpeech;

/// <summary>
/// The batch TTS dialog for two sentences, opened as from the search results' menu, and rendered: <c>BatchTextToSpeechDialog.png</c>, and with the first<br/>
/// sentence selected, <c>BatchTextToSpeechDialogWithANoteSelected.png</c>, showing its own text-to-speech. A view that doesn't load, such as one using a<br/>
/// resource it doesn't define, throws when the window opens, so it fails here rather than in Anki. The dialog speaks nothing as it opens, nor when a note<br/>
/// is selected, so no tts-provider is called. It offers the voices chosen in the user's profile, if any.
/// </summary>
public class When_opening_the_batch_tts_dialog : SpecificationOpeningWindows, IAIGeneratedSpec
{
   readonly BatchTextToSpeechDialog _dialog;

   public When_opening_the_batch_tts_dialog()
   {
      IHasTextToSpeechAudio[] sentences = [CreateTestSentence("家に帰る", "Go home"), CreateTestSentence("明日学校に行く", "I'll go to school tomorrow")];
      _dialog = Open(() => BatchTextToSpeechDialog.OpenFor(sentences, Services));
      Render(_dialog, "BatchTextToSpeechDialog");
   }

   [XF] public void the_dialog_is_shown() => OnTheUIThread(() => _dialog.IsVisible).Must().BeTrue();

   public class with_the_first_sentence_selected : When_opening_the_batch_tts_dialog
   {
      public with_the_first_sentence_selected()
      {
         OnTheUIThread(() =>
         {
            var dialog = ViewModelOf<BatchTextToSpeechDialogViewModel>(_dialog);
            dialog.SelectedNote = dialog.Notes[0];
         });
         Render(_dialog, "BatchTextToSpeechDialogWithANoteSelected");
      }

      [XF] public void its_own_text_to_speech_shows_its_text_to_speak() =>
         OnTheUIThread(() => ViewModelOf<BatchTextToSpeechDialogViewModel>(_dialog).SelectedNote?.TextToSpeech.TextToSpeak).Must().Be("家に帰る");
   }
}
