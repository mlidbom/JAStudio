using System.Threading;

namespace JAStudio.Core.TaskRunners;

public class TaskRunner
{
   readonly DialogProgressPresenter _dialogPresenter;

   internal TaskRunner(DialogProgressPresenter dialogPresenter) => _dialogPresenter = dialogPresenter;

   /// <summary>Tracks the logical nesting depth per async call chain. Parallel siblings calling <see cref="Current"/> from the same parent scope each inherit the parent's depth and thus land at the same visual level.</summary>
   readonly AsyncLocal<int> _nestingDepth = new();

   /// <summary>Tracks the current parent scope view model per async call chain so that child scopes are nested inside their parent rather than appended to the top-level dialog. </summary>
   readonly AsyncLocal<TaskProgressScopeViewModel?> _parentScopeViewmodel = new();

   /// <summary>Tracks the current log entry per async call chain so that child scopes and tasks can attach their log entries to the parent. </summary>
   readonly AsyncLocal<TaskLogEntry?> _currentLogEntry = new();

   /// <summary>Returns a scope that implements <see cref="ITaskProgressRunner"/>. Nested calls share the same progress dialog window which stays open and in place until the outermost scope is disposed.
   /// Each individual method call on the scope gets its own progress panel within that dialog. Each scope gets its own heading panel and logs its total elapsed time.</summary>
   /// <param name="scopeTitle">The scope's heading in the progress dialog, and its name in the log.</param>
   /// <param name="forceHide">Shows no progress for this scope, nor for the scopes opened inside it: for a caller that shows its progress itself.</param>
   /// <param name="allowCancel">Whether the progress dialog offers to cancel the scope's work.</param>
   public ITaskProgressRunner Current(string scopeTitle, bool forceHide = false, bool allowCancel = true)
   {
      var visible = !CoreApp.IsTesting && !forceHide && !IsInsideHiddenScope;

      var previousNestingDepth = _nestingDepth.Value;
      _nestingDepth.Value += 1;

      if(visible)
         _dialogPresenter.ShowDialog();

      var scopeViewModel = visible ? _dialogPresenter.AddScope(scopeTitle, _nestingDepth.Value, _parentScopeViewmodel.Value) : null;

      var scope = new TaskRunnerScope(this, scopeTitle, visible, allowCancel, _nestingDepth.Value, previousNestingDepth, _parentScopeViewmodel.Value, _currentLogEntry.Value, scopeViewModel);
      _parentScopeViewmodel.Value = scopeViewModel;
      _currentLogEntry.Value = scope.LogEntry;
      return scope;
   }

   /// <summary>
   /// Whether the current call chain is inside a hidden scope, so that a scope opened now must be hidden too.<br/>
   /// A visible scope always has a view model in the progress dialog and a hidden one has none,<br/>
   /// so this is so exactly when there is an enclosing scope but no enclosing scope view model.
   /// </summary>
   bool IsInsideHiddenScope => _nestingDepth.Value > 0 && _parentScopeViewmodel.Value == null;

   internal ITaskProgressRunner CreateRunner(TaskProgressScopeViewModel? scopeViewModel, string labelText, bool? visible = null, bool allowCancel = true)
   {
      visible ??= !CoreApp.IsTesting;

      if(!visible.Value || scopeViewModel == null)
      {
         return new InvisibleTaskRunner(labelText);
      }

      return new VisibleTaskRunner(_dialogPresenter.CreateScopePresenter(scopeViewModel, allowCancel), allowCancel);
   }

   internal void OnScopeDisposed(TaskProgressScopeViewModel? disposedScopeViewmodel, bool visible, int previousNestingDepth, TaskProgressScopeViewModel? previousParentScopeViewmodel, TaskLogEntry? previousLogEntry)
   {
      if(disposedScopeViewmodel != null)
         _dialogPresenter.RemoveScope(disposedScopeViewmodel, previousParentScopeViewmodel);

      _nestingDepth.Value = previousNestingDepth;
      _parentScopeViewmodel.Value = previousParentScopeViewmodel;
      _currentLogEntry.Value = previousLogEntry;

      if(visible)
         _dialogPresenter.HideDialogIfNoOpenScopes();
   }
}
