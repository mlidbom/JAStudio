using Avalonia.Controls;
using Avalonia.Threading;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.ViewModels.Workspace.Editors;

namespace JAStudio.UI.Views.Workspace.Editors;

/// <summary>What every editor window does to stay true to what it edits (<see cref="IEditorWindowViewModel"/>), which may change elsewhere while it's open.</summary>
static class EditorWindowExtensions
{
   extension(Window window)
   {
      /// <summary>
      /// Shows what <paramref name="viewModel"/>, the window's, edits as it is now whenever the window is activated, since something else may have changed<br/>
      /// it meanwhile, and closes the window then if it's gone.
      /// </summary>
      /// <remarks>
      /// The window also closes when the collection is loaded anew, as after a sync: what it edits is then loaded anew too, such as the shared-conditions,<br/>
      /// and the window would hold what was loaded before.
      /// </remarks>
      internal void KeepShowingWhatItEditsAsItIsNow(IEditorWindowViewModel viewModel, TemporaryServiceCollection services)
      {
         window.Activated += (_, _) => ShowItAsItIsNowOrCloseIfItIsGone();

         var uiThread = services.Resolve<IUIThreadDispatcher>();
         var closingOnTheNextReload = services.CoreApp.Collection.ReloadedFromBackend.Subscribe(_ => uiThread.PostToUIThread(window.Close));
         window.Closed += (_, _) => closingOnTheNextReload.Dispose();
         return;

         void ShowItAsItIsNowOrCloseIfItIsGone()
         {
            if(viewModel.IsGone)
               Dispatcher.UIThread.Post(window.Close);
            else
               viewModel.ShowItAsItIsNow();
         }
      }
   }
}
