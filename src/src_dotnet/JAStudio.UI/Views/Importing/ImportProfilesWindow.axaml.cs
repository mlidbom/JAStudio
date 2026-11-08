using System;
using System.Linq;
using Avalonia.Controls;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Importing;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Views.Importing;

/// <summary>The window for editing <see cref="ImportProfile"/>s, and for checking and running them: see <see cref="ImportProfilesWindowViewModel"/>.</summary>
partial class ImportProfilesWindow : Window
{
   readonly ImportProfilesWindowViewModel _viewModel;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public ImportProfilesWindow() => InitializeComponent();
#pragma warning restore CS8618

   ImportProfilesWindow(TemporaryServiceCollection services)
   {
      _viewModel = new ImportProfilesWindowViewModel(services, WorkspaceWindow.ShowNoteList);
      DataContext = _viewModel;
      InitializeComponent();
      _viewModel.PropertyChanged += (_, changed) =>
      {
         if(changed.PropertyName == nameof(ImportProfilesWindowViewModel.ReportedFieldMapping)) ShowTheColumnsOfTheMappedFields();
      };
   }

   /// <summary>Opens the window near the cursor, as from the main menu.</summary>
   internal static void OpenNearTheCursor(TemporaryServiceCollection services) => new ImportProfilesWindow(services).ShowNearCursor();

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
