using System;
using Avalonia.Controls;
using Compze.Internals.Logging;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class OptionsDialog : Window
{
   [Obsolete("For XAML designer/previever only")]
   // ReSharper disable once UnusedMember.Global
   public OptionsDialog() {}

   public OptionsDialog(Core.TemporaryServiceCollection services)
   {
      this.Log().Info("OptionsDialog constructor: calling InitializeComponent()...");
      InitializeComponent();
      this.Log().Info("OptionsDialog constructor: creating ViewModel...");
      DataContext = new OptionsDialogViewModel(this, services);
      this.Log().Info("OptionsDialog constructor: completed");
   }
}
