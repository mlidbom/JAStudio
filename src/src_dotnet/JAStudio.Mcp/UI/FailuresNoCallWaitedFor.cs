using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Avalonia.Threading;

namespace JAStudio.Mcp.UI;

/// <summary>
/// The failures of work the <see cref="HeadlessUI"/> did on its own, which no call to it was waiting for, such as handling a window's activation the<br/>
/// platform posted. Kept, such a failure fails the next call to the UI.
/// </summary>
/// <remarks>
/// Unhandled, such a failure would end the UI thread's loop, and every call to the UI would then wait forever.
/// </remarks>
sealed class FailuresNoCallWaitedFor
{
   readonly ConcurrentQueue<Exception> _failures = new();

   FailuresNoCallWaitedFor() {}

   /// <summary>Starts keeping the failures of the work the UI thread whose dispatcher is <paramref name="uiThread"/> does on its own.</summary>
   public static FailuresNoCallWaitedFor KeptFrom(Dispatcher uiThread)
   {
      var failures = new FailuresNoCallWaitedFor();
      uiThread.UnhandledException += failures.Keep;
      return failures;
   }

   /// <summary>Throws the failures kept since the last were thrown, all in one, if any were.</summary>
   public void ThrowAnyKept()
   {
      List<Exception> failures = [];
      while(_failures.TryDequeue(out var failure)) failures.Add(failure);
      if(failures.Count > 0)
         throw new AggregateException("JAStudio's headless UI failed in work it did on its own, which no call to it was waiting for, such as handling an event.", failures);
   }

   /// <summary>Keeps the failure of work the UI did on its own, rather than letting it end the UI thread's loop, so the next call fails with it.</summary>
   void Keep(object sender, DispatcherUnhandledExceptionEventArgs failure)
   {
      _failures.Enqueue(failure.Exception);
      failure.Handled = true;
   }
}
