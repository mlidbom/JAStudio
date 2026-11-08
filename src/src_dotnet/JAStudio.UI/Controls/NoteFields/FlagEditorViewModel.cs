using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One <see cref="FlagField"/> of a note, edited live as a check box. Checking or unchecking it writes to the note at once.
/// </summary>
/// <remarks>
/// It reads and writes the flag through <c>currentFlag</c>, which finds it on the note the collection holds now, since the collection<br/>
/// replaces a note's object when it reloads the note (see <see cref="ViewModels.Workspace.Editors.VocabEditorSectionViewModel"/>).
/// </remarks>
class FlagEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<FlagField> _currentFlag;
   readonly Action _flagChanged;
   bool _isSet;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a change, which the check box's binding would swallow.</param>
   /// <param name="label">What the flag means, shown beside the check box.</param>
   /// <param name="currentFlag">Finds the flag on the note as the collection holds it now.</param>
   /// <param name="flagChanged">Called after a change is written to the note, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public FlagEditorViewModel(IFatalErrorHandler fatalErrorHandler, string label, Func<FlagField> currentFlag, Action flagChanged)
   {
      _fatalErrorHandler = fatalErrorHandler;
      Label = label;
      _currentFlag = currentFlag;
      _flagChanged = flagChanged;
      _isSet = currentFlag().IsSet();
   }

   public string Label { get; }

   public bool IsSet
   {
      get => _isSet;
      set
      {
         if(value == _isSet) return;

         _fatalErrorHandler.HandleExceptionsFrom(() =>
         {
            _currentFlag().SetTo(value);
            Show(value);
            _flagChanged();
         });
      }
   }

   /// <summary>Shows the flag as the note has it now, after a change made elsewhere.</summary>
   public void Refresh() => Show(_currentFlag().IsSet());

   void Show(bool isSet)
   {
      _isSet = isSet;
      OnPropertyChanged(nameof(IsSet));
   }
}
