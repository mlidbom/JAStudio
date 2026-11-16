using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One <see cref="FlagField"/> of a note, edited live as a check box. Checking or unchecking it writes to the note at once.
/// </summary>
/// <remarks>
/// It reads and writes the flag on the note the collection holds now, since the collection replaces a note's object when it reloads the note<br/>
/// (see <see cref="ViewModels.Workspace.Editors.VocabEditorSectionViewModel"/>).
/// </remarks>
class FlagEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<bool> _readTheFlagOnTheNote;
   readonly Action<bool> _writeTheFlagToTheNote;
   readonly Action _flagChanged;
   bool _isSet;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a change, which the check box's binding would swallow.</param>
   /// <param name="label">What the flag means, shown beside the check box.</param>
   /// <param name="readTheFlagOnTheNote">Reads whether the flag is set on the note as the collection holds it now.</param>
   /// <param name="writeTheFlagToTheNote">Sets or unsets the flag on the note as the collection holds it now, and saves the note.</param>
   /// <param name="flagChanged">Called after a change is written to the note, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public FlagEditorViewModel(IFatalErrorHandler fatalErrorHandler, string label, Func<bool> readTheFlagOnTheNote, Action<bool> writeTheFlagToTheNote, Action flagChanged)
   {
      _fatalErrorHandler = fatalErrorHandler;
      Label = label;
      _readTheFlagOnTheNote = readTheFlagOnTheNote;
      _writeTheFlagToTheNote = writeTheFlagToTheNote;
      _flagChanged = flagChanged;
      _isSet = readTheFlagOnTheNote();
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
            _writeTheFlagToTheNote(value);
            Show(value);
            _flagChanged();
         });
      }
   }

   /// <summary>Shows the flag as the note has it now, after a change made elsewhere.</summary>
   public void Refresh() => Show(_readTheFlagOnTheNote());

   void Show(bool isSet)
   {
      _isSet = isSet;
      OnPropertyChanged(nameof(IsSet));
   }
}
