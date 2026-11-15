using System;
using Avalonia.Controls;
using Avalonia.Input;
using JAStudio.UI.ViewModels.Studying;

namespace JAStudio.UI.Views.Studying;

/// <summary>
/// The view of a <see cref="SyncButtonViewModel"/>, the Sync button: two arrows in a circle, which turn while a sync is under way, and a green<br/>
/// check mark instead once the last sync has finished. A right-click opens the Studying window. The window it's in says beside it why a sync<br/>
/// couldn't run (<see cref="SyncButtonViewModel.WhyTheSyncDidNotRun"/>). It shows only while there is a spaced repetition system to sync with.
/// </summary>
partial class SyncButton : UserControl
{
   public SyncButton() => InitializeComponent();

   /// <summary>Given by the window the button is in, which binds its data context to its <see cref="SyncButtonViewModel"/>.</summary>
   SyncButtonViewModel ViewModel => DataContext as SyncButtonViewModel
                                 ?? throw new InvalidOperationException("The Sync button is shown without its SyncButtonViewModel.");

   /// <summary>A right-click on the button, or the keyboard's menu key: what would open a context menu opens the Studying window.</summary>
   void OnContextRequested(object? sender, ContextRequestedEventArgs e)
   {
      ViewModel.OpenTheStudyingWindowCommand.Execute(null);
      e.Handled = true;
   }
}
