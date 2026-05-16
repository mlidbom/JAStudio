using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class KanjiEditorDialog : Window
{
   public KanjiEditorDialog()
   {
      InitializeComponent();
   }

   public KanjiEditorDialog(KanjiNote kanji) : this()
   {
      var viewModel = new KanjiEditorViewModel(kanji);

      viewModel.SaveCommand = new RelayCommand(() =>
      {
         viewModel.Save();
         AnkiFacade.UIUtils.Refresh();
         Close();
      });

      viewModel.ApplyCommand = new RelayCommand(() =>
      {
         viewModel.Save();
         AnkiFacade.UIUtils.Refresh();
      });

      viewModel.CancelCommand = new RelayCommand(() =>
      {
         Close();
      });

      DataContext = viewModel;
   }
}
