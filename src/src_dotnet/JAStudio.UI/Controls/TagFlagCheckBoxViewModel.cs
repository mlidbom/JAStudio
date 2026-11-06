using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.UI.Controls;

/// <summary>
/// One flag of a note, stored as a tag, edited live as a check box. Checking or unchecking it writes to the note at once.
/// </summary>
/// <remarks>
/// It reads and writes the flag through <c>currentFlag</c>, which finds it on the note the collection holds now,<br/>
/// because the note recreates its flag objects whenever its tags change.<br/>
/// Writing a flag the note already has changes nothing, so a <see cref="Refresh"/> needs no guard against writing back what it read.
/// </remarks>
partial class TagFlagCheckBoxViewModel : ObservableObject
{
   readonly Func<TagFlagField> _currentFlag;

   /// <param name="label">What the flag means, shown beside the check box.</param>
   /// <param name="currentFlag">Finds the flag on the note as the collection holds it now.</param>
   public TagFlagCheckBoxViewModel(string label, Func<TagFlagField> currentFlag)
   {
      Label = label;
      _currentFlag = currentFlag;
      _isSet = currentFlag().IsSet();
   }

   public string Label { get; }

   [ObservableProperty] bool _isSet;

   /// <summary>Shows the flag as the note has it now, after a change made elsewhere.</summary>
   public void Refresh() => IsSet = _currentFlag().IsSet();

   partial void OnIsSetChanged(bool value) => _currentFlag().SetTo(value);
}
