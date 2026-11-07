using System;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.TextToSpeech;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// A note's TTS audio field, such as a vocab's <c>Audio_TTS</c>: edited as text like any <see cref="TextFieldEditorViewModel"/>,<br/>
/// with a "TTS..." button beside it that opens the <see cref="TextToSpeechDialog"/> for the note, to generate new TTS audio for it.
/// </summary>
/// <remarks>
/// Keeping an attempt in the dialog changes the field, and the editor shows the change as it does any other change to the note:<br/>
/// the tab showing the note refreshes its editor section when the collection announces the note's update.
/// </remarks>
partial class TextToSpeechAudioFieldEditorViewModel : TextFieldEditorViewModel
{
   readonly Action _openTheTextToSpeechDialog;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a keystroke, which the text box's binding would swallow.</param>
   /// <param name="label">What the field holds, shown above the text box.</param>
   /// <param name="currentText">Reads the field from the note as the collection holds it now.</param>
   /// <param name="write">Writes the field to the note as the collection holds it now.</param>
   /// <param name="openTheTextToSpeechDialog">Opens the <see cref="TextToSpeechDialog"/> for the note as the collection holds it now.</param>
   public TextToSpeechAudioFieldEditorViewModel(IFatalErrorHandler fatalErrorHandler, string label, Func<string> currentText, Action<string> write, Action openTheTextToSpeechDialog)
      : base(fatalErrorHandler, label, TextFieldFormat.Line, currentText, write) =>
      _openTheTextToSpeechDialog = openTheTextToSpeechDialog;

   [RelayCommand] void OpenTheTextToSpeechDialog() => _openTheTextToSpeechDialog();
}
