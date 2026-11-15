using System;
using System.Collections.Generic;
using Avalonia.Controls;
using JAStudio.Core;
using JAStudio.Core.Studying;
using JAStudio.UI.ViewModels.Studying;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.Studying;

/// <summary>
/// The Studying window: the Sync button, which starts JAStudio's sync with Anki (<see cref="SRSSync"/>), what the last sync did, and the<br/>
/// study-notes in Anki that belong to no note: see <see cref="StudyingWindowViewModel"/>. A JAStudio has one at most, so opening it again<br/>
/// brings it to the front.
/// </summary>
partial class StudyingWindow : Window
{
   /// <summary>The open window of each JAStudio, by its services: one in the application, and one for each specification that opens the window.</summary>
   static readonly Dictionary<TemporaryServiceCollection, StudyingWindow> OpenWindowsByServices = [];

   [Obsolete("For the XAML designer/previewer only")]
   public StudyingWindow() => InitializeComponent();

   StudyingWindow(TemporaryServiceCollection services)
   {
      var viewModel = new StudyingWindowViewModel(services);
      DataContext = viewModel;
      InitializeComponent();
      Opened += (_, _) => SyncButton.Focus();
      Closed += (_, _) =>
      {
         OpenWindowsByServices.Remove(services);
         viewModel.Dispose();
      };
   }

   /// <summary>Opens the window where it was last, as from the workspace's main menu, or brings the one already open to the front, and returns it.</summary>
   internal static StudyingWindow Open(TemporaryServiceCollection services)
   {
      if(!OpenWindowsByServices.TryGetValue(services, out var openWindow))
         return OpenWindowsByServices[services] = new StudyingWindow(services).ShowWhereItWasLast(services);

      if(openWindow.WindowState == WindowState.Minimized) openWindow.WindowState = WindowState.Normal;
      openWindow.Activate();
      return openWindow;
   }
}
