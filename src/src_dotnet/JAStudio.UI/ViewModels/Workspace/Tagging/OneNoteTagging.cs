using System;
using System.Threading.Tasks;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.ViewModels.Workspace.Navigation.SearchResults;
using JAStudio.UI.Views.Workspace.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>
/// Adds a tag to one note, or removes one of its tags, picked in a <see cref="TagPickerDialog"/>:<br/>
/// what Ctrl+T and Ctrl+Shift+T do on a note's tab, and what the note's Tags editor does.
/// </summary>
/// <remarks>
/// A tag of the note's own is changed at once, on the UI thread, as the other editors change the note. An Anki tag is changed on its study-note in<br/>
/// Anki, in the background, with no progress dialog, which would only flash (<see cref="AnkiTagChanges"/>); the note's tab shows it once it's read back.<br/>
/// Tagging the notes of a selection of search results goes through <see cref="ActionsOnSelectedResults"/> instead, which changes them in the background<br/>
/// with a progress dialog.
/// </remarks>
class OneNoteTagging
{
   readonly JPCollection _collection;
   readonly AnkiTagChanges _ankiTagChanges;
   readonly IFatalErrorHandler _fatalErrorHandler;

   public OneNoteTagging(TemporaryServiceCollection services)
   {
      _collection = services.CoreApp.Collection;
      _ankiTagChanges = services.Resolve<AnkiTagChanges>();
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
   }

   /// <summary>Picks a tag through <paramref name="pickATagInADialog"/>, any tag in use or a new one, and adds it to the note <paramref name="currentNote"/> finds.</summary>
   /// <param name="currentNote">Finds the note as the collection holds it now, since the collection may replace it while the dialog is open.</param>
   /// <param name="pickATagInADialog">Shows a tag picker dialog, and returns the tag path picked in it, or null when it was cancelled.</param>
   public void AddATagTo(Func<JPNote> currentNote, Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await pickATagInADialog(TagPickerDialogViewModel.ForAddingATagTo([currentNote()], _collection)) is {} tagPath)
            await AddTheTagTo(currentNote(), Tag.FromName(tagPath));
      });

   /// <summary>Picks one of the tags of the note <paramref name="currentNote"/> finds through <paramref name="pickATagInADialog"/>, and removes it.</summary>
   /// <param name="currentNote">Finds the note as the collection holds it now, since the collection may replace it while the dialog is open.</param>
   /// <param name="pickATagInADialog">Shows a tag picker dialog, and returns the tag path picked in it, or null when it was cancelled.</param>
   public void RemoveATagFrom(Func<JPNote> currentNote, Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(await pickATagInADialog(TagPickerDialogViewModel.ForRemovingATagFrom([currentNote()])) is {} tagPath)
            await RemoveTheTagFrom(currentNote(), Tag.FromName(tagPath));
      });

   /// <summary>Removes <paramref name="tag"/> from <paramref name="note"/>: one of its own at once, or an Anki tag from its study-note in Anki, in the background.</summary>
   public Task RemoveTheTagFrom(JPNote note, Tag tag)
   {
      if(tag.IsAnAnkiTag) return Task.Run(() => _ankiTagChanges.RemoveFrom([note], tag));

      note.Tags.Unset(tag);
      return Task.CompletedTask;
   }

   Task AddTheTagTo(JPNote note, Tag tag)
   {
      if(tag.IsAnAnkiTag) return Task.Run(() => _ankiTagChanges.AddTo([note], tag));

      note.Tags.Set(tag);
      return Task.CompletedTask;
   }
}
