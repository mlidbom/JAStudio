using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Controls;

/// <summary>
/// One text field of a note, edited live in a text box: every keystroke is written to the note at once.
/// </summary>
/// <remarks>
/// It reads and writes the field through <c>currentText</c> and <c>write</c>, which find the field on the note the collection holds now,<br/>
/// because the collection replaces a note's object when Anki edits the note and when it reloads.
/// </remarks>
/// <remarks>
/// Every keystroke writes the text box's whole text to the note, so the note holds what the text box shows.<br/>
/// And because it does, <see cref="Refresh"/> never overwrites typing.<br/>
/// A comma separated list may be read back as the list joined again. The text box then keeps the text as typed, as long as it means the same list:<br/>
/// otherwise text being typed, like a trailing ", ", would disappear.
/// </remarks>
/// <remarks>
/// The note may change what was typed on purpose, as when a kanji's on readings are turned into hiragana.<br/>
/// The note then holds the changed text, but the text box keeps showing what was typed until the editor is shown again:<br/>
/// <see cref="Text"/>'s setter announces the note's text while Avalonia's binding is still writing the typed text to it,<br/>
/// and the text box doesn't take it. The note stays right, because the next keystroke writes the whole text again.
/// </remarks>
class TextFieldEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<string> _currentText;
   readonly Action<string> _write;
   string _text;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a keystroke, which the text box's binding would swallow.</param>
   /// <param name="label">What the field holds, shown above the text box.</param>
   /// <param name="format">What kind of text the field holds.</param>
   /// <param name="currentText">Reads the field from the note as the collection holds it now.</param>
   /// <param name="write">Writes the field to the note as the collection holds it now.</param>
   public TextFieldEditorViewModel(IFatalErrorHandler fatalErrorHandler, string label, TextFieldFormat format, Func<string> currentText, Action<string> write)
   {
      _fatalErrorHandler = fatalErrorHandler;
      Label = label;
      Format = format;
      _currentText = currentText;
      _write = write;
      _text = currentText();
   }

   public string Label { get; }

   public TextFieldFormat Format { get; }

   /// <summary>Whether the text box wraps its lines and Enter starts a new one.</summary>
   public bool IsMultiLine => Format == TextFieldFormat.Paragraphs;

   /// <summary>Shown in the text box while it's empty: how to write several values, for the formats that hold them.</summary>
   public string Hint => Format switch
   {
      TextFieldFormat.CommaSeparated => "comma separated",
      TextFieldFormat.CommaSeparatedReadings => "comma separated, primary readings marked <primary>reading</primary>",
      _ => ""
   };

   public string Text
   {
      get => _text;
      set
      {
         if(value == _text) return;

         _text = value;
         _fatalErrorHandler.HandleExceptionsFrom(() =>
         {
            _write(value);
            Show(_currentText());
         });
      }
   }

   /// <summary>Shows the field as the note has it now, after a change made elsewhere.</summary>
   public void Refresh() => Show(_currentText());

   bool IsCommaSeparated => Format is TextFieldFormat.CommaSeparated or TextFieldFormat.CommaSeparatedReadings;

   /// <summary>Shows <paramref name="notesText"/>, unless the text box already holds text that means the same.</summary>
   void Show(string notesText)
   {
      if(MeansTheSameAsTheShownText(notesText)) return;

      _text = notesText;
      OnPropertyChanged(nameof(Text));
   }

   bool MeansTheSameAsTheShownText(string notesText) =>
      IsCommaSeparated
         ? StringExtensions.ExtractCommaSeparatedValues(notesText).SequenceEqual(StringExtensions.ExtractCommaSeparatedValues(_text))
         : notesText == _text;
}
