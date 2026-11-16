using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using JAStudio.UI.ViewModels.Workspace.Navigation.ParsingCases;

namespace JAStudio.UI.Views.Workspace.Navigation.ParsingCases;

/// <summary>
/// The view of a <see cref="ParsingCasesViewModel"/>: the navigation pane's list of the corpus's parsing cases, in a grid, with its filters above it<br/>
/// and the selected case's why and differences below it. It turns clicks and keys on the grid into opening a case's sentence.
/// </summary>
/// <remarks>
/// Clicking a column header sorts by that column, and Shift+click adds a column to sort by when the earlier ones are equal. The grid starts out<br/>
/// sorted by how the cases parse, those that fail first, then by their names. Enter or a double-click opens a case's sentence, and middle-clicking<br/>
/// opens it in a background tab.
/// </remarks>
public partial class ParsingCasesView : UserControl
{
   public ParsingCasesView()
   {
      InitializeComponent();
      SortByTheOutcomeThenTheNameAtFirst();
      CasesGrid.AddHandler(KeyDownEvent, OnKeyDownInTheGrid, RoutingStrategies.Tunnel);
      CasesGrid.AddHandler(DoubleTappedEvent, OnDoubleTappedInTheGrid, RoutingStrategies.Bubble, handledEventsToo: true);
      CasesGrid.AddHandler(PointerReleasedEvent, OnPointerReleasedInTheGrid, RoutingStrategies.Bubble, handledEventsToo: true);
   }

   /// <summary>Given by the navigation pane, which binds its data context to a <see cref="ParsingCasesViewModel"/>.</summary>
   ParsingCasesViewModel ViewModel => DataContext as ParsingCasesViewModel
                                   ?? throw new InvalidOperationException("The parsing cases are shown without their ParsingCasesViewModel.");

   /// <summary>
   /// Sorts the grid once it has its rows, which is when the grid's own sorted view of them exists. That happens once, since the grid shows the<br/>
   /// same collection for as long as it lives.
   /// </summary>
   void SortByTheOutcomeThenTheNameAtFirst() =>
      CasesGrid.PropertyChanged += (_, e) =>
      {
         if(e.Property != DataGrid.CollectionViewProperty || e.NewValue is not IDataGridCollectionView sortedRows) return;

         sortedRows.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(ListedParsingCaseViewModel.Outcome)));
         sortedRows.SortDescriptions.Add(DataGridSortDescription.FromPath(nameof(ListedParsingCaseViewModel.Name)));
      };

   /// <summary>Enter opens the selected case's sentence. Handling it here keeps the grid from moving to the next row, as it otherwise does on Enter.</summary>
   void OnKeyDownInTheGrid(object? sender, KeyEventArgs e)
   {
      if(e is not { Key: Key.Enter, KeyModifiers: KeyModifiers.None } || CasesGrid.SelectedItem is not ListedParsingCaseViewModel selected) return;

      ViewModel.Open(selected);
      e.Handled = true;
   }

   void OnDoubleTappedInTheGrid(object? sender, TappedEventArgs e)
   {
      if(CaseUnder(e) is not {} listedCase) return;

      ViewModel.Open(listedCase);
      e.Handled = true;
   }

   /// <summary>Middle-clicking a row opens its sentence in a background tab, as middle-clicking a link does.</summary>
   void OnPointerReleasedInTheGrid(object? sender, PointerReleasedEventArgs e)
   {
      if(e.InitialPressMouseButton == MouseButton.Middle && CaseUnder(e) is {} listedCase)
         ViewModel.OpenInBackgroundTab(listedCase);
   }

   static ListedParsingCaseViewModel? CaseUnder(RoutedEventArgs e) =>
      (e.Source as Visual)?.FindAncestorOfType<DataGridRow>(includeSelf: true)?.DataContext as ListedParsingCaseViewModel;
}
