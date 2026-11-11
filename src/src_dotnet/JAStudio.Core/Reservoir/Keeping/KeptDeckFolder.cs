using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

   /// <summary>The folder the deck in <paramref name="package"/>, an .apkg file, is kept in, in <paramref name="textFolder"/>, created if need be.</summary>
   public static KeptDeckFolder For(FileInfo package, DirectoryInfo textFolder) => new(textFolder.CreateSubdirectory(Path.Combine("decks", FolderNameOf(package))));

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
   public void WriteTheCompanion(KeptDeckCompanion companion) => KeptJson.WriteDocument(new FileInfo(Path.Combine(_folder.FullName, "deck.json")), companion);

   /// <summary>The name of the folder a deck is kept in: its package's file name without the extension, any character a file name can't hold made <c>_</c>.</summary>
   static string FolderNameOf(FileInfo package)
   {
      //review: two packages with the same file name in different folders, as a folder of decks from many uploaders may hold, are kept in the
      //   same deck folder, the second overwriting the first's files without a word. So are two whose names differ only in characters made _
      //   here, which characters depend on the platform, or, on Windows, only in case.
      return string.Concat(Path.GetFileNameWithoutExtension(package.Name).Select(it => Path.GetInvalidFileNameChars().Contains(it) ? '_' : it)).TrimEnd(' ', '.');
   }
}
