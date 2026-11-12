using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// A vocab's <see cref="VocabNotePartsOfSpeech"/>, edited live in a <see cref="PartsOfSpeechEditor"/>: each part of speech as a chip with a button<br/>
/// that removes it, and a box for adding one, which offers only the <see cref="PartOfSpeech.All"/> JAStudio knows, filtered by what's typed.<br/>
/// Adding or removing one writes to the vocab at once.
/// </summary>
/// <remarks>
/// It reads and writes the parts of speech through <see cref="_currentPartsOfSpeech"/>, which finds them on the vocab the collection holds now,<br/>
/// since the collection replaces a note's object when it reloads the note.
/// </remarks>
partial class PartsOfSpeechEditorViewModel : ObservableObject
{
   readonly Func<VocabNotePartsOfSpeech> _currentPartsOfSpeech;
   readonly Action _partsOfSpeechChanged;

   /// <param name="currentPartsOfSpeech">Finds the parts of speech edited, on the vocab as the collection holds it now.</param>
   /// <param name="partsOfSpeechChanged">Called after a change is saved, but not when <see cref="Refresh"/> shows a change made elsewhere.</param>
   public PartsOfSpeechEditorViewModel(Func<VocabNotePartsOfSpeech> currentPartsOfSpeech, Action partsOfSpeechChanged)
   {
      _currentPartsOfSpeech = currentPartsOfSpeech;
      _partsOfSpeechChanged = partsOfSpeechChanged;
      Refresh();
   }

   /// <summary>The vocab's parts of speech, in alphabetical order.</summary>
   public ObservableCollection<PartOfSpeechChipViewModel> Items { get; } = [];

   /// <summary>The names of the parts of speech the vocab doesn't have yet: what the box for adding one offers.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   public partial IReadOnlyList<string> Addable { get; set; } = [];

   /// <summary>The text in the box for adding a part of speech. Only the name of one of the <see cref="Addable"/> can be added.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   public partial string NewPartOfSpeech { get; set; } = string.Empty;

   /// <summary>Shows the parts of speech as the vocab has them now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      var partsOfSpeech = _currentPartsOfSpeech();
      Items.Clear();
      foreach(var partOfSpeech in partsOfSpeech.All())
         Items.Add(new PartOfSpeechChipViewModel(partOfSpeech, Remove));

      Addable = [.. PartOfSpeech.All.Where(it => !partsOfSpeech.Contains(it)).Select(it => it.Name)];
   }

   /// <summary>Adds the part of speech named in <see cref="NewPartOfSpeech"/>.</summary>
   [RelayCommand(CanExecute = nameof(CanAdd))]
   void Add()
   {
      _currentPartsOfSpeech().Add(PartOfSpeech.Named(NewPartOfSpeech.Trim()));
      NewPartOfSpeech = string.Empty;
      ChangeSaved();
   }

   bool CanAdd() => Addable.Contains(NewPartOfSpeech.Trim());

   void Remove(PartOfSpeech partOfSpeech)
   {
      _currentPartsOfSpeech().Remove(partOfSpeech);
      ChangeSaved();
   }

   void ChangeSaved()
   {
      Refresh();
      _partsOfSpeechChanged();
   }
}
