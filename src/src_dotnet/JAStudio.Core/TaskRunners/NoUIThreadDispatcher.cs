using System;
using System.Collections.Concurrent;
using Compze.Threading;

namespace JAStudio.Core.TaskRunners;

/// <summary>
/// The <see cref="IUIThreadDispatcher"/> where there is no UI thread, such as the specifications': the thread that waits for the application to<br/>
/// catch up is the UI thread. A call posted to it waits until then, as a call posted to a UI thread waits until the UI thread gets to it, and runs<br/>
/// on that thread (<see cref="RunWhatWasPosted"/>). The headless JAStudio, which runs Avalonia's UI, has a dispatcher of its own<br/>
/// (<c>JAStudio.HeadlessHost.HeadlessJAStudioUIThreadDispatcher</c>).
/// </summary>
/// <remarks>
/// The view models' state is changed only on the UI thread. Each observer of tevents posts its calls here from a thread of its own<br/>
/// (<c>JAStudio.UI.ViewModels.TeventsObservedOnTheUIThread</c>): run there at once, they changed a view model while the specification's own thread<br/>
/// changed it too, and an editor's list that both refreshed at once showed a kanji's radical twice. A call invoked synchronously runs at once, since<br/>
/// its caller waits for it, and all of them run one at a time.
/// </remarks>
public class NoUIThreadDispatcher : IUIThreadDispatcher
{
   readonly IMonitor _oneAtATime = IMonitor.New();
   readonly ConcurrentQueue<Action> _posted = new();

   public void PostToUIThread(Action action) => _posted.Enqueue(action);
   public void InvokeSynchronouslyOnUIThread(Action action) => _oneAtATime.Locked(action);
   public bool CurrentThreadIsUIThread() => true;
   public void PumpEventsToKeepUIResponsive() {}
   public void OnTaskRunnerInstantiated(TaskProgressDialogViewModel viewModel) {}

   /// <summary>
   /// Runs the calls posted so far, and those they post meanwhile, in the order they were posted, on the calling thread: what a UI thread does when<br/>
   /// it gets to them.
   /// </summary>
   /// <returns>Whether it ran any.</returns>
   public bool RunWhatWasPosted()
   {
      var ranAny = false;
      while(_posted.TryDequeue(out var call))
      {
         _oneAtATime.Locked(call);
         ranAny = true;
      }

      return ranAny;
   }
}
