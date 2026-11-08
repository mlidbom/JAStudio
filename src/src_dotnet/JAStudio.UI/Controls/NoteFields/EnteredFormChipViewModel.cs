using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One of the forms in an <see cref="EnteredFormsEditorViewModel"/>, shown as a chip: its text, a check box for whether the vocab owns it,<br/>
/// and a button that removes it.
/// </summary>
partial class EnteredFormChipViewModel : ObservableObject
{
   readonly Action<EnteredForm> _remove;
   readonly Action<EnteredForm> _changeTheOwnership;
   bool _isOwned;

   /// <param name="form">The form shown.</param>
   /// <param name="remove">Removes the form from the vocab.</param>
   /// <param name="changeTheOwnership">Makes the vocab own the form, or stop owning it, as the form passed says.</param>
   public EnteredFormChipViewModel(EnteredForm form, Action<EnteredForm> remove, Action<EnteredForm> changeTheOwnership)
   {
      Text = form.Text;
      _isOwned = form.IsOwned;
      _remove = remove;
      _changeTheOwnership = changeTheOwnership;
   }

   public string Text { get; }

   /// <summary>Whether the vocab owns the form. Checking or unchecking it makes the vocab own the form, or stop owning it, at once.</summary>
   public bool IsOwned
   {
      get => _isOwned;
      set
      {
         if(SetProperty(ref _isOwned, value))
            _changeTheOwnership(new EnteredForm(Text, value));
      }
   }

   [RelayCommand]
   void Remove() => _remove(new EnteredForm(Text, IsOwned));
}
