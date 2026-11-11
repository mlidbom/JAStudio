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
/// <remarks>
/// A zip in a show's folder that can't be read, as a corrupt one, is left out, and told of through the <c>tellOfAnUnreadableArchive</c> each<br/>
/// method takes; the show's other subtitles are read as usual.
/// </remarks>
public static partial class SubtitleCollection
{
   const string NoShowsFolder = "(no show's folder)";

   /// <summary>The shows in <paramref name="zipOrFolder"/>: a zip (<see cref="WorksInZip"/>) or a folder (<see cref="WorksInFolder"/>).</summary>
   /// <param name="tellOfAnUnreadableArchive">Told of each zip in a show's folder that's left out because it can't be read, naming it and the show.</param>
   public static IEnumerable<SourceWork> WorksIn(string collectionName, FileSystemInfo zipOrFolder, Action<string> tellOfAnUnreadableArchive) =>
      zipOrFolder is FileInfo zip ? WorksInZip(collectionName, zip, tellOfAnUnreadableArchive) : WorksInFolder(collectionName, (DirectoryInfo)zipOrFolder, tellOfAnUnreadableArchive);

   /// <summary>The shows in <paramref name="zip"/>, a zip of a folder per show, or of one folder holding them, such as <c>kitsunekko_backup/</c>.</summary>
   /// <param name="tellOfAnUnreadableArchive">Told of each zip in a show's folder that's left out because it can't be read, naming it and the show.</param>
   public static IEnumerable<SourceWork> WorksInZip(string collectionName, FileInfo zip, Action<string> tellOfAnUnreadableArchive)
   {
      using var archive = ZipFile.OpenRead(zip.FullName);
      var files = archive.Entries.Where(IsAFile).ToList();
      var showsAreInOneRootFolder = AreAllInOneRootFolderOfFolders(files);
      foreach(var show in files.GroupBy(ShowOf).OrderBy(it => it.Key, StringComparer.Ordinal))
         yield return new SourceWork(new ReservoirSource(collectionName, show.Key, ReservoirSourceKind.Subtitles, $"{zip.FullName}/{show.Key}"),
                                     show.SelectMany(file => SubtitleFilesIn(new FileInAShowsFolder(show.Key, file.Name, file.Open), tellOfAnUnreadableArchive))
                                         .SelectMany(it => it.Sentences()));

      string ShowOf(ZipArchiveEntry file) => file.FullName.Split('/') is var path && path.Length > 1 ? path[showsAreInOneRootFolder ? 1 : 0] : NoShowsFolder;
   }

   /// <summary>The shows in <paramref name="folder"/>, a folder of a folder per show.</summary>
   /// <param name="tellOfAnUnreadableArchive">Told of each zip in a show's folder that's left out because it can't be read, naming it and the show.</param>
   public static IEnumerable<SourceWork> WorksInFolder(string collectionName, DirectoryInfo folder, Action<string> tellOfAnUnreadableArchive) =>
      folder.GetDirectories()
            .OrderBy(it => it.Name, StringComparer.Ordinal)
            .Select(show => new SourceWork(new ReservoirSource(collectionName, show.Name, ReservoirSourceKind.Subtitles, show.FullName),
                                           show.GetFiles("*", SearchOption.AllDirectories)
                                               .SelectMany(file => SubtitleFilesIn(new FileInAShowsFolder(show.Name, file.Name, () => file.OpenRead()), tellOfAnUnreadableArchive))
                                               .SelectMany(it => it.Sentences())));

   /// <summary>Whether <paramref name="files"/>, a zip's files, are all in one folder, which holds them in folders of its own, as a folder per show.</summary>
   static bool AreAllInOneRootFolderOfFolders(IReadOnlyList<ZipArchiveEntry> files) =>
      files.Select(it => it.FullName.Split('/')[0]).Distinct().Count() == 1 && files.All(it => it.FullName.Count(character => character == '/') >= 2);

   /// <summary>Whether <paramref name="entry"/> is a file, not a folder, which a zip lists as an entry with no name.</summary>
   static bool IsAFile(ZipArchiveEntry entry) => entry.Name.Length > 0;

   /// <summary><paramref name="file"/>, when it's a subtitle file, or the subtitle files in it, when it's a zip, or none.</summary>
   static IEnumerable<SubtitleFile> SubtitleFilesIn(FileInAShowsFolder file, Action<string> tellOfAnUnreadableArchive)
   {
      if(SubtitleFile.IsOne(file.Name)) return [new SubtitleFile(file.Name, BytesOf(file.Open))];
      if(IsAZip(file.Name)) return SubtitleFilesInZip(file, tellOfAnUnreadableArchive);
      return [];
   }

   /// <summary>The subtitle files in <paramref name="zip"/>, a zip in a show's folder, or none when it can't be read, which <paramref name="tellOfAnUnreadableArchive"/> is told of.</summary>
   static IEnumerable<SubtitleFile> SubtitleFilesInZip(FileInAShowsFolder zip, Action<string> tellOfAnUnreadableArchive)
   {
      var bytes = BytesOf(zip.Open);
      try
      {
         using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
         return [.. archive.Entries.Where(IsAFile).SelectMany(entry => SubtitleFilesIn(new FileInAShowsFolder(zip.Show, entry.Name, entry.Open), tellOfAnUnreadableArchive))];
      }
      // A zip in a show's folder of the kitsunekko backup is corrupt: "End of Central Directory record could not be found." (2026-11-11, filling
      // the reservoir from kitsunekko_backup.zip). Such external data can't be guaranteed sound, and the rest of the show is worth reading.
      catch(InvalidDataException unreadable)
      {
         tellOfAnUnreadableArchive($"{zip.Show}: the zip {zip.Name} can't be read, so its subtitles are left out: {unreadable.Message}");
         return [];
      }
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
