using System;
using System.IO;
using System.Security.Cryptography;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>The Anki package, the .apkg file, a deck was kept from: enough to find it again and to tell it from another upload of the same deck.</summary>
/// <param name="FileName">The package's file name, such as <c>Akame ga Kill (JA-EN 480x320).apkg</c>.</param>
/// <param name="Length">The package's length, in bytes.</param>
/// <param name="Sha256">The SHA-256 of the whole package, as lowercase hex.</param>
public sealed record KeptPackage(string FileName, long Length, string Sha256)
{
   /// <summary>The record of <paramref name="package"/>, an .apkg file, which is read whole to hash it.</summary>
   public static KeptPackage Of(FileInfo package)
   {
      using var stream = package.OpenRead();
      return new KeptPackage(package.Name, package.Length, Convert.ToHexStringLower(SHA256.HashData(stream)));
   }
}
