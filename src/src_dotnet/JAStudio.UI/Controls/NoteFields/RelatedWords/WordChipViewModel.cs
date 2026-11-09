using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.Controls.NoteFields.RelatedWords;

/// <summary>
/// One word in a <see cref="WordSetEditorViewModel"/>'s set, shown as a chip: its name, and a button that removes it. A word that refers to no vocab in the<br/>
/// corpus is shown dimmed, since it may be a mistake, though a word without a vocab is allowed.
/// </summary>
partial class WordChipViewModel : ObservableObject
{
   readonly WordReference _word;
   readonly Action<WordReference> _remove;

   /// <param name="word">The word shown.</param>
   /// <param name="refersToAVocab">Whether the word refers to a vocab in the corpus (<see cref="Core.Note.Collection.VocabCollection.ReferredToBy"/>).</param>
   /// <param name="remove">Removes the word from the set.</param>
   public WordChipViewModel(WordReference word, bool refersToAVocab, Action<WordReference> remove)
   {
      _word = word;
      RefersToAVocab = refersToAVocab;
      _remove = remove;
   }

   /// <summary>The word's name, its <see cref="WordReference.DisambiguationName"/>.</summary>
   public string Name => _word.DisambiguationName;

   public bool RefersToAVocab { get; }

   /// <summary>Why the chip is dimmed, when it is: shown when the mouse rests on it.</summary>
   public string? WhyDimmed => RefersToAVocab ? null : "No vocab in the corpus has this word";

   [RelayCommand]
   void Remove() => _remove(_word);
}
