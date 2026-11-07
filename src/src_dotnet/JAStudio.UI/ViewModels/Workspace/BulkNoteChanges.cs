using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.TaskRunners;
using Python.Runtime;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// Makes a change to many notes in the background, a note at a time, with a progress dialog: tagging the selected search results,<br/>
/// suspending their cards, and renaming or deleting a branch of the tag tree. Afterwards Anki's own windows are refreshed to show the change.
/// </summary>
class BulkNoteChanges(TemporaryServiceCollection services)
{
   readonly TaskRunner _taskRunner = services.TaskRunner;

   /// <summary>Does <paramref name="changeANote"/> to each of <paramref name="notes"/> in the background, showing its progress as <paramref name="whatIsDone"/>.</summary>
   public Task ChangeEachOf(List<JPNote> notes, string whatIsDone, Action<JPNote> changeANote) =>
      TaskCE.Run(() =>
      {
         using(var scope = _taskRunner.Current(whatIsDone))
            scope.RunBatch(notes, changeANote, whatIsDone);
         ShowTheChangeInAnki();
      });

   /// <summary>Refreshes Anki's own windows, such as its browser, as the note menus do after they change notes. There's no Anki when JAStudio runs without it.</summary>
   static void ShowTheChangeInAnki()
   {
      if(PythonEngine.IsInitialized)
         AnkiFacade.UIUtils.Refresh();
   }
}
