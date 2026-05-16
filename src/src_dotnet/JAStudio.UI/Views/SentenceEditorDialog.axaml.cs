using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Anki;
using JAStudio.Core.Note.Sentences;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class SentenceEditorDialog : Window
{
   public SentenceEditorDialog()
   {
      InitializeComponent();
   }

   public SentenceEditorDialog(SentenceNote sentence) : this()
   {
      var viewModel = new SentenceEditorViewModel(sentence);

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
