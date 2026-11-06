using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Controls;

/// <summary>
/// One text field of a note where every change is costly, such as a vocab's question, which changes what the vocab matches:<br/>
/// what's typed is written to the note only when it's committed, by pressing Enter or by the keyboard leaving the text box.<br/>
/// Until then the text box shows that it holds <see cref="HasUncommittedText"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="TextFieldEditorViewModel"/>, the text box and the note differ while there's uncommitted text,<br/>
/// so <see cref="Refresh"/> keeps it: it only shows a change made elsewhere when nothing is being typed.
/// </remarks>
partial class CommittingTextFieldEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<string> _currentText;
   readonly Action<string> _commit;

   /// <summary>The field as the note had it when it was last shown or committed.</summary>
   string _committedText;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while committing.</param>
   /// <param name="label">What the field holds, shown above the text box.</param>
   /// <param name="currentText">Reads the field from the note as the collection holds it now.</param>
   /// <param name="commit">Writes the field to the note as the collection holds it now, with everything that follows from the change.</param>
   public CommittingTextFieldEditorViewModel(IFatalErrorHandler fatalErrorHandler, string label, Func<string> currentText, Action<string> commit)
   {
      _fatalErrorHandler = fatalErrorHandler;
      Label = label;
      _currentText = currentText;
      _commit = commit;
      _committedText = currentText();
      _text = _committedText;
   }

   public string Label { get; }

   /// <summary>What the text box holds: the field as committed, or what's being typed.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(HasUncommittedText))]
   string _text;

   /// <summary>Whether <see cref="Text"/> has been typed but not yet committed to the note.</summary>
   public bool HasUncommittedText => Text != _committedText;

   /// <summary>Writes <see cref="Text"/> to the note, if it holds uncommitted text. Called on Enter and when the keyboard leaves the text box.</summary>
   [RelayCommand]
   public void Commit()
   {
      if(!HasUncommittedText) return;

      _fatalErrorHandler.HandleExceptionsFrom(() =>
      {
         _commit(Text);
         ShowTheNotesText();
      });
   }

   /// <summary>Shows the field as the note has it now, after a change made elsewhere, unless uncommitted text is being typed.</summary>
   public void Refresh()
   {
      if(HasUncommittedText) return;

      ShowTheNotesText();
   }

   void ShowTheNotesText()
   {
      _committedText = _currentText();
      Text = _committedText;
      OnPropertyChanged(nameof(HasUncommittedText));
   }
}
