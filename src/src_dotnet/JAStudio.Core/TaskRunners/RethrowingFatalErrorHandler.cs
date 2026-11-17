using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Compze.Contracts;

namespace JAStudio.Core.TaskRunners;

/// <summary>
/// The tests' <see cref="IFatalErrorHandler"/>: it throws each exception it's handed on, wrapped, so that the code that handed it fails the test,<br/>
/// and keeps every one in <see cref="HandledExceptions"/>. It also keeps the work it starts, so that a test ends only once that work has finished<br/>
/// (<see cref="WaitUntilTheStartedWorkHasFinished"/>).
/// </summary>
/// <remarks>
/// A background task's exception thrown on goes nowhere: nothing waits for the task. <see cref="HandledExceptions"/> is how a test sees it.
/// </remarks>
/// <remarks>
/// Work started and never awaited can outlive the test that started it. A search the search editor started, and a later search superseded, reads<br/>
/// the notes on after the later one has finished, and so after the test, waiting for that, has disposed the app's container. It then fails on the<br/>
/// disposed container, in no test.
/// </remarks>
public class RethrowingFatalErrorHandler : IFatalErrorHandler
{
   static readonly TimeSpan LongestTheStartedWorkMayTake = TimeSpan.FromSeconds(30);

   readonly ConcurrentQueue<Exception> _handledExceptions = new();
   readonly ConcurrentQueue<Task> _startedWork = new();

   /// <summary>Every exception handed to <see cref="Handle"/> so far, in the order they were handed to it.</summary>
   public IReadOnlyList<Exception> HandledExceptions => [.. _handledExceptions];

   public void Handle(Exception exception)
   {
      _handledExceptions.Enqueue(exception);
      throw new InvalidOperationException("Unhandled exception in background task during testing.", exception);
   }

   /// <summary>
   /// Starts <paramref name="asyncWork"/> without waiting for it to finish, as <see cref="IFatalErrorHandler.StartAndHandleExceptionsFrom"/> does, and<br/>
   /// keeps it for <see cref="WaitUntilTheStartedWorkHasFinished"/>.
   /// </summary>
   public void StartAndHandleExceptionsFrom(Func<Task> asyncWork) => _startedWork.Enqueue(RunAndHandleExceptionsFrom(asyncWork));

   /// <summary>
   /// Waits until the work <see cref="StartAndHandleExceptionsFrom"/> started has finished, and throws what <see cref="Handle"/> threw for it, so<br/>
   /// that the test that started it fails.
   /// </summary>
   public void WaitUntilTheStartedWorkHasFinished()
   {
      var hasFinished = Task.WhenAll(_startedWork).Wait(LongestTheStartedWorkMayTake);
      Contract.State.Assert(hasFinished, () => $"The work the test started through {nameof(StartAndHandleExceptionsFrom)} hasn't finished after {LongestTheStartedWorkMayTake.TotalSeconds} seconds.");
   }

   async Task RunAndHandleExceptionsFrom(Func<Task> asyncWork)
   {
      try
      {
         await asyncWork();
      }
      catch(Exception exception)
      {
         Handle(exception);
      }
   }
}
