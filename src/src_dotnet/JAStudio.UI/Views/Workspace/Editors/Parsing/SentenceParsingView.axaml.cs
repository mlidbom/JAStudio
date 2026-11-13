using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using JAStudio.UI.Menus;
using JAStudio.UI.ViewModels.Workspace.Editors.Parsing;

namespace JAStudio.UI.Views.Workspace.Editors.Parsing;

/// <summary>
/// The view of a <see cref="SentenceParsingViewModel"/>: how a sentence parsed, every match indented under the shown match that covers it, in the<br/>
/// sentence's editor section. Right-clicking a match gives its menu.
/// </summary>
/// <remarks>This code-behind only opens the menu of the match right-clicked.</remarks>
public partial class SentenceParsingView : UserControl
{
   public SentenceParsingView()
   {
      InitializeComponent();
      ContextRequested += ShowTheMenuOfTheMatchRightClicked;
   }

   SentenceParsingViewModel Parsing => DataContext as SentenceParsingViewModel ?? throw new InvalidOperationException("A sentence's parsing view shows no parsing view model.");

   /// <summary>Opens the menu of the match right-clicked, where it was right-clicked, or at the view's top left from the menu key. A right-click on no match opens none.</summary>
   void ShowTheMenuOfTheMatchRightClicked(object? sender, ContextRequestedEventArgs requested)
   {
      if(TheMatchRightClicked(requested.Source) is not {} match) return;

      requested.Handled = true;
      var menuTopLeft = this.PointToScreen(requested.TryGetPosition(this, out var pointer) ? pointer : new Point(0, 0));
      PopupMenuWindow.ShowAt(menuTopLeft, Parsing.MenuOf(match, ""));
   }

   /// <summary>The match whose row <paramref name="source"/>, the control right-clicked, is in, if any.</summary>
   static ParsedMatchViewModel? TheMatchRightClicked(object? source) =>
      (source as Visual)?.GetSelfAndVisualAncestors().Select(it => it.DataContext).OfType<ParsedMatchViewModel>().FirstOrDefault();
}
