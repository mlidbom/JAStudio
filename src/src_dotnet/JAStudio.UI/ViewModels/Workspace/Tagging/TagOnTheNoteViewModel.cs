using System;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>One of a note's tags, as its <see cref="NoteTagsEditorViewModel"/> shows it: the tag's name, with a button that removes it from the note.</summary>
sealed class TagOnTheNoteViewModel
{
   /// <param name="tag">The tag the note has.</param>
   /// <param name="removeFromTheNote">Removes the tag from the note.</param>
   public TagOnTheNoteViewModel(Tag tag, Action<Tag> removeFromTheNote)
   {
      Name = tag.Name;
      RemoveCommand = new RelayCommand(() => removeFromTheNote(tag));
   }

   public string Name { get; }

   /// <summary>Removes the tag from the note.</summary>
   public IRelayCommand RemoveCommand { get; }
}
