using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Importing;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Views.Importing;

/// <summary>
/// The window that checks an <see cref="ImportProfile"/>, reports on it and runs it: see <see cref="ImportRunWindowViewModel"/>.<br/>
/// A profile has one at most, so checking it again brings its window to the front rather than opening another.
/// </summary>
partial class ImportRunWindow : Window
{
   static readonly Dictionary<ImportProfile, ImportRunWindow> OpenWindowsByProfile = [];

   readonly ImportRunWindowViewModel _viewModel;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportRunWindow() => InitializeComponent();
#pragma warning restore CS8618

   ImportRunWindow(ImportProfile profile, TemporaryServiceCollection services)
   {
      _viewModel = new ImportRunWindowViewModel(profile, services, WorkspaceWindow.ShowNoteList, Close);
      DataContext = _viewModel;
      InitializeComponent();
      _viewModel.PropertyChanged += (_, changed) =>
      {
         if(changed.PropertyName == nameof(ImportRunWindowViewModel.ReportedFieldMapping)) ShowTheColumnsOfTheMappedFields();
      };
      Closed += (_, _) =>
      {
         OpenWindowsByProfile.Remove(profile);
         _viewModel.Dispose();
      };
   }

   /// <summary>
   /// Checks <paramref name="profile"/> in its import run window: in the one already open, brought to the front, or in a new one opened near the cursor.
   /// </summary>
   internal static void CheckInItsWindow(ImportProfile profile, TemporaryServiceCollection services)
   {
      if(OpenWindowsByProfile.TryGetValue(profile, out var openWindow))
      {
         openWindow._viewModel.CheckAgainUnlessBusy();
         openWindow.BringToFront();
      }
      else
         OpenWindowsByProfile[profile] = new ImportRunWindow(profile, services).ShowNearCursor();
   }

   void BringToFront()
   {
      if(WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
      Activate();
   }

   /// <summary>
   /// Shows the report's columns of the fields the reported profile maps, and hides the others. A DataGrid's columns aren't in the visual tree,<br/>
   /// so no binding reaches their visibility.
   /// </summary>
   void ShowTheColumnsOfTheMappedFields()
   {
      var mapping = _viewModel.ReportedFieldMapping;
      ColumnHeaded("External id").IsVisible = mapping?.ExternalId != "";
      ColumnHeaded("Audio").IsVisible = mapping?.Audio != "";
      ColumnHeaded("Screenshot").IsVisible = mapping?.Screenshot != "";
      return;

      DataGridColumn ColumnHeaded(string header) => ReportGrid.Columns.Single(it => Equals(it.Header, header));
   }
}
