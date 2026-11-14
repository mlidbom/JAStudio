using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JAStudio.Core.TaskRunners;

/// <summary>
/// The tests' <see cref="IFatalErrorHandler"/>: it throws each exception it's handed on, wrapped, so that the code that handed it fails the test,<br/>
/// and keeps every one in <see cref="HandledExceptions"/>.
/// </summary>
/// <remarks>
/// A background task's exception thrown on goes nowhere: nothing waits for the task. <see cref="HandledExceptions"/> is how a test sees it.
/// </remarks>
public class RethrowingFatalErrorHandler : IFatalErrorHandler
{
   readonly ConcurrentQueue<Exception> _handledExceptions = new();

   /// <summary>Every exception handed to <see cref="Handle"/> so far, in the order they were handed to it.</summary>
   public IReadOnlyList<Exception> HandledExceptions => [.. _handledExceptions];

   public void Handle(Exception exception)
   {
      _handledExceptions.Enqueue(exception);
      throw new InvalidOperationException("Unhandled exception in background task during testing.", exception);
   }
}
