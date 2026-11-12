using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JAStudio.UI.ViewModels.Workspace.Editors;

namespace JAStudio.UI.Views.Workspace.Editors;

/// <summary>
/// The view of a <see cref="VocabMatchingConfigEditorViewModel"/>: the "Matching" tab of a vocab's <see cref="VocabEditorSection"/>. A double-click on the<br/>
/// match-condition opens its window, where it's edited as a tree.
/// </summary>
public partial class VocabMatchingConfigEditor : UserControl
{
   public VocabMatchingConfigEditor()
   {
      InitializeComponent();
      MatchCondition.AddHandler(DoubleTappedEvent, OnDoubleTappedOnTheMatchCondition, RoutingStrategies.Bubble, handledEventsToo: true);
   }

   VocabMatchingConfigEditorViewModel ViewModel =>
      DataContext as VocabMatchingConfigEditorViewModel ?? throw new InvalidOperationException("A VocabMatchingConfigEditor shows a VocabMatchingConfigEditorViewModel.");

   void OnDoubleTappedOnTheMatchCondition(object? sender, TappedEventArgs e)
   {
      e.Handled = true;
      ViewModel.OpenTheMatchConditionWindow();
   }
}
