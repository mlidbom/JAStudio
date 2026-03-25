using Avalonia.Controls;
using JAStudio.Anki;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class VocabFlagsDialog : Window
{
   public VocabFlagsDialog()
   {
      InitializeComponent();
   }

   public VocabFlagsDialog(VocabNote vocab, Core.TemporaryServiceCollection services) : this()
   {
      var viewModel = new VocabFlagsViewModel(vocab, services, this);

      // Wire up commands BEFORE setting DataContext so bindings resolve to non-null commands
      viewModel.SaveCommand = new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(async () =>
      {
         await viewModel.SaveAsync();
         AnkiFacade.UIUtils.Refresh();
         Close();
      });

      viewModel.CancelCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(Close);

      DataContext = viewModel;
   }
}
