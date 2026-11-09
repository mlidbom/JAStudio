using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>One of the parts of speech in a <see cref="PartsOfSpeechEditorViewModel"/>, shown as a chip: its <see cref="PartOfSpeech.Name"/>, and a button that removes it.</summary>
partial class PartOfSpeechChipViewModel : ObservableObject
{
   readonly PartOfSpeech _partOfSpeech;
   readonly Action<PartOfSpeech> _remove;

   /// <param name="partOfSpeech">The part of speech shown.</param>
   /// <param name="remove">Removes the part of speech from the vocab.</param>
   public PartOfSpeechChipViewModel(PartOfSpeech partOfSpeech, Action<PartOfSpeech> remove)
   {
      _partOfSpeech = partOfSpeech;
      _remove = remove;
   }

   public string Name => _partOfSpeech.Name;

   [RelayCommand]
   void Remove() => _remove(_partOfSpeech);
}
