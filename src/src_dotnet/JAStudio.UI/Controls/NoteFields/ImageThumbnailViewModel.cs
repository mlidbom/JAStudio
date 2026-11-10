using System;
using System.IO;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Storage.Media;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One image of a note's image list, in an <see cref="ImageListEditorViewModel"/>: a small thumbnail with a button removing it from the note, and on hover a<br/>
/// larger preview with the file's name and alt text.
/// </summary>
/// <remarks>
/// An image that can't be shown, as the media layer hasn't its file or it's an SVG drawing rather than a picture, shows as a box with its file's name.
/// </remarks>
partial class ImageThumbnailViewModel
{
   readonly ImageReference _image;
   readonly ImageAttachment? _file;
   readonly Action<ImageReference> _remove;

   /// <param name="image">The note's reference to the file.</param>
   /// <param name="file">The file in the media layer, or null when the media layer hasn't it.</param>
   /// <param name="remove">Removes <paramref name="image"/> from the note's list.</param>
   public ImageThumbnailViewModel(ImageReference image, ImageAttachment? file, Action<ImageReference> remove)
   {
      _image = image;
      _file = file;
      _remove = remove;
   }

   /// <summary>The file's name, such as <c>kv_right_img.jpg</c>.</summary>
   public string FileName => _image.FileName;

   /// <summary>The image's alt text, or an empty text when it has none.</summary>
   public string AltText => _image.AltText ?? "";

   public bool HasAltText => _image.AltText is not null;

   /// <summary>The path of the image's file, to show it from (<see cref="ImageFileConverter"/>), or null when it <see cref="CanBeShown"/> not.</summary>
   public string? PathToShow => CanBeShown ? _file?.FilePath : null;

   /// <summary>Whether the image can be shown: the media layer has its file, and it's a picture, not an SVG drawing, which JAStudio doesn't draw.</summary>
   public bool CanBeShown => _file is not null && !Path.GetExtension(FileName).Equals(".svg", StringComparison.OrdinalIgnoreCase);

   /// <summary>What the preview says when the image can't be shown.</summary>
   public string WhyItCannotBeShown => _file is null ? "The file isn't in JAStudio's media" : "JAStudio doesn't show SVG drawings";

   [RelayCommand] void Remove() => _remove(_image);
}
