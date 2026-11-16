using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// A kanji's readings of one <see cref="KanjiReadingKind"/>, edited live in a <see cref="KanjiReadingListEditor"/>: each reading as a chip that says<br/>
/// whether it's primary, and a box for adding one. Adding or removing a reading, and making it primary or secondary, writes to the kanji at once.
/// </summary>
/// <remarks>
/// It reads the readings on the kanji the collection holds now, and changes them in an edit of the kanji, through <see cref="_readings"/>.
/// </remarks>
partial class KanjiReadingListEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly EditedNotePart<KanjiReadingList> _readings;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while making a reading primary or secondary, which the check box's binding would swallow.</param>
   /// <param name="readings">The readings edited, on the kanji the editor edits.</param>
   public KanjiReadingListEditorViewModel(IFatalErrorHandler fatalErrorHandler, EditedNotePart<KanjiReadingList> readings)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _readings = readings;
      Label = readings.Current.Kind.ToString();
      Refresh();
   }

   /// <summary>The editor's heading: the <see cref="KanjiReadingKind"/> of the readings it edits, such as "On".</summary>
   public string Label { get; }

   /// <summary>The readings, in the kanji's order.</summary>
   public ObservableCollection<KanjiReadingChipViewModel> Items { get; } = [];

   /// <summary>The reading typed in to be added. An on reading may be typed in katakana: it's kept in hiragana.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   public partial string NewReading { get; set; } = string.Empty;

   /// <summary>Shows the readings as the kanji has them now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      Items.Clear();
      foreach(var reading in _readings.Current.All())
         Items.Add(new KanjiReadingChipViewModel(reading, Remove, ChangeWhetherPrimary));
   }

   /// <summary>Adds the <see cref="NewReading"/> last, as a secondary reading.</summary>
   [RelayCommand(CanExecute = nameof(CanAdd))]
   void Add()
   {
      _readings.Edit(it => it.Add(NewReading.Trim()));
      NewReading = string.Empty;
      Refresh();
   }

   bool CanAdd() => !string.IsNullOrWhiteSpace(NewReading);

   void Remove(KanjiReading reading)
   {
      _readings.Edit(it => it.Remove(reading.Kana));
      Refresh();
   }

   void ChangeWhetherPrimary(KanjiReading reading) => _fatalErrorHandler.HandleExceptionsFrom(() =>
   {
      _readings.Edit(it =>
      {
         if(reading.IsPrimary)
            it.MakePrimary(reading.Kana);
         else
            it.MakeSecondary(reading.Kana);
      });

      Refresh();
   });
}
