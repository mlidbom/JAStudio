using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Reservoir.Sources;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using JAStudio.Core.Reservoir.Sources.Subtitles;

namespace JAStudio.Core.Reservoir;

/// <summary>
/// Fills a <see cref="ReservoirFile"/> from where its sources are kept: folders of decks, and collections of subtitles (<see cref="SubtitleCollection"/>).<br/>
/// A work the reservoir already has is skipped, so a filling can be stopped and started again, and a source given twice is added once.
/// </summary>
public sealed class ReservoirFilling
{
   readonly ReservoirFile _reservoir;
   readonly Action<string> _report;

   /// <param name="reservoir">The reservoir to fill.</param>
   /// <param name="report">Told what each work added, and for a deck, which of its fields were recognized as what.</param>
   public ReservoirFilling(ReservoirFile reservoir, Action<string> report)
   {
      _reservoir = reservoir;
      _report = report;
   }

   /// <summary>
   /// Adds the decks in <paramref name="folder"/>, at any depth: each an .apkg file, or a folder holding a deck's collection database taken out of<br/>
   /// its package, named as in the package (<see cref="AnkiPackageDatabase.CollectionFileNamesNewestFirst"/>).
   /// </summary>
   public void AddDecksIn(string collectionName, DirectoryInfo folder)
   {
      foreach(var deck in DecksIn(folder).OrderBy(it => it.Name, StringComparer.Ordinal))
      {
         var source = new ReservoirSource(collectionName, deck.Name, ReservoirSourceKind.Deck, deck.FullName);
         if(_reservoir.Has(source)) continue;

         using var database = deck is FileInfo package ? AnkiPackageDatabase.InPackage(package) : AnkiPackageDatabase.InCollectionFile(CollectionFileIn((DirectoryInfo)deck));
         var ankiDeck = new AnkiDeck(database);
         _report($"{_reservoir.Add(new SourceWork(source, ankiDeck.Sentences()))}{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", ankiDeck.FieldRoles)}");
      }
   }

   /// <summary>Adds the shows in <paramref name="zipOrFolder"/>, a collection of subtitles kept as a folder per show, zipped or not.</summary>
   public void AddSubtitlesIn(string collectionName, FileSystemInfo zipOrFolder)
   {
      var works = zipOrFolder is FileInfo zip ? SubtitleCollection.WorksInZip(collectionName, zip) : SubtitleCollection.WorksInFolder(collectionName, (DirectoryInfo)zipOrFolder);
      foreach(var work in works.Where(it => !_reservoir.Has(it.Source)))
         _report(_reservoir.Add(work).ToString());
   }

   static IEnumerable<FileSystemInfo> DecksIn(DirectoryInfo folder) =>
      folder.GetFiles("*.apkg", SearchOption.AllDirectories)
            .Cast<FileSystemInfo>()
            .Concat(folder.GetDirectories("*", SearchOption.AllDirectories).Where(it => CollectionFilesIn(it).Any()));

   static FileInfo CollectionFileIn(DirectoryInfo deckFolder) => CollectionFilesIn(deckFolder).First();

   static IEnumerable<FileInfo> CollectionFilesIn(DirectoryInfo folder) =>
      AnkiPackageDatabase.CollectionFileNamesNewestFirst.Select(name => new FileInfo(Path.Combine(folder.FullName, name))).Where(it => it.Exists);
}
