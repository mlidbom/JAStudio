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
/// <remarks>
/// The companion file is written last, so a folder holding one holds a deck archived whole, and a folder holding files but no companion file, a deck<br/>
/// whose archiving was interrupted.
/// </remarks>
sealed class ArchivedDeckFolder
{
   readonly DirectoryInfo _folder;

   ArchivedDeckFolder(DirectoryInfo folder) => _folder = folder;

   /// <summary>The folder the deck in <paramref name="package"/>, an .apkg file, is archived in, in <paramref name="textFolder"/>, whether it's archived yet or not.</summary>
   public static ArchivedDeckFolder Of(FileInfo package, DirectoryInfo textFolder) => new(new DirectoryInfo(Path.Combine(textFolder.FullName, "decks", FolderNameOf(package))));

   /// <summary>Whether a deck is archived whole in the folder: its companion file, written last, exists.</summary>
   public bool HoldsADeck => CompanionFile.Exists;

   /// <summary>
   /// Whether the deck in <paramref name="package"/>, an .apkg file, is archived whole in the folder. Any deck archived in it must be that package's:<br/>
   /// two packages with the same file name, as uploads from different people can have, have the same folder, and the second is refused.
   /// </summary>
   public bool HoldsTheDeckOf(FileInfo package)
   {
      if(!HoldsADeck) return false;
      Contract.State.Assert(PackageOfItsDeck().Sha256 == ArchivedPackage.Of(package).Sha256,
                            () => $"{_folder.FullName} holds the deck of another package named as {package.Name} is, not the deck of {package.FullName}.");
      return true;
   }

   /// <summary>The package of the deck archived in the folder, from its <c>deck.json</c>.</summary>
   ArchivedPackage PackageOfItsDeck() => ArchiveJson.Read<ArchivedDeckCompanion>(CompanionFile).Package;

   FileInfo CompanionFile => new(Path.Combine(_folder.FullName, "deck.json"));

   /// <summary>Creates the folder, unless it exists: it does when an archiving of its deck was interrupted.</summary>
   public void Create() => _folder.Create();

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

   /// <summary>Writes <paramref name="companion"/> as <c>deck.json</c>, last, as it says the deck is archived whole.</summary>
   public void WriteTheCompanion(ArchivedDeckCompanion companion) => ArchiveJson.WriteDocument(CompanionFile, companion);

   public override string ToString() => _folder.FullName;

   /// <summary>The name of the folder a deck is archived in: its package's file name without the extension, any character a file name can't hold made <c>_</c>.</summary>
   static string FolderNameOf(FileInfo package)
   {
      return string.Concat(Path.GetFileNameWithoutExtension(package.Name).Select(it => Path.GetInvalidFileNameChars().Contains(it) ? '_' : it)).TrimEnd(' ', '.');
   }
}
