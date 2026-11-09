using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>One text in a <see cref="TextSetEditorViewModel"/>'s set, shown as a chip with a button that removes it.</summary>
partial class TextChipViewModel(string value, Action<string> remove) : ObservableObject
{
   public string Value { get; } = value;

   [RelayCommand]
   void Remove() => remove(Value);
}
