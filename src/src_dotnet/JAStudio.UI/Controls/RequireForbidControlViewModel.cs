using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Controls;

/// <summary>
/// One require/forbid rule of a note, edited live in a <see cref="RequireForbidControl"/>: unset, required or forbidden.<br/>
/// A choice is written to the note at once.
/// </summary>
/// <remarks>
/// It reads and writes the rule through <c>currentField</c>, which finds it on the note the collection holds now,<br/>
/// because the note recreates its rule objects whenever its tags change.
/// </remarks>
class RequireForbidControlViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<RequireForbidFlagField> _currentField;
   readonly Action _choiceMade;
   RequireForbidChoice _choice;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a choice, which the radio buttons' bindings would swallow.</param>
   /// <param name="label">What the rule is about, shown beside its choices.</param>
   /// <param name="currentField">Finds the rule on the note as the collection holds it now.</param>
   /// <param name="choiceMade">Called after a choice is written to the note, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public RequireForbidControlViewModel(IFatalErrorHandler fatalErrorHandler, string label, Func<RequireForbidFlagField> currentField, Action choiceMade)
   {
      _fatalErrorHandler = fatalErrorHandler;
      Label = label;
      _currentField = currentField;
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
         WriteToTheNote(choice);
         Show(choice);
         _choiceMade();
      });
   }

   void WriteToTheNote(RequireForbidChoice choice)
   {
      var field = _currentField();
      switch(choice)
      {
         case RequireForbidChoice.Required:
            field.SetRequired(true);
            break;
         case RequireForbidChoice.Forbidden:
            field.SetForbidden(true);
            break;
         case RequireForbidChoice.Unset:
            field.SetRequired(false);
            field.SetForbidden(false);
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(choice), choice, null);
      }
   }

   void Show(RequireForbidChoice choice)
   {
      _choice = choice;
      OnPropertyChanged(nameof(IsUnset));
      OnPropertyChanged(nameof(IsRequired));
      OnPropertyChanged(nameof(IsForbidden));
   }

   RequireForbidChoice ChoiceOnTheNote()
   {
      var field = _currentField();
      return field.IsConfiguredRequired  ? RequireForbidChoice.Required
           : field.IsConfiguredForbidden ? RequireForbidChoice.Forbidden
                                         : RequireForbidChoice.Unset;
   }
}
