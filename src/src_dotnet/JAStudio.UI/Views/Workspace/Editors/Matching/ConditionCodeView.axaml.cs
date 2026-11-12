using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;

namespace JAStudio.UI.Views.Workspace.Editors.Matching;

/// <summary>
/// The view of a <see cref="ConditionCodeViewModel"/>: a condition's code shown read-only, such as a vocab's match-condition on its Matching tab. Ctrl+click<br/>
/// on a shared-condition's name opens its window, as in an IDE, and hovering over the name shows its definition.
/// </summary>
public partial class ConditionCodeView : UserControl
{
   public ConditionCodeView()
   {
      InitializeComponent();
      CodeLines.AddHandler(TappedEvent, OnTapped, RoutingStrategies.Bubble, handledEventsToo: true);
   }

   ConditionCodeViewModel ViewModel => DataContext as ConditionCodeViewModel ?? throw new InvalidOperationException("A ConditionCodeView shows a ConditionCodeViewModel.");

   void OnTapped(object? sender, TappedEventArgs e)
   {
      if(!e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.Source is not Control { DataContext: CodePieceViewModel { SharedCondition: {} sharedCondition } }) return;

      e.Handled = true;
      ViewModel.OpenTheDefinitionOf(sharedCondition);
   }
}
