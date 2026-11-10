using CommunityToolkit.Mvvm.ComponentModel;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One of a note's lists of media files (<see cref="Core.Note.Media.MediaReferenceList{TReference}"/>), shown in the editor's section for where the files<br/>
/// came from: the note's source, or the corpus. Each file can be removed, a correction, which saves the note at once. Adding a file by hand is a feature<br/>
/// of its own, for later (<c>corpus-data-model.md</c>, Media, "The shape").
/// </summary>
abstract class MediaListEditorViewModel(string title) : ObservableObject
{
   /// <summary>What the list holds, shown above it, such as "Audio" or "Screenshots".</summary>
   public string Title { get; } = title;

   /// <summary>Shows the list as the note has it now, after a change made elsewhere.</summary>
   public abstract void Refresh();
}
