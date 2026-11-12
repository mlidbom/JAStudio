using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using JAStudio.Core;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.Workspace.Editors.Matching;

/// <summary>
/// The window listing every one of the corpus's shared-conditions, showing a <see cref="SharedConditionsWindowViewModel"/>, as the workspace's main menu<br/>
/// opens it. Enter or a double-click opens the selected one's <see cref="SharedConditionWindow"/>, and each one's button lists the vocab depending on it in<br/>
/// the workspace's "List" tab. It lists them as they are now whenever it's activated, as after a change in one's window.
/// </summary>
partial class SharedConditionsWindow : Window
{
   readonly TemporaryServiceCollection _services;
   readonly SharedConditionsWindowViewModel _viewModel;

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public SharedConditionsWindow() => InitializeComponent();
#pragma warning restore CS8618

   SharedConditionsWindow(TemporaryServiceCollection services)
   {
      _services = services;
      DataContext = _viewModel = new SharedConditionsWindowViewModel(services, WorkspaceWindow.ShowNoteList);
      InitializeComponent();
      Activated += (_, _) => _viewModel.ShowThemAsTheyAreNow();
      SharedConditionList.AddHandler(KeyDownEvent, OnKeyDownInTheList, RoutingStrategies.Tunnel);
      SharedConditionList.DoubleTapped += (_, _) => OpenTheSelectedOne();
   }

   /// <summary>Opens the window where it was last, as from the workspace's main menu, and returns it.</summary>
   internal static SharedConditionsWindow Open(TemporaryServiceCollection services) => new SharedConditionsWindow(services).ShowWhereItWasLast(services);

   void OnKeyDownInTheList(object? sender, KeyEventArgs e)
   {
      if(e.Key != Key.Enter) return;

      e.Handled = true;
      OpenTheSelectedOne();
   }

   void OpenTheSelectedOne()
   {
      if(_viewModel.Selected is {} selected)
         SharedConditionWindow.Open(selected.SharedCondition, _services);
   }
}
