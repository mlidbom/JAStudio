using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class ReadingsMappingsDialog : Window
{
   [Obsolete("For XAML designer/previever only")]
   // ReSharper disable once UnusedMember.Global
   public ReadingsMappingsDialog() {}

   public ReadingsMappingsDialog(Core.TemporaryServiceCollection services)
   {
      InitializeComponent();
      DataContext = new ReadingsMappingsDialogViewModel(this, services);
   }

   void InitializeComponent()
   {
      AvaloniaXamlLoader.Load(this);
   }
}
