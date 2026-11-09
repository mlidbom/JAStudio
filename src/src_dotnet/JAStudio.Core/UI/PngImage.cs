using System;
using System.Buffers.Binary;

namespace JAStudio.Core.UI;

/// <summary>An image in the PNG format, with its size in pixels: such as a window of JAStudio's, or a card page, rendered for Claude to look at.</summary>
/// <param name="Png">The image, as the bytes of a PNG file.</param>
public record PngImage(byte[] Png, int Width, int Height)
{
   /// <summary>The bytes every PNG file starts with.</summary>
   static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

   /// <summary>
   /// The image in the PNG file <paramref name="png"/>, with the size its header says: the header chunk follows the signature, the chunk's length<br/>
   /// and its type, and starts with the width and the height, each four bytes, most significant first.
   /// </summary>
   public static PngImage Of(byte[] png) =>
      png.AsSpan(0, Signature.Length).SequenceEqual(Signature)
         ? new PngImage(png, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)), BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)))
         : throw new ArgumentException("The bytes aren't a PNG file: they don't start with its signature.", nameof(png));
}
