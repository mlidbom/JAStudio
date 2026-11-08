using System;
using Avalonia.Controls;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.UI.WindowPlaces;
using JAStudio.UI.ViewModels.Importing;

namespace JAStudio.UI.Views.Importing;

/// <summary>
/// The window for editing <see cref="ImportProfile"/>s: see <see cref="ImportProfilesWindowViewModel"/>. Checking and running a profile happen in<br/>
/// its <see cref="ImportRunWindow"/>.
/// </summary>
partial class ImportProfilesWindow : Window
{
#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportProfilesWindow() => InitializeComponent();
#pragma warning restore CS8618

   ImportProfilesWindow(TemporaryServiceCollection services)
   {
      DataContext = new ImportProfilesWindowViewModel(services, profile => ImportRunWindow.CheckInItsWindow(profile, services));
      InitializeComponent();
   }

   /// <summary>Opens the window where it was last, as from the workspace's main menu, and returns it.</summary>
   internal static ImportProfilesWindow Open(TemporaryServiceCollection services) => new ImportProfilesWindow(services).ShowWhereItWasLast(services);
}
