using System.Collections.ObjectModel;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Storage.Media;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One of a note's lists of images (<see cref="MediaReferenceList{TReference}"/> of <see cref="ImageReference"/>), such as a sentence's screenshots, shown in<br/>
/// an <see cref="ImageListEditor"/>: a small thumbnail for each, which shows a larger preview with the file's name and alt text on hover.
/// </summary>
class ImageListEditorViewModel : MediaListEditorViewModel
{
   readonly EditedNotePart<MediaReferenceList<ImageReference>> _list;
   readonly MediaFileIndex _media;

   /// <param name="title">What the list holds, shown above it, such as "Screenshots".</param>
   /// <param name="list">The list, on the note the editor edits.</param>
   /// <param name="media">Finds each file in the media layer, to show it.</param>
   public ImageListEditorViewModel(string title, EditedNotePart<MediaReferenceList<ImageReference>> list, MediaFileIndex media) : base(title)
   {
      _list = list;
      _media = media;
      Refresh();
   }

   /// <summary>The list's images, in the order they're preferred in.</summary>
   public ObservableCollection<ImageThumbnailViewModel> Items { get; } = [];

   public override void Refresh()
   {
      Items.Clear();
      foreach(var image in _list.Current.All)
         Items.Add(new ImageThumbnailViewModel(image, _media.Resolve(image), Remove));
   }

   void Remove(ImageReference image)
   {
      _list.Edit(it => it.Remove(image));
      Refresh();
   }
}
