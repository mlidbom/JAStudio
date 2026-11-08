using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.KanjiReadings;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One of the readings in a <see cref="KanjiReadingListEditorViewModel"/>, shown as a chip: its kana, a check box for whether it's primary,<br/>
/// and a button that removes it.
/// </summary>
partial class KanjiReadingChipViewModel : ObservableObject
{
   readonly Action<KanjiReading> _remove;
   readonly Action<KanjiReading> _changeWhetherPrimary;
   bool _isPrimary;

   /// <param name="reading">The reading shown.</param>
   /// <param name="remove">Removes the reading from the kanji.</param>
   /// <param name="changeWhetherPrimary">Makes the reading primary, or secondary, as the reading passed says.</param>
   public KanjiReadingChipViewModel(KanjiReading reading, Action<KanjiReading> remove, Action<KanjiReading> changeWhetherPrimary)
   {
      Kana = reading.Kana;
      _isPrimary = reading.IsPrimary;
      _remove = remove;
      _changeWhetherPrimary = changeWhetherPrimary;
   }

   public string Kana { get; }

   /// <summary>Whether the reading is primary. Checking or unchecking it makes the reading primary, or secondary, at once.</summary>
   public bool IsPrimary
   {
      get => _isPrimary;
      set
      {
         if(SetProperty(ref _isPrimary, value))
            _changeWhetherPrimary(new KanjiReading(Kana, value));
      }
   }

   [RelayCommand]
   void Remove() => _remove(new KanjiReading(Kana, IsPrimary));
}
