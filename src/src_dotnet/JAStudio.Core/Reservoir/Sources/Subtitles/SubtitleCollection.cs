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
   const string NoShowsFolder = "(no show's folder)";

   /// <summary>The shows in <paramref name="zipOrFolder"/>: a zip (<see cref="WorksInZip"/>) or a folder (<see cref="WorksInFolder"/>).</summary>
   public static IEnumerable<SourceWork> WorksIn(string collectionName, FileSystemInfo zipOrFolder) =>
      zipOrFolder is FileInfo zip ? WorksInZip(collectionName, zip) : WorksInFolder(collectionName, (DirectoryInfo)zipOrFolder);

   /// <summary>The shows in <paramref name="zip"/>, a zip of a folder per show, or of one folder holding them, such as <c>kitsunekko_backup/</c>.</summary>
   public static IEnumerable<SourceWork> WorksInZip(string collectionName, FileInfo zip)
   {
      using var archive = ZipFile.OpenRead(zip.FullName);
      var files = archive.Entries.Where(IsAFile).ToList();
      var showsAreInOneRootFolder = AreAllInOneRootFolderOfFolders(files);
      foreach(var show in files.GroupBy(ShowOf).OrderBy(it => it.Key, StringComparer.Ordinal))
         yield return new SourceWork(new ReservoirSource(collectionName, show.Key, ReservoirSourceKind.Subtitles, $"{zip.FullName}/{show.Key}"),
                                     show.SelectMany(SubtitleFilesIn).SelectMany(it => it.Sentences()));

      string ShowOf(ZipArchiveEntry file) => file.FullName.Split('/') is var path && path.Length > 1 ? path[showsAreInOneRootFolder ? 1 : 0] : NoShowsFolder;
   }

   /// <summary>The shows in <paramref name="folder"/>, a folder of a folder per show.</summary>
   public static IEnumerable<SourceWork> WorksInFolder(string collectionName, DirectoryInfo folder) =>
      folder.GetDirectories()
            .OrderBy(it => it.Name, StringComparer.Ordinal)
            .Select(show => new SourceWork(new ReservoirSource(collectionName, show.Name, ReservoirSourceKind.Subtitles, show.FullName),
                                           show.GetFiles("*", SearchOption.AllDirectories)
                                               .SelectMany(file => SubtitleFilesIn(file.Name, () => file.OpenRead()))
                                               .SelectMany(it => it.Sentences())));

   /// <summary>Whether <paramref name="files"/>, a zip's files, are all in one folder, which holds them in folders of its own, as a folder per show.</summary>
   static bool AreAllInOneRootFolderOfFolders(IReadOnlyList<ZipArchiveEntry> files) =>
      files.Select(it => it.FullName.Split('/')[0]).Distinct().Count() == 1 && files.All(it => it.FullName.Count(character => character == '/') >= 2);

   /// <summary>Whether <paramref name="entry"/> is a file, not a folder, which a zip lists as an entry with no name.</summary>
   static bool IsAFile(ZipArchiveEntry entry) => entry.Name.Length > 0;

   static IEnumerable<SubtitleFile> SubtitleFilesIn(ZipArchiveEntry entry) => SubtitleFilesIn(entry.Name, entry.Open);

   /// <summary>The subtitle file named <paramref name="fileName"/>, or those in it when it's a zip, or none.</summary>
   static IEnumerable<SubtitleFile> SubtitleFilesIn(string fileName, Func<Stream> open)
   {
      if(SubtitleFile.IsOne(fileName)) return [new SubtitleFile(fileName, BytesOf(open))];
      if(IsAZip(fileName)) return SubtitleFilesInZip(BytesOf(open));
      return [];
   }

   static IEnumerable<SubtitleFile> SubtitleFilesInZip(byte[] zip)
   {
      using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
      return [.. archive.Entries.Where(IsAFile).SelectMany(SubtitleFilesIn)];
   }

   static bool IsAZip(string fileName) => Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase);

   static byte[] BytesOf(Func<Stream> open)
   {
      using var stream = open();
      using var bytes = new MemoryStream();
      stream.CopyTo(bytes);
      return bytes.ToArray();
   }
}
