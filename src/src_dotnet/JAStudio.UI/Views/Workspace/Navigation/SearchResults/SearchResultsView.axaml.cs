using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using JAStudio.UI.Menus;
using JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;
using JAStudio.UI.ViewModels.Workspace.Tagging;
using JAStudio.UI.Views.Workspace.Tagging;

namespace JAStudio.UI.Views.Workspace.Navigation.SearchResults;

/// <summary>
/// The view of a <see cref="SearchResultsViewModel"/>: the notes a search found, in a grid per note type, below the search's condition tree.<br/>
/// It turns clicks and keys on a grid into opening notes, and shows the menu of the selected notes.
/// </summary>
/// <remarks>
/// Clicking a column header sorts by that column, Shift+click adds a column to sort by when the earlier ones are equal, and Ctrl+click stops sorting.<br/>
/// A grid starts out sorted by the question. Ctrl/Shift+click and Ctrl+A select several notes. Enter or a double-click opens a note,<br/>
/// middle-clicking opens it in a background tab, and right-clicking, or the menu key, shows the menu of the selected notes.<br/>
/// Hovering anywhere over a cell shows its whole text, which a narrow column cuts off with an ellipsis (<see cref="TextColumnShowingTheWholeTextOnHover"/>).<br/>
/// The studying columns' headers are R and L, with their full names on hover anywhere over the header, since the columns are only check boxes.
/// </remarks>
public partial class SearchResultsView : UserControl
{
   public SearchResultsView()
   {
      InitializeComponent();
      foreach(var grid in new[] { VocabGrid, KanjiGrid, SentencesGrid })
      {
         SortByTheQuestionAtFirst(grid);
         ShowTheHeaderTooltipsOverTheWholeHeadersOf(grid);
         BindTheTaggingKeysOf(grid);
         grid.AddHandler(KeyDownEvent, (_, e) => OnKeyDownIn(grid, e), RoutingStrategies.Tunnel);
         grid.AddHandler(DoubleTappedEvent, OnDoubleTappedInAGrid, RoutingStrategies.Bubble, handledEventsToo: true);
         grid.AddHandler(PointerReleasedEvent, OnPointerReleasedInAGrid, RoutingStrategies.Bubble, handledEventsToo: true);
         grid.ContextRequested += (_, e) => OnContextRequestedIn(grid, e);
      }
   }

   /// <summary>Given by the view it's part of, which binds its data context to a <see cref="SearchResultsViewModel"/>.</summary>
   SearchResultsViewModel ViewModel => DataContext as SearchResultsViewModel
                                    ?? throw new InvalidOperationException("The search results are shown without their SearchResultsViewModel.");

   /// <summary>The results in the grid being used: those of the type shown, since the other types' grids are hidden.</summary>
   SearchResultsOfOneTypeViewModel ShownResults => ViewModel.Shown
                                                ?? throw new InvalidOperationException("A results grid was used while no results are shown.");

