using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Anki;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class VocabEditorDialog : Window
{
   public VocabEditorDialog()
   {
      InitializeComponent();
   }

   public VocabEditorDialog(VocabNote vocab) : this()
   {
      var viewModel = new VocabEditorViewModel(vocab);

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
