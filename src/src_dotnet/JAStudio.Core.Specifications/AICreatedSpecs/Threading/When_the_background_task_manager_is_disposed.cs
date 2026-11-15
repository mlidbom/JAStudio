using System;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.TaskRunners;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Threading;

/// <summary>
/// Disposing the <see cref="BackgroundTaskManager"/>, as JAStudio does when it shuts down, waits until every background task has finished.
/// </summary>
public class When_the_background_task_manager_is_disposed : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   /// <summary>Long enough that the disposal is waiting for the task by the time the task starts another one.</summary>
   static readonly TimeSpan HowLongTheFirstTaskRunsBeforeItStartsAnother = TimeSpan.FromMilliseconds(300);

   /// <summary>Until 2026-11-15 the disposal held the lock that starting a task takes, so the two waited for each other until the lock timed out.</summary>
   public class while_a_task_that_then_starts_another_task_is_running : When_the_background_task_manager_is_disposed
   {
      bool _theFirstTaskFinished;
      bool _theTaskItStartedFinished;

      public while_a_task_that_then_starts_another_task_is_running()
      {
         var backgroundTasks = GetService<BackgroundTaskManager>();
         backgroundTasks.Run(() =>
         {
            Thread.Sleep(HowLongTheFirstTaskRunsBeforeItStartsAnother);
            backgroundTasks.Run(() => Volatile.Write(ref _theTaskItStartedFinished, true));
            Volatile.Write(ref _theFirstTaskFinished, true);
         });

         backgroundTasks.Dispose();
      }

      [XF] public void it_has_waited_for_the_first_task() => Volatile.Read(ref _theFirstTaskFinished).Must().BeTrue();
      [XF] public void it_has_waited_for_the_task_that_one_started() => Volatile.Read(ref _theTaskItStartedFinished).Must().BeTrue();
   }
}
