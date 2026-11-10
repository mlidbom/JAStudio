using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// Turns the path of an image file into the image, for an <c>Image</c> control to show: <see cref="Thumbnail"/> decodes it small, at a thumbnail's height,<br/>
/// and <see cref="Preview"/> as it is, for showing it larger. No path gives no image.
/// </summary>
/// <remarks>Decoding the thumbnail at its height, rather than scaling the whole image down when drawing it, keeps a screenshot's thumbnail from holding the whole screenshot.</remarks>
sealed class ImageFileConverter : IValueConverter
{
   /// <summary>The height thumbnails are decoded at, in pixels.</summary>
   public const int ThumbnailHeight = 48;

   readonly int? _heightDecodedAt;

   ImageFileConverter(int? heightDecodedAt) => _heightDecodedAt = heightDecodedAt;

   /// <summary>The image at <see cref="ThumbnailHeight"/>.</summary>
   public static ImageFileConverter Thumbnail { get; } = new(ThumbnailHeight);

   /// <summary>The image as it is, at its own size.</summary>
   public static ImageFileConverter Preview { get; } = new(null);

   public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
   {
      if(value is not string path) return null;

      using var file = File.OpenRead(path);
      return _heightDecodedAt is {} height ? Bitmap.DecodeToHeight(file, height) : new Bitmap(file);
   }

   public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException("An image isn't turned back into its file's path.");
}
