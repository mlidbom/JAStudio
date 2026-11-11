using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using JAStudio.Core.Reservoir.Storage;
using JAStudio.Core.Reservoir.Storage.Format;
using JAStudio.Core.Reservoir.Storage.Media;

namespace JAStudio.Core.Reservoir.Importing;

/// <summary>
/// Imports Anki decks into the reservoir, stored as JAStudio's own so that nothing depends on where they were found: a deck's collection database<br/>
/// as it is, with a companion file saying where it came from and how its note types map to sentences; every note as a line of JSON, its fields as<br/>
/// they are and the sentence it gives; and its media, normalized, in the <see cref="MediaStore"/>. The text folder is meant for git; the media store isn't.
/// </summary>
/// <remarks>
/// The text folder holds <c>manifest.json</c> (<see cref="Manifest"/>), and for each deck a <see cref="DeckFolder"/>, <c>decks/&lt;package name&gt;/</c>,<br/>
/// with its collection database, <c>deck.json</c> (<see cref="DeckCompanion"/>) and <c>notes.jsonl</c> (<see cref="NoteLine"/>).<br/>
/// Once a deck is imported, its files are the truth, and its package only a backup: the files are never written again from the package, so the id<br/>
/// each line gives its sentence stays the sentence's. The format is meant never to change: should it have to, the files are migrated into a new<br/>
/// repository (see <c>dev_docs/todo/expanding-the-sentence-corpus.md</c>, "Keeping the reservoir safe").
/// </remarks>
public sealed class DeckImporter
{
   readonly DirectoryInfo _textFolder;
   readonly MediaStore _mediaStore;

   /// <summary>Imports decks into <paramref name="textFolder"/>, created with its manifest if need be, and their media into <paramref name="mediaStore"/>.</summary>
   public DeckImporter(DirectoryInfo textFolder, MediaStore mediaStore)
   {
      textFolder.Create();
      _textFolder = textFolder;
      _mediaStore = mediaStore;
      Json.WriteDocument(new FileInfo(Path.Combine(textFolder.FullName, "manifest.json")), Manifest.OfTheCurrentFormat);
   }

   /// <summary>
   /// Whether the deck in <paramref name="package"/>, an .apkg file, is imported already. Its folder must hold no other package's deck: another<br/>
   /// package with the same file name, as uploads from different people can have, is refused (<see cref="DeckFolder.HoldsTheDeckOf"/>).
   /// </summary>
   public bool HasImported(FileInfo package) => DeckFolder.Of(package, _textFolder).HoldsTheDeckOf(package);

   /// <summary>
   /// Imports the deck in <paramref name="package"/>, an .apkg file, which must not be imported yet (<see cref="HasImported"/>): its database, its<br/>
   /// media, its notes as lines, each with a new id, and, last, its companion file. A deck whose import was interrupted is imported anew.
   /// </summary>
   public ImportedDeck Import(FileInfo package)
   {
      var deckFolder = DeckFolder.Of(package, _textFolder);
      Contract.State.Assert(!deckFolder.HoldsADeck,
                            () => $"{deckFolder} holds an imported deck already, whose files are never written again from its package: to import {package.FullName} anew, delete the folder first.");
      deckFolder.Create();
      var packageAsRecorded = Package.Of(package);
      var collectionDatabase = deckFolder.CopyTheCollectionDatabaseOf(package);
      var media = _mediaStore.StoreTheMediaOf(package);

      using var database = AnkiPackageDatabase.InPackage(package);
      var deck = new AnkiDeck(database);
      var lines = deck.FieldRoles.SelectMany(roles => database.NotesOf(roles.NoteType).Select(note => NoteLine.Of(note, roles, media))).ToList();
      var noteLinesFile = deckFolder.WriteTheNoteLines(lines);
      deckFolder.WriteTheCompanion(DeckCompanion.Of(packageAsRecorded, collectionDatabase, deck, media));

      return new ImportedDeck(package.Name, lines.Count, lines.Count(it => it.SourceData is not null), collectionDatabase.Length, noteLinesFile.Length, media, MediaReferencesToFilesThePackageLacks(lines));
   }

   static int MediaReferencesToFilesThePackageLacks(IEnumerable<NoteLine> lines) =>
      lines.Select(it => it.SourceData).OfType<SentenceSourceData>().SelectMany(it => it.Audio.Concat(it.Screenshots)).Count(it => it.Hash is null);
}
