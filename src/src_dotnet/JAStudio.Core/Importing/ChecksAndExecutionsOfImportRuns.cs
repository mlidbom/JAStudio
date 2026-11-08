using System;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Threading;
using JAStudio.Core.SysUtils.Threading;

namespace JAStudio.Core.Importing;

/// <summary>
/// Lets one check or execution of an <see cref="ImportRun"/> happen at a time, whichever window or <see cref="AutomaticImportRuns"/> asks for it,<br/>
/// so that a check never sees an execution half done and two runs never execute together. It counts the executions, so that a run knows when<br/>
/// another was executed after its check: <see cref="ImportRun.IsOutOfDate"/>.
/// </summary>
class ChecksAndExecutionsOfImportRuns
{
   readonly OneAtATime _oneAtATime = new();
   readonly SimpleObservable<ImportRun> _runExecuted = new();
   readonly IMonitor _monitor = IMonitor.New();
   int _numberOfExecutedRuns;

   /// <summary>Notifies every run that was executed, on the thread that executed it.</summary>
   public IObservable<ImportRun> RunExecuted => _runExecuted;

   /// <summary>How many runs were executed so far. A run notes it at its check, and is out of date once it has changed.</summary>
   public int NumberOfExecutedRuns => _monitor.Locked(() => _numberOfExecutedRuns);

   /// <summary>Prepares a run with <paramref name="check"/>, once the check or execution under way has finished.</summary>
   public ImportRun Check(Func<ImportRun> check) => _oneAtATime.Do(check);

   /// <summary>
   /// Executes <paramref name="run"/> with <paramref name="execute"/>, unless it <see cref="ImportRun.IsOutOfDate"/>, once the check or execution under way<br/>
   /// has finished. Says whether it executed it.
   /// </summary>
   /// <remarks>An execution that fails is counted too: it may have imported some of its raw notes before it failed.</remarks>
   public bool ExecuteUnlessOutOfDate(ImportRun run, Action execute)
   {
      var executed = _oneAtATime.Do(() =>
      {
         if(run.IsOutOfDate) return false;

         try
         {
            execute();
         }
         finally
         {
            CountTheExecution();
         }

         return true;
      });

      if(executed) _runExecuted.OnNext(run);
      return executed;
   }

   void CountTheExecution() => _monitor.Locked(() => _numberOfExecutedRuns++);
}
