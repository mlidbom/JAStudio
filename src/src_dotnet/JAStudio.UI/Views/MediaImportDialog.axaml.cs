using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class MediaImportDialog : Window
{
   public MediaImportDialog() => InitializeComponent();

   public MediaImportDialog(Core.TemporaryServiceCollection services) : this()
   {
      var viewModel = new MediaImportDialogViewModel(services);
      DataContext = viewModel;
      viewModel.CloseCommand = new RelayCommand(Close);
      Loaded += (_, _) => viewModel.AnalyzeCommand.Execute(null);
   }
}
