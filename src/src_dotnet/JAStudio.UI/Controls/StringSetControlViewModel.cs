using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JAStudio.UI.Controls;

/// <summary>
/// One set of strings on a note, such as a vocab's "prefix is not" matching rule, edited live in a <see cref="StringSetControl"/>:<br/>
/// typing a string and adding it, or removing one, writes to the note at once.
/// </summary>
/// <remarks>
/// It reads and changes the set through <c>currentSet</c>, which finds it on the note the collection holds now.
/// </remarks>
partial class StringSetControlViewModel : ObservableObject
{
   readonly Func<HashSet<string>> _currentSet;
   readonly Action _saveTheNote;
   readonly Action _setChanged;

   /// <param name="title">What the set is, shown above it.</param>
   /// <param name="currentSet">Finds the set on the note as the collection holds it now.</param>
   /// <param name="saveTheNote">Saves the note after its set was changed: changing the set itself saves nothing.</param>
   /// <param name="setChanged">Called after a change to the set is saved, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public StringSetControlViewModel(string title, Func<HashSet<string>> currentSet, Action saveTheNote, Action setChanged)
   {
      Title = title;
      _currentSet = currentSet;
      _saveTheNote = saveTheNote;
      _setChanged = setChanged;
      Refresh();
   }

   public string Title { get; }

   /// <summary>The strings in the set, in alphabetical order.</summary>
   public ObservableCollection<StringChipViewModel> Items { get; } = [];

   /// <summary>The string typed in to be added.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   string _newValue = string.Empty;

   /// <summary>Shows the set as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      Items.Clear();
      foreach(var value in _currentSet().OrderBy(it => it))
         Items.Add(new StringChipViewModel(value, Remove));
   }

   [RelayCommand(CanExecute = nameof(CanAdd))]
   void Add()
   {
      _currentSet().Add(NewValue.Trim());
      NewValue = string.Empty;
      SaveChange();
   }

   bool CanAdd() => !string.IsNullOrWhiteSpace(NewValue);

   void Remove(string value)
   {
      _currentSet().Remove(value);
      SaveChange();
   }

   void SaveChange()
   {
      _saveTheNote();
      Refresh();
      _setChanged();
   }
}
