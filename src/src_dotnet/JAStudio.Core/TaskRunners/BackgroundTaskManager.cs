using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.Internals.Logging;
using Compze.Threading;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;

namespace JAStudio.Core.TaskRunners;

/// <summary>Executes an action on background threads, logs on exceptions, and invokes fatal error to surface the error to the UI.</summary>
public class BackgroundTaskManager : IDisposable
{
   static readonly ILogger Log = CompzeLogger.For(typeof(BackgroundTaskManager));
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly IMonitor _monitor = IMonitor.New();
   readonly List<Task> _pendingTasks = [];

   internal BackgroundTaskManager(IFatalErrorHandler fatalErrorHandler) => _fatalErrorHandler = fatalErrorHandler;

   /// <summary> Run an action on a background thread and guarantee that exceptions will be surfaced. </summary>
   public void Run(Action action)
   {
      var task = TaskCE.Run(() =>
      {
         try
         {
            action();
         }
         catch(Exception ex)
         {
            HandleException(ex);
         }
      });

      TrackTask(task);
   }

   /// <summary> Run an async action on a background thread and guarantee that exceptions will be surfaced. </summary>
   public void RunAsync(Func<Task> action)
   {
      var task = TaskCE.Run(async () =>
      {
         try
         {
            await action();
         }
         catch(OperationCanceledException)
         {
            // Expected for debounced/cancelled work — not an error.
         }
         catch(Exception ex)
         {
            HandleException(ex);
         }
      });

      TrackTask(task);
   }

   void TrackTask(Task task)
   {
      _monitor.Locked(() => _pendingTasks.Add(task));
      task.ContinueWith(_ => _monitor.Locked(() => _pendingTasks.Remove(task)), TaskScheduler.Default);
   }

   void HandleException(Exception ex)
   {
      Log.Error(ex, "Unhandled exception in background task:");
      _fatalErrorHandler.Handle(ex);
   }

   public void Dispose() => _monitor.Locked(() =>
   {
      var tasks = _pendingTasks.Where(t => !t.IsCompleted).ToArray();
      _pendingTasks.Clear();
      Task.WaitAll(tasks);
   });
}
