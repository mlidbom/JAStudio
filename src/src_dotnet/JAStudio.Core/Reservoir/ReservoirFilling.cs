using System;
using System.IO;
using System.Linq;
using JAStudio.Core.Reservoir.Sources;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using JAStudio.Core.Reservoir.Sources.Subtitles;

namespace JAStudio.Core.Reservoir;

/// <summary>
/// Fills a <see cref="ReservoirFile"/> from where its sources are kept: folders of decks (<see cref="DeckOnDisk"/>), and collections of subtitles<br/>
/// (<see cref="SubtitleCollection"/>). A work the reservoir already has is skipped, so a filling can be stopped and started again, and a source<br/>
/// given twice is added once.
/// </summary>
public sealed class ReservoirFilling
{
   readonly ReservoirFile _reservoir;
   readonly Action<string> _tellProgress;

   /// <param name="reservoir">The reservoir to fill.</param>
   /// <param name="tellProgress">Told what each work added, and for a deck, which of its fields were recognized as what.</param>
   public ReservoirFilling(ReservoirFile reservoir, Action<string> tellProgress)
   {
      _reservoir = reservoir;
      _tellProgress = tellProgress;
   }

   /// <summary>Adds the decks in <paramref name="folder"/>, at any depth, in the order of their names (<see cref="DeckOnDisk.EveryDeckIn"/>).</summary>
   public void AddDecksIn(string collectionName, DirectoryInfo folder)
   {
      foreach(var deck in DeckOnDisk.EveryDeckIn(folder).OrderBy(it => it.Name, StringComparer.Ordinal))
      {
         var source = new ReservoirSource(collectionName, deck.Name, ReservoirSourceKind.Deck, deck.Location);
         if(_reservoir.Has(source)) continue;

         using var database = deck.OpenDatabase();
         var ankiDeck = new AnkiDeck(database);
         _tellProgress(Described(_reservoir.Add(new SourceWork(source, ankiDeck.Sentences())), ankiDeck));
      }
   }

   /// <summary>Adds the shows in <paramref name="zipOrFolder"/>, a collection of subtitles kept as a folder per show, zipped or not.</summary>
   public void AddSubtitlesIn(string collectionName, FileSystemInfo zipOrFolder)
   {
      foreach(var work in SubtitleCollection.WorksIn(collectionName, zipOrFolder).Where(it => !_reservoir.Has(it.Source)))
         _tellProgress(_reservoir.Add(work).ToString());
   }

   /// <summary>What adding <paramref name="deck"/> did, followed by the roles recognized in each of its note types' fields, a line each.</summary>
   static string Described(WorkAddedToTheReservoir added, AnkiDeck deck) =>
      $"{added}{Environment.NewLine}   {string.Join($"{Environment.NewLine}   ", deck.FieldRoles)}";
}
