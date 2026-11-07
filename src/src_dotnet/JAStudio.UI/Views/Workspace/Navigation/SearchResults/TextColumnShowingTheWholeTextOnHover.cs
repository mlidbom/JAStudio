using System;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace JAStudio.UI.Views.Workspace.Navigation.SearchResults;

/// <summary>
/// A grid column of text that ends text cut off at a cell's edge in an ellipsis, and shows the cell's whole text in a tooltip<br/>
/// wherever over the cell the mouse is. For the results grid's columns, which are often too narrow for their text.
/// </summary>
/// <remarks>
/// A tooltip on the cell's text would respond only over the text itself, a fraction of a cell with short text.<br/>
/// So the tooltip goes on the cell, which responds over its whole area, bound to the text, so that it follows the cell<br/>
/// when the grid reuses the cell for another row. An empty cell shows no tooltip.
/// </remarks>
class TextColumnShowingTheWholeTextOnHover : DataGridTextColumn
{
   protected override Control GenerateElement(DataGridCell cell, object dataItem)
   {
      if(base.GenerateElement(cell, dataItem) is not TextBlock text)
         throw new InvalidOperationException($"A {nameof(DataGridTextColumn)} was expected to show its cells' text in a {nameof(TextBlock)}.");

      text.TextTrimming = TextTrimming.CharacterEllipsis;
      cell.Bind(ToolTip.TipProperty, new Binding(nameof(TextBlock.Text)) { Source = text });
      cell.Bind(ToolTip.ServiceEnabledProperty, new Binding(nameof(TextBlock.Text)) { Source = text, Converter = StringConverters.IsNotNullOrEmpty });
      return text;
   }
}
