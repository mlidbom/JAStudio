using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One set of texts on a note (<see cref="Core.Note.EditableFields.EditableTextSetField{TNote}"/>), such as a vocab's "prefix is not" matching rule, edited live in a <see cref="TextSetEditor"/>:<br/>
/// typing a text and adding it, or removing one, writes to the note at once.
/// </summary>
/// <remarks>
/// It reads and changes the set through <c>currentValues</c>, <c>add</c> and <c>remove</c>, which find it on the note the collection holds now.
/// </remarks>
partial class TextSetEditorViewModel : ObservableObject
{
   readonly Func<IEnumerable<string>> _currentValues;
   readonly Action<string> _add;
   readonly Action<string> _remove;
   readonly Action _setChanged;

   /// <param name="title">What the set is, shown above it.</param>
   /// <param name="currentValues">Reads the set from the note as the collection holds it now.</param>
   /// <param name="add">Adds a text to the set on the note as the collection holds it now, and saves the note.</param>
   /// <param name="remove">Removes a text from the set on the note as the collection holds it now, and saves the note.</param>
   /// <param name="setChanged">Called after a change to the set is saved, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public TextSetEditorViewModel(string title, Func<IEnumerable<string>> currentValues, Action<string> add, Action<string> remove, Action setChanged)
   {
      Title = title;
      _currentValues = currentValues;
      _add = add;
      _remove = remove;
      _setChanged = setChanged;
      Refresh();
   }

   public string Title { get; }

   /// <summary>The texts in the set, in alphabetical order.</summary>
   public ObservableCollection<TextChipViewModel> Items { get; } = [];

   /// <summary>The text typed in to be added.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   public partial string NewValue { get; set; } = string.Empty;

   /// <summary>Shows the set as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      Items.Clear();
      foreach(var value in _currentValues().OrderBy(it => it))
         Items.Add(new TextChipViewModel(value, Remove));
   }

   [RelayCommand(CanExecute = nameof(CanAdd))]
   void Add()
   {
      _add(NewValue.Trim());
      NewValue = string.Empty;
      ChangeSaved();
   }

   bool CanAdd() => !string.IsNullOrWhiteSpace(NewValue);

   void Remove(string value)
   {
      _remove(value);
      ChangeSaved();
   }

   void ChangeSaved()
   {
      Refresh();
      _setChanged();
   }
}
