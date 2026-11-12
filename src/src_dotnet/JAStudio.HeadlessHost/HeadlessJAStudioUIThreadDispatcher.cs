using System;
using Avalonia.Threading;
using JAStudio.Core.TaskRunners;
using JAStudio.Mcp.UI;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio's <see cref="IUIThreadDispatcher"/>: while JAStudio loads, before its <see cref="HeadlessUI"/> runs, there is no UI thread,<br/>
/// and work runs on the calling thread; from <see cref="UseTheThreadOf"/> on, it runs on the headless UI's thread, which every window and view<br/>
/// model lives on, as JAStudio inside Anki has Avalonia's UI thread.
/// </summary>
/// <remarks>
/// Run on the calling thread, a view model's refresh posted from a background thread, such as a note tab showing a note the editor's reparse<br/>
/// changed, failed: "The calling thread cannot access this object because a different thread owns it" (2026-11-12).
/// </remarks>
/// <remarks>It shows no progress dialogs (<see cref="OnTaskRunnerInstantiated"/>), as before: no one watches the headless JAStudio's screen as it works.</remarks>
sealed class HeadlessJAStudioUIThreadDispatcher : IUIThreadDispatcher
{
   volatile Dispatcher? _theHeadlessUIsThread;

   /// <summary>From now on, runs the UI's work on <paramref name="headlessUI"/>'s thread.</summary>
   public void UseTheThreadOf(HeadlessUI headlessUI) => _theHeadlessUIsThread = headlessUI.UIThread;

   public void PostToUIThread(Action action)
   {
      if(_theHeadlessUIsThread is {} uiThread) uiThread.Post(action);
      else action();
   }

   public void InvokeSynchronouslyOnUIThread(Action action)
   {
      if(_theHeadlessUIsThread is {} uiThread) uiThread.Invoke(action);
      else action();
   }

   public bool CurrentThreadIsUIThread() => _theHeadlessUIsThread?.CheckAccess() ?? true;

   public void PumpEventsToKeepUIResponsive()
   {
      if(_theHeadlessUIsThread is {} uiThread && uiThread.CheckAccess()) uiThread.RunJobs();
   }

   public void OnTaskRunnerInstantiated(TaskProgressDialogViewModel viewModel) {}
}
