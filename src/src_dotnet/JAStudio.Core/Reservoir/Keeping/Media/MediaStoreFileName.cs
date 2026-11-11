using System;
using System.Security.Cryptography;

namespace JAStudio.Core.Reservoir.Keeping.Media;

/// <summary>
/// The name the <see cref="MediaStore"/> keeps a content under, such as <c>3fa9….mp3</c>: the SHA-256 of the content, then the extension of the<br/>
/// file it came from. Two files with the same content and extension have the same name, so the store keeps the content once.
/// </summary>
/// <param name="Hash">The SHA-256 of the content, as lowercase hex.</param>
/// <param name="Extension">The extension of the file the content came from, lowercase, such as <c>.mp3</c>.</param>
public sealed record MediaStoreFileName(string Hash, string Extension)
{
   /// <summary>The name <paramref name="content"/>, from a file with <paramref name="extension"/>, is kept under.</summary>
   public static MediaStoreFileName Of(byte[] content, string extension) => new(Convert.ToHexStringLower(SHA256.HashData(content)), extension);

   public override string ToString() => Hash + Extension;
}
