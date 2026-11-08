using System;
using Avalonia.Controls;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Importing;

namespace JAStudio.UI.Views.Importing;

/// <summary>The window for editing <see cref="ImportProfile"/>s, and for checking and running them: see <see cref="ImportProfilesWindowViewModel"/>.</summary>
partial class ImportProfilesWindow : Window
{
#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportProfilesWindow() => InitializeComponent();
#pragma warning restore CS8618

   ImportProfilesWindow(TemporaryServiceCollection services)
   {
      DataContext = new ImportProfilesWindowViewModel(services);
      InitializeComponent();
   }

   /// <summary>Opens the window near the cursor, as from the main menu.</summary>
   internal static void OpenNearTheCursor(TemporaryServiceCollection services) => new ImportProfilesWindow(services).ShowNearCursor();
}
