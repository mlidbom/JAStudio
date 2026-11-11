using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// A deck as it's kept on disk: an Anki package, an .apkg file, or a folder holding a deck's collection database taken out of its package,<br/>
/// named as in the package (<see cref="AnkiPackageDatabase.CollectionFileNamesNewestFirst"/>).
/// </summary>
public sealed class DeckOnDisk
{
   readonly FileSystemInfo _packageOrFolder;
   readonly Func<AnkiPackageDatabase> _openDatabase;

   DeckOnDisk(FileSystemInfo packageOrFolder, Func<AnkiPackageDatabase> openDatabase)
   {
      _packageOrFolder = packageOrFolder;
      _openDatabase = openDatabase;
   }

   /// <summary>The decks in <paramref name="folder"/>, at any depth, in no particular order.</summary>
   public static IEnumerable<DeckOnDisk> EveryDeckIn(DirectoryInfo folder) =>
      folder.GetFiles("*.apkg", SearchOption.AllDirectories)
            .Select(package => new DeckOnDisk(package, () => AnkiPackageDatabase.InPackage(package)))
            .Concat(folder.GetDirectories("*", SearchOption.AllDirectories).Select(DeckInOrNone).OfType<DeckOnDisk>());

   /// <summary>The name of the package's file, or of the folder.</summary>
   public string Name => _packageOrFolder.Name;

   /// <summary>The full path of the package's file, or of the folder.</summary>
   public string Location => _packageOrFolder.FullName;

   /// <summary>The deck's collection database, opened to read; disposing it deletes any temporary copy it made.</summary>
   public AnkiPackageDatabase OpenDatabase() => _openDatabase();

   /// <summary>The deck taken out of its package into <paramref name="folder"/>, or none when the folder holds no collection database.</summary>
   static DeckOnDisk? DeckInOrNone(DirectoryInfo folder) =>
      NewestCollectionFileInOrNone(folder) is {} collectionFile ? new DeckOnDisk(folder, () => AnkiPackageDatabase.InCollectionFile(collectionFile)) : null;

   static FileInfo? NewestCollectionFileInOrNone(DirectoryInfo folder) =>
      AnkiPackageDatabase.CollectionFileNamesNewestFirst.Select(name => new FileInfo(Path.Combine(folder.FullName, name))).FirstOrDefault(it => it.Exists);
}
