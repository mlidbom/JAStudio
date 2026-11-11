using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Reservoir.Archiving.Format;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.Core.Reservoir.Archiving;

/// <summary>
/// The folder a deck is archived in, <c>decks/&lt;package name&gt;/</c> in a <see cref="ReservoirArchive"/>'s text folder: the deck's collection database<br/>
/// as its package has it, its companion file <c>deck.json</c> (<see cref="ArchivedDeckCompanion"/>), and its notes as lines, <c>notes.jsonl</c><br/>
/// (<see cref="ArchivedNoteLine"/>).
/// </summary>
sealed class ArchivedDeckFolder
{
   readonly DirectoryInfo _folder;

   ArchivedDeckFolder(DirectoryInfo folder) => _folder = folder;

   /// <summary>
   /// The folder the deck in <paramref name="package"/>, an .apkg file whose hash and length are <paramref name="archivedPackage"/>, is archived in, in<br/>
   /// <paramref name="textFolder"/>, created if need be. It must hold no other package's deck: two packages with the same file name, as uploads<br/>
   /// from different people can have, would otherwise be archived in one folder, the second overwriting the first.
   /// </summary>
   public static ArchivedDeckFolder For(FileInfo package, ArchivedPackage archivedPackage, DirectoryInfo textFolder)
   {
      var folder = new ArchivedDeckFolder(textFolder.CreateSubdirectory(Path.Combine("decks", FolderNameOf(package))));
      Contract.State.Assert(folder.ArchivedPackageOrNone() is not {} archived || archived.Sha256 == archivedPackage.Sha256,
                            () => $"{folder._folder.FullName} holds the deck of another package named as {package.Name} is: archiving {package.FullName} would overwrite it.");
      return folder;
   }

   /// <summary>The package of the deck archived in the folder, from its <c>deck.json</c>, or none when no deck is archived in it yet.</summary>
   ArchivedPackage? ArchivedPackageOrNone() => CompanionFile.Exists ? ArchiveJson.Read<ArchivedDeckCompanion>(CompanionFile).Package : null;

   FileInfo CompanionFile => new(Path.Combine(_folder.FullName, "deck.json"));

   /// <summary>Copies the collection database <paramref name="package"/> is read from into the folder, as the package has it, and gives the copy.</summary>
   public FileInfo CopyTheCollectionDatabaseOf(FileInfo package)
   {
      using var archive = ZipFile.OpenRead(package.FullName);
      var entry = AnkiPackageDatabase.CollectionEntryIn(archive);
      var copy = new FileInfo(Path.Combine(_folder.FullName, entry.Name));
      entry.ExtractToFile(copy.FullName, overwrite: true);
      copy.Refresh();
      return copy;
   }

   /// <summary>Writes <paramref name="lines"/> as <c>notes.jsonl</c>, a line of JSON each, and gives the file.</summary>
   public FileInfo WriteTheNoteLines(IEnumerable<ArchivedNoteLine> lines)
   {
      var file = new FileInfo(Path.Combine(_folder.FullName, "notes.jsonl"));
      ArchiveJson.WriteLines(file, lines);
      return file;
   }

   /// <summary>Writes <paramref name="companion"/> as <c>deck.json</c>.</summary>
   public void WriteTheCompanion(ArchivedDeckCompanion companion) => ArchiveJson.WriteDocument(CompanionFile, companion);

   /// <summary>The name of the folder a deck is archived in: its package's file name without the extension, any character a file name can't hold made <c>_</c>.</summary>
   static string FolderNameOf(FileInfo package)
   {
      return string.Concat(Path.GetFileNameWithoutExtension(package.Name).Select(it => Path.GetInvalidFileNameChars().Contains(it) ? '_' : it)).TrimEnd(' ', '.');
   }
}
