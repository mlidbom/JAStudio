using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace JAStudio.Core.Reservoir.Sources.Subtitles;

/// <summary>
/// Subtitles kept as a folder per show, as the kitsunekko backup keeps them, in a zip or on disk: each show a work for the reservoir, its<br/>
/// sentences those of the subtitle files in its folder, at any depth. Subtitle files in zips in a show's folder are read too; other archives,<br/>
/// such as .rar and .7z, aren't.
/// </summary>
public static class SubtitleCollection
{
   /// <summary>The shows in <paramref name="zip"/>, a zip of a folder per show, or of one folder holding them, such as <c>kitsunekko_backup/</c>.</summary>
   public static IEnumerable<SourceWork> WorksInZip(string collectionName, FileInfo zip)
   {
      using var archive = ZipFile.OpenRead(zip.FullName);
      var files = archive.Entries.Where(it => it.Name.Length > 0).ToList();
      var rootFolderOfAll = files.Select(it => it.FullName.Split('/')[0]).Distinct().Count() == 1 && files.All(it => it.FullName.Count(c => c == '/') >= 2);
      foreach(var show in files.GroupBy(ShowOf).OrderBy(it => it.Key, StringComparer.Ordinal))
         yield return new SourceWork(new ReservoirSource(collectionName, show.Key, ReservoirSourceKind.Subtitles, $"{zip.FullName}/{show.Key}"),
                                     show.SelectMany(SubtitleFilesIn).SelectMany(it => it.Sentences()));

      string ShowOf(ZipArchiveEntry file) => file.FullName.Split('/') is var path && path.Length > 1 ? path[rootFolderOfAll ? 1 : 0] : "(no show's folder)";
   }

   /// <summary>The shows in <paramref name="folder"/>, a folder of a folder per show.</summary>
   public static IEnumerable<SourceWork> WorksInFolder(string collectionName, DirectoryInfo folder) =>
      folder.GetDirectories()
            .OrderBy(it => it.Name, StringComparer.Ordinal)
            .Select(show => new SourceWork(new ReservoirSource(collectionName, show.Name, ReservoirSourceKind.Subtitles, show.FullName),
                                           show.GetFiles("*", SearchOption.AllDirectories)
                                               .SelectMany(file => SubtitleFilesIn(file.Name, () => file.OpenRead()))
                                               .SelectMany(it => it.Sentences())));

   static IEnumerable<SubtitleFile> SubtitleFilesIn(ZipArchiveEntry entry) => SubtitleFilesIn(entry.Name, entry.Open);

   /// <summary>The subtitle file named <paramref name="fileName"/>, or those in it when it's a zip, or none.</summary>
   static IEnumerable<SubtitleFile> SubtitleFilesIn(string fileName, Func<Stream> open)
   {
      if(SubtitleFile.IsOne(fileName)) return [new SubtitleFile(fileName, BytesOf(open))];
      if(Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase)) return SubtitleFilesInZip(BytesOf(open));
      return [];
   }

   static IEnumerable<SubtitleFile> SubtitleFilesInZip(byte[] zip)
   {
      using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
      return [.. archive.Entries.Where(it => it.Name.Length > 0).SelectMany(SubtitleFilesIn)];
   }

   static byte[] BytesOf(Func<Stream> open)
   {
      using var stream = open();
      using var bytes = new MemoryStream();
      stream.CopyTo(bytes);
      return bytes.ToArray();
   }
}
