using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TaskRunners;
using Python.Runtime;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// Makes a change to many notes in the background, with a progress dialog: tagging the selected search results, suspending their cards, and renaming<br/>
/// or deleting a branch of the tag tree. A change to the notes' own tags, or to their cards, is made a note at a time, and afterwards Anki's own windows<br/>
/// are refreshed to show it. A change to their Anki tags, below <c>anki::</c>, is made in Anki, as one Anki operation (<see cref="AnkiTagChanges"/>).
/// </summary>
class BulkNoteChanges(TemporaryServiceCollection services)
{
   readonly TaskRunner _taskRunner = services.TaskRunner;
   readonly AnkiTagChanges _ankiTagChanges = services.Resolve<AnkiTagChanges>();

   /// <summary>Does <paramref name="changeANote"/> to each of <paramref name="notes"/> in the background, showing its progress as <paramref name="whatIsDone"/>.</summary>
   public Task ChangeEachOf(List<JPNote> notes, string whatIsDone, Action<JPNote> changeANote) =>
      TaskCE.Run(() =>
      {
         using(var scope = _taskRunner.Current(whatIsDone))
            scope.RunBatch(notes, changeANote, whatIsDone);
         ShowTheChangeInAnki();
      });

   /// <summary>Adds <paramref name="tag"/> to each of <paramref name="notes"/> that lacks it: to the note itself, or, for an Anki tag, to its study-note in Anki.</summary>
   public Task AddTheTagTo(IReadOnlyList<JPNote> notes, Tag tag) =>
      tag.IsAnAnkiTag
         ? InAnki($"Adding the tag {tag.Name}", () => _ankiTagChanges.AddTo(notes, tag))
         : ChangeEachOf([..notes.Where(it => !it.Tags.Contains(tag))], $"Adding the tag {tag.Name}", it => it.Tags.Set(tag));

   /// <summary>Removes <paramref name="tag"/> from each of <paramref name="notes"/> that has it: from the note itself, or, for an Anki tag, from its study-note in Anki.</summary>
   public Task RemoveTheTagFrom(IReadOnlyList<JPNote> notes, Tag tag) =>
      tag.IsAnAnkiTag
         ? InAnki($"Removing the tag {tag.Name}", () => _ankiTagChanges.RemoveFrom(notes, tag))
         : ChangeEachOf([..notes.Where(it => it.Tags.Contains(tag))], $"Removing the tag {tag.Name}", it => it.Tags.Unset(tag));

   /// <summary>
   /// Makes <paramref name="rename"/>: on each of <paramref name="notesWithATagInTheBranch"/>, or, for a branch below <c>anki::</c>, on every Anki note with a<br/>
   /// tag in it, in Anki.
   /// </summary>
   public Task RenameTheBranch(TagBranchRename rename, List<JPNote> notesWithATagInTheBranch) =>
      rename.Branch.IsInTheAnkiBranch
         ? InAnki($"Renaming {rename.Branch.Path} to {rename.Renamed.Path}", () => _ankiTagChanges.Rename(rename))
         : ChangeEachOf(notesWithATagInTheBranch, $"Renaming {rename.Branch.Path} to {rename.Renamed.Path}", rename.ApplyTo);

   /// <summary>
   /// Deletes <paramref name="branch"/>: from each of <paramref name="notesWithATagInTheBranch"/>, or, for a branch below <c>anki::</c>, from every Anki note with a<br/>
   /// tag in it, in Anki.
   /// </summary>
   public Task DeleteTheBranch(TagBranch branch, List<JPNote> notesWithATagInTheBranch) =>
      branch.IsInTheAnkiBranch
         ? InAnki($"Deleting {branch.Path}", () => _ankiTagChanges.Delete(branch))
         : ChangeEachOf(notesWithATagInTheBranch, $"Deleting {branch.Path}", it => it.Tags.RemoveTheBranch(branch));

   /// <summary>Makes <paramref name="change"/>, a change in Anki, in the background, showing its progress as <paramref name="whatIsDone"/>. Anki's windows show it as they show any Anki operation.</summary>
   Task InAnki(string whatIsDone, Action change) =>
      TaskCE.Run(() =>
      {
         using var scope = _taskRunner.Current(whatIsDone);
         scope.RunIndeterminate(whatIsDone, change);
      });

   /// <summary>Refreshes Anki's own windows, such as its browser, as the note menus do after they change notes. There's no Anki when JAStudio runs without it.</summary>
   static void ShowTheChangeInAnki()
   {
      if(PythonEngine.IsInitialized)
         AnkiFacade.UIUtils.Refresh();
   }
}
