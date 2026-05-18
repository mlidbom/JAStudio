using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JAStudio.Core.SysUtils;
using JetBrains.Annotations;
// ReSharper disable UnusedMethodReturnValue.Global

namespace JAStudio.Core.TaskRunners;

public interface ITaskProgressRunner : IDisposable
{
   void RunBatch<TInput>(List<TInput> items, [InstantHandle] Action<TInput> processItem, string message) => RunBatch(items, processItem.AsFunc(), message, ThreadCount.One);
   void RunBatch<TInput>(List<TInput> items, [InstantHandle] Action<TInput> processItem, string message, ThreadCount threads) => RunBatch(items, processItem.AsFunc(), message, threads);
   public List<TOutput> RunBatch<TInput, TOutput>(List<TInput> items, [InstantHandle] Func<TInput, TOutput> processItem, string message) => RunBatch(items, processItem, message, ThreadCount.One);
   List<TOutput> RunBatch<TInput, TOutput>(List<TInput> items, [InstantHandle] Func<TInput, TOutput> processItem, string message, ThreadCount threads);

   Task<List<TOutput>> RunBatchAsync<TInput, TOutput>(List<TInput> items, Func<TInput, TOutput> processItem, string message) => RunBatchAsync(items, processItem, message, ThreadCount.One);
   Task<List<TOutput>> RunBatchAsync<TInput, TOutput>(List<TInput> items, Func<TInput, TOutput> processItem, string message, ThreadCount threadCount);

   Task RunBatchAsync<TInput>(List<TInput> items, Action<TInput> processItem, string message) => RunBatchAsync(items, processItem.AsFunc(), message, ThreadCount.One);
   Task RunBatchAsync<TInput>(List<TInput> items, Action<TInput> processItem, string message, ThreadCount threadCount) => RunBatchAsync(items, processItem.AsFunc(), message, threadCount);

   void RunIndeterminate(string message, [InstantHandle] Action action) => RunIndeterminate(message, action.AsFunc());
   TResult RunIndeterminate<TResult>(string message, [InstantHandle] Func<TResult> action);
   Task<TResult> RunIndeterminateAsync<TResult>(string message, Func<TResult> action);
   Task RunIndeterminateAsync(string message, Action action) => RunIndeterminateAsync(message, action.AsFunc());
}
