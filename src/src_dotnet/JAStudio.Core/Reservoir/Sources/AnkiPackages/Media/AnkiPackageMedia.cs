using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JAStudio.Core.SysUtils.IO.Compression;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages.Media;

/// <summary>The media files an Anki package, an .apkg, holds besides its collection database, as its media map names them (<see cref="AnkiPackageMediaMap"/>).</summary>
public static class AnkiPackageMedia
{
   /// <summary>
   /// Every media file in <paramref name="package"/>, in the order of its media map, each read, and decompressed if need be, as it's enumerated, so<br/>
   /// that a package of gigabytes is never all in memory. A package with no media map holds none.
   /// </summary>
   public static IEnumerable<PackagedMediaFile> In(FileInfo package)
   {
      using var archive = ZipFile.OpenRead(package.FullName);
      var map = AnkiPackageMediaMap.In(archive);
      foreach(var entry in map.Entries) yield return new PackagedMediaFile(entry.FileName, map.FileContentFrom(TheZipEntryHolding(entry).ReadAllBytes()));

      ZipArchiveEntry TheZipEntryHolding(AnkiPackageMediaMapEntry entry) =>
         archive.GetEntry(entry.ZipEntryName) ?? throw new InvalidDataException($"{package.Name}'s media map names {entry.FileName} as its entry {entry.ZipEntryName}, which it doesn't hold.");
   }
}
