using System.IO;
using Avalonia.Media.Imaging;
using JAStudio.Core.UI;

namespace JAStudio.Mcp.UI;

/// <summary>Avalonia's bitmaps, such as a window's rendered frame, as the <see cref="PngImage"/> the tools give Claude.</summary>
static class BitmapPngImages
{
   public static PngImage ToPngImage(this Bitmap bitmap)
   {
      using var png = new MemoryStream();
      bitmap.Save(png, PngBitmapEncoderOptions.Default);
      return PngImage.Of(png.ToArray());
   }
}
