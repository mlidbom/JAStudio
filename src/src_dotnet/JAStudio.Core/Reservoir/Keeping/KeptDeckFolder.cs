using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Reservoir.Keeping.Format;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.Core.Reservoir.Keeping;

/// <summary>
/// The folder a deck is kept in, <c>decks/&lt;package name&gt;/</c> in a <see cref="ReservoirKeeping"/>'s text folder: the deck's collection database<br/>
/// as its package has it, its companion file <c>deck.json</c> (<see cref="KeptDeckCompanion"/>), and its notes as lines, <c>notes.jsonl</c><br/>
/// (<see cref="KeptNoteLine"/>).
/// </summary>
sealed class KeptDeckFolder
{
   readonly DirectoryInfo _folder;

   KeptDeckFolder(DirectoryInfo folder) => _folder = folder;

   /// <summary>
   /// The folder the deck in <paramref name="package"/>, an .apkg file whose hash and length are <paramref name="keptPackage"/>, is kept in, in<br/>
   /// <paramref name="textFolder"/>, created if need be. It must hold no other package's deck: two packages with the same file name, as uploads<br/>
   /// from different people can have, would otherwise be kept in one folder, the second overwriting the first.
   /// </summary>
   public static KeptDeckFolder For(FileInfo package, KeptPackage keptPackage, DirectoryInfo textFolder)
   {
      var folder = new KeptDeckFolder(textFolder.CreateSubdirectory(Path.Combine("decks", FolderNameOf(package))));
      Contract.State.Assert(folder.KeptPackageOrNone() is not {} kept || kept.Sha256 == keptPackage.Sha256,
                            () => $"{folder._folder.FullName} holds the deck of another package named as {package.Name} is: keeping {package.FullName} would overwrite it.");
      return folder;
   }

   /// <summary>The package of the deck kept in the folder, from its <c>deck.json</c>, or none when no deck is kept in it yet.</summary>
   KeptPackage? KeptPackageOrNone() => CompanionFile.Exists ? KeptJson.Read<KeptDeckCompanion>(CompanionFile).Package : null;

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
   public FileInfo WriteTheNoteLines(IEnumerable<KeptNoteLine> lines)
   {
      var file = new FileInfo(Path.Combine(_folder.FullName, "notes.jsonl"));
      KeptJson.WriteLines(file, lines);
      return file;
   }

   /// <summary>Writes <paramref name="companion"/> as <c>deck.json</c>.</summary>
   public void WriteTheCompanion(KeptDeckCompanion companion) => KeptJson.WriteDocument(CompanionFile, companion);

   /// <summary>The name of the folder a deck is kept in: its package's file name without the extension, any character a file name can't hold made <c>_</c>.</summary>
   static string FolderNameOf(FileInfo package)
   {
      return string.Concat(Path.GetFileNameWithoutExtension(package.Name).Select(it => Path.GetInvalidFileNameChars().Contains(it) ? '_' : it)).TrimEnd(' ', '.');
   }
}
