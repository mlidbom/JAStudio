using System;
using Compze.Threading;

namespace JAStudio.Core.TaskRunners;

/// <summary>
/// The <see cref="IUIThreadDispatcher"/> where there is no UI thread, such as the specifications': runs everything on the calling thread, one at a<br/>
/// time, as a UI thread would. The headless JAStudio, which runs Avalonia's UI, has a dispatcher of its own<br/>
/// (<c>JAStudio.HeadlessHost.HeadlessJAStudioUIThreadDispatcher</c>).
/// </summary>
/// <remarks>
/// One at a time, since the view models' state is changed only on the UI thread: each observer of tevents posts its calls here from a thread of its<br/>
/// own (<c>JAStudio.UI.ViewModels.TeventsObservedOnTheUIThread</c>), and two of them changing a view model at once could leave it showing the change<br/>
/// that came first.
/// </remarks>
public class NoUIThreadDispatcher : IUIThreadDispatcher
{
   readonly IMonitor _oneAtATime = IMonitor.New();

   public void PostToUIThread(Action action) => _oneAtATime.Locked(action);
   public void InvokeSynchronouslyOnUIThread(Action action) => _oneAtATime.Locked(action);
   public bool CurrentThreadIsUIThread() => true;
   public void PumpEventsToKeepUIResponsive() {}
   public void OnTaskRunnerInstantiated(TaskProgressDialogViewModel viewModel) {}
}
