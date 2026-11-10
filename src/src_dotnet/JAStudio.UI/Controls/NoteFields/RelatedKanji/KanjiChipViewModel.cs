using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JAStudio.UI.Controls.NoteFields.RelatedKanji;

/// <summary>
/// One kanji in a <see cref="KanjiListEditorViewModel"/>'s list, shown as a chip: its character, and a button that removes it. A kanji the corpus has no<br/>
/// note for is shown dimmed, since it may be a mistake, though naming such a kanji is allowed.
/// </summary>
partial class KanjiChipViewModel : ObservableObject
{
   readonly Action<string> _remove;

   /// <param name="kanji">The kanji shown, by its character.</param>
   /// <param name="theCorpusHasANoteForIt">Whether the corpus has a note for the kanji.</param>
   /// <param name="remove">Removes the kanji from the list.</param>
   public KanjiChipViewModel(string kanji, bool theCorpusHasANoteForIt, Action<string> remove)
   {
      Kanji = kanji;
      TheCorpusHasANoteForIt = theCorpusHasANoteForIt;
      _remove = remove;
   }

   public string Kanji { get; }

   public bool TheCorpusHasANoteForIt { get; }

   /// <summary>Why the chip is dimmed, when it is: shown when the mouse rests on it.</summary>
   public string? WhyDimmed => TheCorpusHasANoteForIt ? null : "The corpus has no note for this kanji";

   [RelayCommand]
   void Remove() => _remove(Kanji);
}
