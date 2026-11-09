using System.IO;
using Avalonia.Media.Imaging;

namespace JAStudio.Mcp.UI;

/// <summary>An image of a window, as <see cref="HeadlessUI"/> rendered it: a PNG, and its size in pixels.</summary>
public record WindowImage(byte[] Png, int Width, int Height)
{
   public static WindowImage Of(Bitmap frame)
   {
      using var png = new MemoryStream();
      frame.Save(png, PngBitmapEncoderOptions.Default);
      return new WindowImage(png.ToArray(), frame.PixelSize.Width, frame.PixelSize.Height);
   }
}
