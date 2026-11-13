using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
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
      CodeLines.AddHandler(TappedEvent, OnTappedOnTheCode, RoutingStrategies.Bubble, handledEventsToo: true);
   }

   ConditionCodeViewModel ViewModel => DataContext as ConditionCodeViewModel ?? throw new InvalidOperationException("A ConditionCodeView shows a ConditionCodeViewModel.");

   void OnTappedOnTheCode(object? sender, TappedEventArgs e)
   {
      if(!e.KeyModifiers.HasFlag(KeyModifiers.Control) || TheSharedConditionNamedWhereTheEventIs(e) is not {} sharedCondition) return;

      e.Handled = true;
      ViewModel.OpenTheDefinitionOf(sharedCondition);
   }

   /// <summary>The shared-condition whose name the event is on, as a click on the name is: none when it's on another piece of the code, or between them.</summary>
   static ISharedCondition? TheSharedConditionNamedWhereTheEventIs(RoutedEventArgs e) =>
      e.Source is Control { DataContext: CodePieceViewModel { SharedCondition: {} sharedCondition } } ? sharedCondition : null;
}
