using Avalonia.Threading;
using Compze.Internals.Logging;
using JAStudio.Core;
using JAStudio.UI.Utils;
using JAStudio.UI.Views;

namespace JAStudio.UI;

// ReSharper disable MemberCanBeInternal used from python
// ReSharper disable UnusedMember.Global used from python
public class AnkiDialogs
{
   readonly CoreApp _coreApp;
   TemporaryServiceCollection Services => _coreApp.Services;

   public AnkiDialogs(CoreApp coreApp) => _coreApp = coreApp;

   internal void ShowReadingsMappingsDialog()
   {
      this.Log().Info("ShowReadingsMappingsDialog() called");
      Dispatcher.UIThread.Invoke(() => new ReadingsMappingsDialog(Services).ShowNearCursor());
   }

   public void ToggleNoteSearchDialog()
   {
      this.Log().Info("ToggleNoteSearchDialog() called");
      Dispatcher.UIThread.Invoke(() => NoteSearchDialog.ToggleVisibility(Services));
   }

   public void ToggleEnglishWordSearchDialog()
   {
      this.Log().Info("ToggleEnglishWordSearchDialog() called");
      Dispatcher.UIThread.Invoke(EnglishWordSearchDialog.ToggleVisibility);
   }

   public void ToggleMainWindow() => Dispatcher.UIThread.Invoke(MainWindow.ToggleVisibility);
}
