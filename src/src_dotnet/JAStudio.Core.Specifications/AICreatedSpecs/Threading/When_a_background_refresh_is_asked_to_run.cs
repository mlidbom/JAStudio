using System;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.SysUtils.Threading;
using JAStudio.Core.TaskRunners;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Threading;

/// <summary>
/// A <see cref="BackgroundRefresh"/> runs its refresh in the background whenever asked, one run at a time, and once more after a run for the asks made<br/>
/// during it, however many. Here the refresh counts its runs, and its first run, under way once the refresh is asked to run, waits until the<br/>
/// specification lets it finish.
/// </summary>
public class When_a_background_refresh_is_asked_to_run : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static readonly TimeSpan HowLongARunMayTake = TimeSpan.FromSeconds(10);
   static readonly TimeSpan HowLongToWatchForAnotherRun = TimeSpan.FromMilliseconds(300);

   readonly ManualResetEventSlim _theFirstRunIsUnderWay = new();
   readonly ManualResetEventSlim _theFirstRunMayFinish = new();
   readonly SemaphoreSlim _finishedRuns = new(0);
   int _runs;

   protected readonly BackgroundRefresh Refresh;

   public When_a_background_refresh_is_asked_to_run()
   {
      Refresh = new BackgroundRefresh(GetService<BackgroundTaskManager>(), CountedRun);
      Refresh.Run();
      _theFirstRunIsUnderWay.Wait(HowLongARunMayTake);
   }

   protected int Runs => Volatile.Read(ref _runs);

   protected void LetTheFirstRunFinish() => _theFirstRunMayFinish.Set();

   /// <summary>Waits until one more run has finished, and says whether one did within <paramref name="timeout"/>.</summary>
   protected bool ARunFinishesWithin(TimeSpan timeout) => _finishedRuns.Wait(timeout);

   void CountedRun()
   {
      if(Interlocked.Increment(ref _runs) == 1)
      {
         _theFirstRunIsUnderWay.Set();
         _theFirstRunMayFinish.Wait(HowLongARunMayTake);
      }

      _finishedRuns.Release();
   }

   public class asked_three_times_more_during_its_first_run : When_a_background_refresh_is_asked_to_run
   {
      readonly bool _aThirdRunFinished;

      public asked_three_times_more_during_its_first_run()
      {
         Refresh.Run();
         Refresh.Run();
         Refresh.Run();
         LetTheFirstRunFinish();
         ARunFinishesWithin(HowLongARunMayTake);
         ARunFinishesWithin(HowLongARunMayTake);
         _aThirdRunFinished = ARunFinishesWithin(HowLongToWatchForAnotherRun);
      }

      [XF] public void it_runs_once_more_after_the_first_run() => Runs.Must().Be(2);
      [XF] public void no_third_run_follows() => _aThirdRunFinished.Must().BeFalse();
   }

   public class asked_again_after_its_first_run_finished : When_a_background_refresh_is_asked_to_run
   {
      public asked_again_after_its_first_run_finished()
      {
         LetTheFirstRunFinish();
         ARunFinishesWithin(HowLongARunMayTake);
         Refresh.Run();
         ARunFinishesWithin(HowLongARunMayTake);
      }

      [XF] public void it_runs_again() => Runs.Must().Be(2);
   }
}