   /// <summary>
   /// Sorts <paramref name="grid"/> by its question column once the grid has its rows, which is when the grid's own sorted view of them exists.<br/>
   /// That happens once, since each grid shows the same collection for as long as it lives, whose rows are replaced in place.
   /// </summary>
   static void SortByTheQuestionAtFirst(DataGrid grid) =>
      grid.PropertyChanged += (_, e) =>
      {
         if(e.Property == DataGrid.CollectionViewProperty && e.NewValue is IDataGridCollectionView sortedRows)
            sortedRows.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(SearchResultViewModel.Question)));
      };

   /// <summary>
   /// Moves the tooltip of each column header given one, such as the R header's "Studying reading", from the header's own content<br/>
   /// to the header cell that shows it, so that it shows wherever over the header the mouse is, not only over the letter.
   /// </summary>
   /// <remarks>
   /// The grid has no tooltip for a column's header, and creates the header cells itself, so the tooltip moves once the content is in its cell.<br/>
   /// The full name is read before the move, since the content's own tooltip is gone after it, and a header cell made anew gets it again.
   /// </remarks>
   static void ShowTheHeaderTooltipsOverTheWholeHeadersOf(DataGrid grid)
   {
      foreach(var headerContent in grid.Columns.Select(it => it.Header).OfType<Control>())
      {
         if(ToolTip.GetTip(headerContent) is not {} tooltip) continue;

         ToolTip.SetTip(headerContent, null);
         headerContent.AttachedToVisualTree += (_, _) => ToolTip.SetTip(TheHeaderCellShowing(headerContent), tooltip);
      }
   }

   static DataGridColumnHeader TheHeaderCellShowing(Control headerContent) =>
      headerContent.FindAncestorOfType<DataGridColumnHeader>() ?? throw new InvalidOperationException("A column header's content is shown outside a header cell.");

   /// <summary>Enter opens the note on the current row. Handling it here keeps the grid from moving to the next row, as it otherwise does on Enter.</summary>
   void OnKeyDownIn(DataGrid grid, KeyEventArgs e)
   {
      if(e is not { Key: Key.Enter, KeyModifiers: KeyModifiers.None } || grid.SelectedItem is not SearchResultViewModel current) return;

      ShownResults.Open(current);
      e.Handled = true;
   }

   /// <summary>
   /// Ctrl+T adds a tag to the selected notes, and Ctrl+Shift+T removes one (<see cref="ActionsOnSelectedResults"/>). With no row selected they do nothing.<br/>
   /// The window binds the same keys to tag the note its shown tab shows. Key bindings are tried from the control with the keyboard outwards,<br/>
   /// before the key reaches any control, so the grid's own bindings are the ones that run while it has the keyboard.
   /// </summary>
   void BindTheTaggingKeysOf(DataGrid grid)
   {
      BindTheKey(new KeyGesture(Key.T, KeyModifiers.Control), selected => ViewModel.ActionsOnTheSelected.AddATagTo(selected, dialog => PickATagInADialog(grid, dialog)));
      BindTheKey(new KeyGesture(Key.T, KeyModifiers.Control | KeyModifiers.Shift), selected => ViewModel.ActionsOnTheSelected.RemoveATagFrom(selected, dialog => PickATagInADialog(grid, dialog)));
      return;

      void BindTheKey(KeyGesture gesture, Action<List<SearchResultViewModel>> actOnTheSelected) =>
         grid.KeyBindings.Add(new KeyBinding
                              {
                                 Gesture = gesture,
                                 Command = new RelayCommand(() =>
                                 {
                                    if(SelectedIn(grid) is { Count: > 0 } selected)
                                       actOnTheSelected(selected);
                                 })
                              });
   }

   void OnDoubleTappedInAGrid(object? sender, TappedEventArgs e)
   {
      if(ResultUnder(e) is not {} result) return;

      ShownResults.Open(result);
      e.Handled = true;
   }

   /// <summary>Middle-clicking a row opens its note in a background tab, as middle-clicking a link does.</summary>
   void OnPointerReleasedInAGrid(object? sender, PointerReleasedEventArgs e)
   {
      if(e.InitialPressMouseButton == MouseButton.Middle && ResultUnder(e) is {} result)
         ShownResults.OpenInBackgroundTab(result);
   }

   /// <summary>
   /// Shows the menu of the selected notes. Right-clicking a row that isn't selected selects just that row first, as in a file explorer.<br/>
   /// From the menu key the menu opens at the current row, or at the grid's top left when that row is scrolled out of sight.
   /// </summary>
   void OnContextRequestedIn(DataGrid grid, ContextRequestedEventArgs e)
   {
      e.Handled = true;

      if(ResultUnder(e) is {} clicked && !grid.SelectedItems.Contains(clicked))
         grid.SelectedItem = clicked;

      var selected = SelectedIn(grid);
      if(selected.Count == 0) return;

      var menuTopLeft = e.TryGetPosition(grid, out var pointer) ? grid.PointToScreen(pointer) : BelowTheCurrentRowOf(grid);
      var menu = ViewModel.ActionsOnTheSelected.Menu(selected, dialog => PickATagInADialog(grid, dialog));
      Dispatcher.UIThread.Post(() => PopupMenuWindow.ShowAt(menuTopLeft, menu, whenClosedWithoutAPick: () => PutTheKeyboardBackOn(grid)));
   }

   /// <summary>Shows <paramref name="dialog"/> below the current row of <paramref name="grid"/>, and puts the keyboard back on the grid when it closes.</summary>
   async Task<string?> PickATagInADialog(DataGrid grid, TagPickerDialogViewModel dialog)
   {
      var tagPath = await TagPickerDialog.ShowAsync(OwnerWindow, dialog, BelowTheCurrentRowOf(grid));
      PutTheKeyboardBackOn(grid);
      return tagPath;
   }

   static void PutTheKeyboardBackOn(DataGrid grid) => Dispatcher.UIThread.Post(() => grid.Focus(), DispatcherPriority.Background);

   Window OwnerWindow => TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException("The search results are only shown in a window.");

   static List<SearchResultViewModel> SelectedIn(DataGrid grid) => [..grid.SelectedItems.OfType<SearchResultViewModel>()];

   static SearchResultViewModel? ResultUnder(RoutedEventArgs e) =>
      (e.Source as Visual)?.FindAncestorOfType<DataGridRow>(includeSelf: true)?.DataContext as SearchResultViewModel;

   static PixelPoint BelowTheCurrentRowOf(DataGrid grid) =>
      RowsShownIn(grid).FirstOrDefault(it => it.DataContext == grid.SelectedItem) is {} currentRow
         ? currentRow.PointToScreen(new Point(0, currentRow.Bounds.Height))
         : grid.PointToScreen(new Point(0, 0));

   static IEnumerable<DataGridRow> RowsShownIn(DataGrid grid) => grid.GetVisualDescendants().OfType<DataGridRow>();
}
