using System;

namespace JAStudio.Core.TaskRunners;

/// <summary>
/// The <see cref="IUIThreadDispatcher"/> where there is no UI thread, such as the specifications': runs everything on the calling thread. The headless<br/>
/// JAStudio, which runs Avalonia's UI, has a dispatcher of its own (<c>JAStudio.HeadlessHost.HeadlessJAStudioUIThreadDispatcher</c>).
/// </summary>
public class NoUIThreadDispatcher : IUIThreadDispatcher
{
   public void PostToUIThread(Action action) => action();
   public void InvokeSynchronouslyOnUIThread(Action action) => action();
   public bool CurrentThreadIsUIThread() => true;
   public void PumpEventsToKeepUIResponsive() {}
   public void OnTaskRunnerInstantiated(TaskProgressDialogViewModel viewModel) {}
}
