using System;
using System.Collections.Generic;
using Avalonia.Controls;
using JAStudio.Core;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.Views.Workspace.Editors.Matching;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.Workspace.Editors;

/// <summary>
/// The open editor windows of one kind (<see cref="IEditorWindowViewModel"/>), such as the shared-conditions' (<see cref="SharedConditionWindow"/>): one<br/>
/// window at most for each thing edited, so that opening the window of one whose window is open brings that window to the front.
/// </summary>
/// <param name="whatIsEditedEquals">
/// When two things edited are the same one, as a shared-condition is the same object, since a rename changes what it equals. Their own equality when<br/>
/// none is given.
/// </param>
sealed class EditorWindows<TEdited, TWindow>(IEqualityComparer<TEdited>? whatIsEditedEquals = null) where TEdited : notnull where TWindow : Window
{
   readonly Dictionary<TEdited, TWindow> _openWindows = new(whatIsEditedEquals);

   /// <summary>
   /// Opens the window of <paramref name="edited"/>, made by <paramref name="newWindow"/>, where it was last, or brings it to the front when it's open<br/>
   /// already, and returns it.
   /// </summary>
   internal TWindow Open(TEdited edited, Func<TWindow> newWindow, TemporaryServiceCollection services)
   {
      if(_openWindows.TryGetValue(edited, out var open))
      {
         open.Activate();
         return open;
      }

      var window = newWindow().ShowWhereItWasLast(services);
      _openWindows.Add(edited, window);
      window.Closed += (_, _) => _openWindows.Remove(edited);
      return window;
   }
}
