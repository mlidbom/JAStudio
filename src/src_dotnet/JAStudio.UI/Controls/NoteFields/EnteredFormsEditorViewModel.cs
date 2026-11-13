using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// A vocab's <see cref="EnteredForm"/>s, edited live in an <see cref="EnteredFormsEditor"/>: each form as a chip that says whether the vocab owns it,<br/>
/// and a box for adding one. Adding or removing a form, and owning it or no longer owning it, writes to the vocab at once.
/// </summary>
partial class EnteredFormsEditorViewModel : ObservableObject
{
   readonly Func<VocabNote> _currentVocab;
   readonly Action _formsChanged;

   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="formsChanged">Called after a change to the forms is saved, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public EnteredFormsEditorViewModel(Func<VocabNote> currentVocab, Action formsChanged)
   {
      _currentVocab = currentVocab;
      _formsChanged = formsChanged;
      Refresh();
   }

   /// <summary>The forms, in alphabetical order.</summary>
   public ObservableCollection<EnteredFormChipViewModel> Items { get; } = [];

   /// <summary>The form typed in to be added.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   public partial string NewForm { get; set; } = string.Empty;

   /// <summary>Shows the forms as the vocab has them now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      Items.Clear();
      foreach(var form in _currentVocab().Forms.EnteredForms().OrderBy(it => it.Text))
         Items.Add(new EnteredFormChipViewModel(form, Remove, ChangeTheOwnership));
   }

   /// <summary>Adds the <see cref="NewForm"/>, not owned.</summary>
   [RelayCommand(CanExecute = nameof(CanAdd))]
   void Add()
   {
      _currentVocab().Forms.Add(NewForm.Trim());
      NewForm = string.Empty;
      ChangeSaved();
   }

   bool CanAdd() => !string.IsNullOrWhiteSpace(NewForm);

   void Remove(EnteredForm form)
   {
      _currentVocab().Forms.Remove(form.Text);
      ChangeSaved();
   }

   void ChangeTheOwnership(EnteredForm form)
   {
      var forms = _currentVocab().Forms;
      if(form.IsOwned)
         forms.Own(form.Text);
      else
         forms.StopOwning(form.Text);

      ChangeSaved();
   }

   void ChangeSaved()
   {
      Refresh();
      _formsChanged();
   }
}
