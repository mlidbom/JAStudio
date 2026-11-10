using JAStudio.Core.Note.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>
/// One of a note's tags, as its <see cref="NoteTagsEditorViewModel"/> shows it: the tag's name, with a button that removes it from the note,<br/>
/// or, for a tag JAStudio maintains (<see cref="Tags.IsMaintainedByJAStudio"/>), dimmed and without the button, since it isn't removed by hand.
/// </summary>
sealed class TagOnTheNoteViewModel
{
   /// <param name="tag">The tag the note has.</param>
   public TagOnTheNoteViewModel(Tag tag)
   {
      Tag = tag;
      IsMaintainedByJAStudio = Tags.IsMaintainedByJAStudio(tag.Name);
      IsASourceTag = new TagBranch(tag.Name).IsInTheSourceBranch;
   }

   public Tag Tag { get; }

   public string Name => Tag.Name;

   /// <summary>Whether JAStudio maintains the tag (<see cref="Tags.IsMaintainedByJAStudio"/>): it's shown dimmed, with no button that removes it.</summary>
   public bool IsMaintainedByJAStudio { get; }

   /// <summary>Whether it can be removed by hand: it isn't <see cref="IsMaintainedByJAStudio"/>.</summary>
   public bool CanBeRemovedByHand => !IsMaintainedByJAStudio;

   /// <summary>Whether it's a source tag (<see cref="Tags.Source"/>), whose removal changes the note's source, so it's confirmed first.</summary>
   public bool IsASourceTag { get; }

   /// <summary>What hovering the tag shows: why it can't be removed, for one JAStudio maintains.</summary>
   public string? ToolTip => IsMaintainedByJAStudio ? "JAStudio maintains this tag: it generates it, or has an editor of its own for it." : null;
}
