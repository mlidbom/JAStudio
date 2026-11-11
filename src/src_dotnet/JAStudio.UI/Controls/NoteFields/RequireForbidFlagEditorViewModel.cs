using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One require/forbid rule of a note, edited live in a <see cref="RequireForbidFlagEditor"/>: unset, required or forbidden.<br/>
/// A choice is written to the note at once.
/// </summary>
/// <remarks>
/// It reads and writes the rule on the note the collection holds now, since the collection replaces a note's object when it reloads the note (see <see cref="ViewModels.Workspace.Editors.VocabEditorSectionViewModel"/>).
/// </remarks>
class RequireForbidFlagEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<RequireForbidChoice> _readTheChoiceOnTheNote;
   readonly Action<RequireForbidChoice> _writeAChoiceToTheNote;
   readonly Action _choiceMade;
   RequireForbidChoice _choice;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a choice, which the radio buttons' bindings would swallow.</param>
   /// <param name="label">What the rule is about, shown beside its choices.</param>
   /// <param name="readTheChoiceOnTheNote">Reads the rule's choice on the note as the collection holds it now.</param>
   /// <param name="writeAChoiceToTheNote">Writes a choice for the rule to the note as the collection holds it now, and saves the note.</param>
   /// <param name="choiceMade">Called after a choice is written to the note, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public RequireForbidFlagEditorViewModel(IFatalErrorHandler fatalErrorHandler, string label, Func<RequireForbidChoice> readTheChoiceOnTheNote, Action<RequireForbidChoice> writeAChoiceToTheNote, Action choiceMade)
   {
      //review:matching: "Flag" here and in RequireForbidFlagEditor is after RequireForbidFlagField, now only the rules outside the match-condition, but
      //   this edits the RequireForbidMatchingRules of the match-condition too. RequireForbidRuleEditorViewModel would name what it edits in both cases.
      _fatalErrorHandler = fatalErrorHandler;
      Label = label;
      _readTheChoiceOnTheNote = readTheChoiceOnTheNote;
      _writeAChoiceToTheNote = writeAChoiceToTheNote;
      _choiceMade = choiceMade;
      _choice = ChoiceOnTheNote();
   }

   public string Label { get; }

   public bool IsUnset
   {
      get => _choice == RequireForbidChoice.Unset;
      set => ChooseIfChecked(value, RequireForbidChoice.Unset);
   }

   public bool IsRequired
   {
      get => _choice == RequireForbidChoice.Required;
      set => ChooseIfChecked(value, RequireForbidChoice.Required);
   }

   public bool IsForbidden
   {
      get => _choice == RequireForbidChoice.Forbidden;
      set => ChooseIfChecked(value, RequireForbidChoice.Forbidden);
   }

   /// <summary>Shows the rule as the note has it now, after a change made elsewhere.</summary>
   public void Refresh() => Show(ChoiceOnTheNote());

   /// <summary>Checking a radio button makes its choice. Unchecking the others, which the radio buttons do by themselves, chooses nothing.</summary>
   void ChooseIfChecked(bool isChecked, RequireForbidChoice choice)
   {
      if(!isChecked || choice == _choice) return;

      _fatalErrorHandler.HandleExceptionsFrom(() =>
      {
         _writeAChoiceToTheNote(choice);
         Show(choice);
         _choiceMade();
      });
   }

   void Show(RequireForbidChoice choice)
   {
      _choice = choice;
      OnPropertyChanged(nameof(IsUnset));
      OnPropertyChanged(nameof(IsRequired));
      OnPropertyChanged(nameof(IsForbidden));
   }

   RequireForbidChoice ChoiceOnTheNote() => _readTheChoiceOnTheNote();
}
