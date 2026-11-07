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
/// The "Tags" part of a note's editors in the workspace's editor section (<see cref="NoteTagsEditor"/>): the tags the note has,<br/>
/// each with a button that removes it, and a button that adds a tag picked in a <see cref="TagPickerDialog"/>, as Ctrl+T does.<br/>
/// The jastudio-defined-tags aren't shown (<see cref="NoteTags.NotDefinedByJAStudio"/>): they're generated or have editors of their own.
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

   /// <summary>The note's tags, other than the jastudio-defined-tags, in alphabetical order.</summary>
   [ObservableProperty] IReadOnlyList<TagOnTheNoteViewModel> _tags = [];

   /// <summary>Picks a tag through <paramref name="pickATagInADialog"/>, any tag in use or a new one, and adds it to the note.</summary>
   public void AddATag(Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog) => _tagging.AddATagTo(_currentNote, pickATagInADialog);

   /// <summary>Shows the tags as the note has them now, after a change made here or elsewhere.</summary>
   public void Refresh() => Tags = [.._currentNote().Tags.NotDefinedByJAStudio().Select(it => new TagOnTheNoteViewModel(it, Remove))];

   void Remove(Tag tag) => _fatalErrorHandler.HandleExceptionsFrom(() => _currentNote().Tags.Unset(tag));
}
