using System;

namespace JAStudio.Core.TaskRunners;

/// <summary>Used in tests and headless environments where there is no UI thread, such as the headless JAStudio: runs everything on the calling thread.</summary>
public class HeadlessUIThreadDispatcher : IUIThreadDispatcher
{
   public void PostToUIThread(Action action)
   {
      //bug: The headless JAStudio runs Avalonia's UI, for render_the_workspace and the other screen tools, yet registers this dispatcher
      //   (HeadlessEnvironmentDependenciesRegistrar). So a view model's refresh posted from a background thread, such as NoteTabViewModel
      //   showing a note the editor's reparse changed, runs on that thread, and Avalonia throws "The calling thread cannot access this object
      //   because a different thread owns it" (the main checkout's headless log, 2026-11-12). The headless JAStudio should post to
      //   Avalonia's dispatcher, as JAStudio in Anki does.
      action();
   }
   public void InvokeSynchronouslyOnUIThread(Action action) => action();
   public bool CurrentThreadIsUIThread() => true;
   public void PumpEventsToKeepUIResponsive() {}
   public void OnTaskRunnerInstantiated(TaskProgressDialogViewModel viewModel) {}
}
