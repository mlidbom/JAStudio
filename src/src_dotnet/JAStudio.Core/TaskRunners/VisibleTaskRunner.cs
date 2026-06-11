using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;

namespace JAStudio.Core.TaskRunners;

/// <summary>Task progress runner that shows live progress in the UI.</summary>
class VisibleTaskRunner(ScopeProgressPresenter presenter, bool allowCancel) : ITaskProgressRunner
{
   readonly ScopeProgressPresenter _presenter = presenter;
   readonly bool _allowCancel = allowCancel;

   public TResult RunIndeterminate<TResult>(string message, Func<TResult> action)
   {
      using var _ = this.Log().Info().Time(message);
      _presenter.ShowSpinnerForTaskNamed(message);

      var task = TaskCE.Run(action);
      _presenter.KeepUIResponsiveUntilTaskIsDone(task);
      return task.Result;
   }

   public async Task<TResult> RunIndeterminateAsync<TResult>(string message, Func<TResult> action)
   {
      using var _ = this.Log().Info().Time(message);
      _presenter.ShowSpinnerForTaskNamed(message);
      return await TaskCE.Run(action);
   }

   public List<TOutput> RunBatch<TInput, TOutput>(List<TInput> items, Func<TInput, TOutput> processItem, string message, ThreadCount threads)
   {
      var totalItems = items.Count;
      var results = new TOutput[totalItems];
      using var _ = this.Log().Info().Time($"{message} handled {items.Count} items ({threads.Threads} threads)");

      var viewModel = _presenter.ShowBatchProgress(message, totalItems);

      var completed = 0;
      var stopwatch = Stopwatch.StartNew();

      void UpdateProgress()
      {
         var current = Interlocked.Increment(ref completed);
         _presenter.UpdateBatchProgressDisplay(current, totalItems, stopwatch);
      }

      if(threads.IsSequential)
      {
         for(var i = 0; i < totalItems; i++)
         {
            if(_allowCancel && viewModel.WasCanceled)
            {
               this.Log().Info($"Operation canceled by user after {completed} of {totalItems} items");
               break;
            }

            results[i] = processItem(items[i]);
            UpdateProgress();
         }
      } else
      {
         Parallel.For(0,
                      totalItems,
                      threads.ParallelOptions,
                      i =>
                      {
                         if(_allowCancel && viewModel.WasCanceled) return;
                         results[i] = processItem(items[i]);
                         UpdateProgress();
                      });
      }

      return [..results];
   }

   public async Task<List<TOutput>> RunBatchAsync<TInput, TOutput>(List<TInput> items, Func<TInput, TOutput> processItem, string message, ThreadCount threads) =>
      await TaskCE.Run(() => RunBatch(items, processItem, message, threads));

   public void Dispose() => _presenter.Dispose();
}
