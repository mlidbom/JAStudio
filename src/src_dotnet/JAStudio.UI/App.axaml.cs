using System;
using System.Threading;
using Avalonia;
using Avalonia.Markup.Xaml;
using JAStudio.Core;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI;

public class UIApp : Application
{
   static readonly ManualResetEventSlim Initialized = new();

   public override void Initialize() => AvaloniaXamlLoader.Load(this);

   public override void OnFrameworkInitializationCompleted()
   {
      base.OnFrameworkInitializationCompleted();
      Initialized.Set();
   }

   public static void WaitForInitialization(TimeSpan timeout)
   {
      if(!Initialized.Wait(timeout))
      {
         throw new TimeoutException(
            $"Avalonia did not initialize within {timeout.TotalSeconds}s.");
      }
   }

   /// <summary>Creates JAStudio's workspace window, whose tabs show their pages in native web views (<see cref="NoteTabView"/>), as inside Anki.</summary>
   public static void InitializeWorkspaceWindow(TemporaryServiceCollection services) =>
      WorkspaceWindow.CreateAndRegister(services, tab => new NoteTabView(tab));

   /// <summary>
   /// Creates JAStudio's workspace window for a JAStudio with no screen, the headless JAStudio: its tabs show the cards' pages as images<br/>
   /// (<see cref="NoteTabImageView"/>), which <paramref name="webPageImages"/> renders. Its workspace plays no audio, since an agent drives it<br/>
   /// (<see cref="WorkspaceAudioPlayer.PlayNothingFromNowOn"/>).
   /// </summary>
   public static void InitializeWorkspaceWindowShowingPagesAsImages(TemporaryServiceCollection services, IWebPageImages webPageImages)
   {
      WorkspaceAudioPlayer.PlayNothingFromNowOn();
      WorkspaceWindow.CreateAndRegister(services, tab => new NoteTabImageView(tab, webPageImages));
   }
}
