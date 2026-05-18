using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Compze.Internals.Logging;

namespace JAStudio.Core.TaskRunners;

/// <summary>
/// A scoped task runner obtained from <see cref="TaskRunner.Current"/>.
/// Each method call creates its own progress panel for the duration of that call.
/// The scope owns a <see cref="TaskProgressScopeViewModel"/> that displays a heading
/// and elapsed time, and logs total elapsed time on dispose.
/// The dialog window is held open by <see cref="TaskRunner"/> from the outermost
/// scope until it is disposed, so panels can come and go without the window
/// flickering or repositioning.
/// </summary>
class TaskRunnerScope : ITaskProgressRunner
{
   readonly TaskRunner _taskRunner;
   readonly bool _visible;
   readonly bool _allowCancel;
   readonly Stopwatch _stopwatch = Stopwatch.StartNew();
   readonly int _depth;
   readonly int _previousNestingDepth;
   readonly TaskProgressScopeViewModel? _previousParentScopeViewmodel;
   readonly TaskLogEntry? _previousLogEntry;
   bool _disposed;

   internal TaskProgressScopeViewModel? ScopeViewModel { get; }

   internal TaskLogEntry LogEntry { get; }

   internal TaskRunnerScope(TaskRunner taskRunner, string scopeTitle, bool visible, bool allowCancel, int depth, int previousNestingDepth, TaskProgressScopeViewModel? previousParentScopeViewmodel, TaskLogEntry? parentLogEntry, TaskProgressScopeViewModel? scopeViewModel)
   {
      _taskRunner = taskRunner;
      _allowCancel = allowCancel;
      _visible = visible;
      _depth = depth;
      _previousNestingDepth = previousNestingDepth;
      _previousParentScopeViewmodel = previousParentScopeViewmodel;
      _previousLogEntry = parentLogEntry;
      LogEntry = new TaskLogEntry(scopeTitle);
      parentLogEntry?.AddChild(LogEntry);
      ScopeViewModel = scopeViewModel;
   }

   public List<TOutput> RunBatch<TInput, TOutput>(List<TInput> items, Func<TInput, TOutput> processItem, string message, ThreadCount threads)
   {
      var taskLogEntry = new TaskLogEntry(message);
      LogEntry.AddChild(taskLogEntry);
      using var runner = _taskRunner.CreateRunner(ScopeViewModel, message, _visible, _allowCancel);
      var result = runner.RunBatch(items, processItem, message, threads);
      taskLogEntry.MarkCompleted();
      return result;
   }

   public TResult RunIndeterminate<TResult>(string message, Func<TResult> action)
   {
      var taskLogEntry = new TaskLogEntry(message);
      LogEntry.AddChild(taskLogEntry);
      using var runner = _taskRunner.CreateRunner(ScopeViewModel, message, _visible, _allowCancel);
      var result = runner.RunIndeterminate(message, action);
      taskLogEntry.MarkCompleted();
      return result;
   }

   public async Task<List<TOutput>> RunBatchAsync<TInput, TOutput>(List<TInput> items, Func<TInput, TOutput> processItem, string message, ThreadCount threadCount)
   {
      var taskLogEntry = new TaskLogEntry($"(async) {message}");
      LogEntry.AddChild(taskLogEntry);
      using var runner = _taskRunner.CreateRunner(ScopeViewModel, message, _visible, _allowCancel);
      var result = await runner.RunBatchAsync(items, processItem, message, threadCount);
      taskLogEntry.MarkCompleted();
      return result;
   }

   public async Task<TResult> RunIndeterminateAsync<TResult>(string message, Func<TResult> action)
   {
      var taskLogEntry = new TaskLogEntry($"(async) {message}");
      LogEntry.AddChild(taskLogEntry);
      using var runner = _taskRunner.CreateRunner(ScopeViewModel, message, _visible, _allowCancel);
      var result = await runner.RunIndeterminateAsync(message, action);
      taskLogEntry.MarkCompleted();
      return result;
   }

   public void Dispose()
   {
      if(_disposed) return;
      _disposed = true;

      _stopwatch.Stop();
      LogEntry.MarkCompleted();

      if(_depth == 1)
      {
         this.Log().Info($"{Environment.NewLine}{LogEntry.FormatTree()}");
      }

      _taskRunner.OnScopeDisposed(ScopeViewModel, _visible, _previousNestingDepth, _previousParentScopeViewmodel, _previousLogEntry);
   }
}
