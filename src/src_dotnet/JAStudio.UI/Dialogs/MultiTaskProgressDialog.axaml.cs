using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Dialogs;

/// <summary>
/// Shared singleton dialog that displays task progress.
/// Driven entirely by <see cref="TaskProgressDialogViewModel"/>: the
/// <see cref="AvaloniaUIThreadDispatcher"/> calls <see cref="Show"/>/<see cref="Hide()"/>
/// in response to the view model's <c>IsVisible</c> property changes.
/// Content is data-bound to <see cref="TaskProgressDialogViewModel.RootScopes"/>.
/// </summary>
partial class MultiTaskProgressDialog : Window
{
   static MultiTaskProgressDialog? _instance;

   MultiTaskProgressDialog() => InitializeComponent();

   void InitializeComponent() => AvaloniaXamlLoader.Load(this);

   /// <summary>Show the dialog if it is not already visible. Must be called on the UI thread.</summary>
   internal static void Show(TaskProgressDialogViewModel viewModel)
   {
      if(_instance is not { IsVisible: true })
      {
         _instance = new MultiTaskProgressDialog { DataContext = viewModel, WindowStartupLocation = WindowStartupLocation.CenterScreen };
         _instance.Show();
      }
   }

   /// <summary>Close the dialog. Must be called on the UI thread.</summary>
   internal new static void Hide()
   {
      if(_instance == null) return;
      _instance.Close();
      _instance = null;
   }
}
