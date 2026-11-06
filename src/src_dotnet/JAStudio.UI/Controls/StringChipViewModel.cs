using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JAStudio.UI.Controls;

/// <summary>One string in a <see cref="StringSetControlViewModel"/>'s set, shown as a chip with a button that removes it.</summary>
partial class StringChipViewModel(string value, Action<string> remove) : ObservableObject
{
   public string Value { get; } = value;

   [RelayCommand]
   void Remove() => remove(Value);
}
