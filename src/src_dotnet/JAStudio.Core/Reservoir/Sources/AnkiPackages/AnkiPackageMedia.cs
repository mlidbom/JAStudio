using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ZstdSharp;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>The media files an Anki package, an .apkg, holds besides its collection database, as its media map names them (<see cref="AnkiPackageMediaMap"/>).</summary>
public static class AnkiPackageMedia
{
   const string MediaMapEntryName = "media";

   /// <summary>
   /// Every media file in <paramref name="package"/>, in the order of its media map, each read, and decompressed if need be, as it's enumerated, so<br/>
   /// that a package of gigabytes is never all in memory. A package with no media map holds none.
   /// </summary>
   public static IEnumerable<PackagedMediaFile> In(FileInfo package)
   {
      using var archive = ZipFile.OpenRead(package.FullName);
      if(archive.GetEntry(MediaMapEntryName) is not {} mapEntry) yield break;

      var map = AnkiPackageMediaMap.Of(BytesOf(mapEntry));
      foreach(var (zipEntryName, fileName) in map.Files)
      {
         var entry = archive.GetEntry(zipEntryName) ?? throw new InvalidDataException($"{package.Name}'s media map names {fileName} as its entry {zipEntryName}, which it doesn't hold.");
         yield return new PackagedMediaFile(fileName, map.FilesAreCompressed ? Decompressed(BytesOf(entry)) : BytesOf(entry));
      }
   }

   static byte[] BytesOf(ZipArchiveEntry entry)
   {
      using var stream = entry.Open();
      using var bytes = new MemoryStream();
      stream.CopyTo(bytes);
      return bytes.ToArray();
   }

   static byte[] Decompressed(byte[] compressed)
   {
      using var decompressor = new Decompressor();
      return decompressor.Unwrap(compressed).ToArray();
   }
}
