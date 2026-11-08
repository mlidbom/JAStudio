using System;
using Compze.Threading;

namespace JAStudio.Core.SysUtils.Threading;

/// <summary>
/// Lets one piece of work happen at a time, whichever thread asks: each waits for the one under way to finish, however long it takes.<br/>
/// Unlike a lock, no lock is held while the work runs, so work that takes minutes doesn't run into a lock timeout.
/// </summary>
class OneAtATime
{
   readonly IAwaitableMonitor _monitor = IAwaitableMonitor.New(waitTimeout: WaitTimeout.Infinite);
   bool _workIsUnderWay;

   /// <summary>Does <paramref name="work"/> once the work under way, if any, has finished, and returns its result.</summary>
   public TResult Do<TResult>(Func<TResult> work)
   {
      _monitor.UpdateWhen(() => !_workIsUnderWay, () => _workIsUnderWay = true);
      try
      {
         return work();
      }
      finally
      {
         _monitor.Update(() => _workIsUnderWay = false);
      }
   }
}
