using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.Workspace.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>
/// The "Tags" part of a note's editors in the workspace's editor section (<see cref="NoteTagsEditor"/>): every tag the note has (<see cref="JPNote.AllTags"/>),<br/>
/// each with a button that removes it, and a button that adds a tag picked in a <see cref="TagPickerDialog"/>, as Ctrl+T does.<br/>
/// The tags JAStudio maintains (<see cref="Tags.IsMaintainedByJAStudio"/>) are shown dimmed, without the button: they're generated or have editors of their own.<br/>
/// So are the note's Anki tags (<see cref="Tags.Anki"/>), which JAStudio only reads.<br/>
/// Removing a source tag (<see cref="Tags.Source"/>) asks first, since it changes the note's source.
/// </summary>
/// <remarks>
/// It reads and writes through <c>currentNote</c>, the note as the collection holds it now. Every change is written to the note at once,<br/>
/// and the tab showing the note then refreshes its editors, this one included.
/// </remarks>
partial class NoteTagsEditorViewModel : ObservableObject
{
   readonly Func<JPNote> _currentNote;
   readonly OneNoteTagging _tagging;
   readonly IFatalErrorHandler _fatalErrorHandler;

   /// <param name="services">The application's services.</param>
   /// <param name="currentNote">Finds the note being edited, as the collection holds it now.</param>
   public NoteTagsEditorViewModel(TemporaryServiceCollection services, Func<JPNote> currentNote)
   {
      _currentNote = currentNote;
      _tagging = new OneNoteTagging(services);
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      Refresh();
   }

   /// <summary>The note's tags, its Anki tags among them, in alphabetical order.</summary>
   [ObservableProperty] IReadOnlyList<TagOnTheNoteViewModel> _tags = [];

   /// <summary>Picks a tag through <paramref name="pickATagInADialog"/>, any tag in use or a new one, and adds it to the note.</summary>
   public void AddATag(Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog) => _tagging.AddATagTo(_currentNote, pickATagInADialog);

   /// <summary>Shows the tags as the note has them now, after a change made here or elsewhere.</summary>
   public void Refresh() => Tags = [.._currentNote().AllTags.OrderBy(it => it.Name, StringComparer.OrdinalIgnoreCase).Select(it => new TagOnTheNoteViewModel(it))];

   /// <summary>
   /// The × of <paramref name="tag"/>: removes it from the note, and for a source tag, which changes the note's source, only once<br/>
   /// <paramref name="confirmInADialog"/> confirms it.
   /// </summary>
   /// <param name="tag">One of <see cref="Tags"/>, one a person can remove (<see cref="TagOnTheNoteViewModel.CanBeRemovedByHand"/>).</param>
   /// <param name="confirmInADialog">Asks the question it's given in a dialog, and returns whether the answer was yes.</param>
   public void Remove(TagOnTheNoteViewModel tag, Func<string, Task<bool>> confirmInADialog) =>
      _fatalErrorHandler.StartAndHandleExceptionsFrom(async () =>
      {
         if(tag.IsASourceTag && !await confirmInADialog($"{tag.Name} is a source tag: removing it changes the note's source. Remove it?")) return;

         _currentNote().Tags.Unset(tag.Tag);
      });
}
