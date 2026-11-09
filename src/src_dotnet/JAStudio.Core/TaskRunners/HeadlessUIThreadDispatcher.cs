using System;

namespace JAStudio.Core.TaskRunners;

/// <summary>Used in tests and headless environments where there is no UI thread, such as the headless JAStudio: runs everything on the calling thread.</summary>
public class HeadlessUIThreadDispatcher : IUIThreadDispatcher
{
   public void PostToUIThread(Action action) => action();
   public void InvokeSynchronouslyOnUIThread(Action action) => action();
   public bool CurrentThreadIsUIThread() => true;
   public void PumpEventsToKeepUIResponsive() {}
   public void OnTaskRunnerInstantiated(TaskProgressDialogViewModel viewModel) {}
}
