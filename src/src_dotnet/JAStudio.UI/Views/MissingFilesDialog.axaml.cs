using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JAStudio.Core.Note;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class MissingFilesDialog : Window
{
   readonly Action<NoteId>? _openNote;

   public MissingFilesDialog() => InitializeComponent();

   public MissingFilesDialog(List<MissingFileRow> vocabRows, List<MissingFileRow> sentenceRows, Action<NoteId> openNote) : this()
   {
      _openNote = openNote;

      VocabGrid.ItemsSource = vocabRows;
      SentenceGrid.ItemsSource = sentenceRows;

      VocabTab.Header = $"Vocab ({vocabRows.Count})";
      SentenceTab.Header = $"Sentences ({sentenceRows.Count})";

      VocabGrid.DoubleTapped += OnGridDoubleTapped;
      SentenceGrid.DoubleTapped += OnGridDoubleTapped;
   }

   void OnGridDoubleTapped(object? sender, TappedEventArgs e)
   {
      if(sender is DataGrid grid && grid.SelectedItem is MissingFileRow row)
         _openNote?.Invoke(row.NoteId);
   }

   void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
