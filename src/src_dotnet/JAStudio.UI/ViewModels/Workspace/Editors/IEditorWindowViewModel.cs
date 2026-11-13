using JAStudio.UI.ViewModels.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The view model of an editor window: a window of its own editing something the collection holds, such as a shared-condition<br/>
/// (<see cref="SharedConditionWindowViewModel"/>) or a vocab's match-condition (<see cref="MatchConditionWindowViewModel"/>), which may be changed<br/>
/// elsewhere while the window is open, or deleted: in another window, on a vocab's tabs, or through the MCP server.
/// </summary>
interface IEditorWindowViewModel
{
   //review:matching: "editor window" names what the windows of a shared-condition and of a vocab's match-condition have in common, which each
   //   implemented on its own before: one window at most for each thing edited (EditorWindows), showing it as it is now whenever it's activated, and
   //   closing when it's gone or the collection is loaded anew (KeepShowingWhatItEditsAsItIsNow). Each window is still named for what it's of.

   /// <summary>Whether what the window edits is no longer in the collection, as after it was deleted. The window then closes, since every change to it would fail.</summary>
   bool IsGone { get; }

   /// <summary>Shows what the window edits as it is now, when something else may have changed it while the window wasn't the active one.</summary>
   void ShowItAsItIsNow();
}
