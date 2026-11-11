using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Reservoir.Archiving.Format;
using JAStudio.Core.Reservoir.Archiving.Media;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.Core.Reservoir.Archiving;

/// <summary>
/// The reservoir's archive: its sources as JAStudio's own, so that nothing depends on where they were found: a deck's collection database as it is, with<br/>
/// a companion file saying where it came from and how its note types map to sentences; every note as a line of JSON, its fields as they are and<br/>
/// the sentence it gives; and its media, normalized, in the <see cref="MediaStore"/>. The text folder is meant for git; the media store isn't.
/// </summary>
/// <remarks>
/// The text folder holds <c>manifest.json</c> (<see cref="ArchiveManifest"/>), and for each deck an <see cref="ArchivedDeckFolder"/>, <c>decks/&lt;package name&gt;/</c>,<br/>
/// with its collection database, <c>deck.json</c> (<see cref="ArchivedDeckCompanion"/>) and <c>notes.jsonl</c> (<see cref="ArchivedNoteLine"/>).<br/>
/// Once a deck is archived, its files are the truth, and its package only a backup: the files are never written again from the package, so the id<br/>
/// each line gives its sentence stays the sentence's. The format is meant never to change: should it have to, the files are migrated into a new<br/>
/// repository (see <c>dev_docs/todo/expanding-the-sentence-corpus.md</c>, "Keeping the reservoir safe").
/// </remarks>
public sealed class ReservoirArchive
{
   /// <summary>The version of the format of the archive's files, which <c>manifest.json</c> and each <c>deck.json</c> record.</summary>
   public const int FormatVersion = 1;

   const string Description = "Sources of JAStudio's sentence reservoir, archived as JAStudio's own. decks/<package name>/ holds a deck's collection database as its "
                            + "package had it, deck.json (the package, how its note types map to sentences, and its media files by the SHA-256 of their normalized "
                            + "content, which names them in the media store), and notes.jsonl, a line of JSON per note: its fields as they are, and the sentence it gives.";

   readonly DirectoryInfo _textFolder;
   readonly MediaStore _mediaStore;

   /// <summary>Archives sources in <paramref name="textFolder"/>, created with its manifest if need be, and their media in <paramref name="mediaStore"/>.</summary>
   public ReservoirArchive(DirectoryInfo textFolder, MediaStore mediaStore)
   {
      textFolder.Create();
      _textFolder = textFolder;
      _mediaStore = mediaStore;
      ArchiveJson.WriteDocument(new FileInfo(Path.Combine(textFolder.FullName, "manifest.json")), new ArchiveManifest(FormatVersion, Description));
   }

   /// <summary>
   /// Whether the deck in <paramref name="package"/>, an .apkg file, is archived already. Its folder must hold no other package's deck: another<br/>
   /// package with the same file name, as uploads from different people can have, is refused (<see cref="ArchivedDeckFolder.HoldsTheDeckOf"/>).
   /// </summary>
   public bool HasArchived(FileInfo package) => ArchivedDeckFolder.Of(package, _textFolder).HoldsTheDeckOf(package);

   /// <summary>
   /// Archives the deck in <paramref name="package"/>, an .apkg file, which must not be archived yet (<see cref="HasArchived"/>): its database, its<br/>
   /// media, its notes as lines, each with a new id, and, last, its companion file. A deck whose archiving was interrupted is archived anew.
   /// </summary>
   public ArchivedDeck Archive(FileInfo package)
   {
      var deckFolder = ArchivedDeckFolder.Of(package, _textFolder);
      Contract.State.Assert(!deckFolder.HoldsADeck,
                            () => $"{deckFolder} holds an archived deck already, whose files are never written again from its package: to archive {package.FullName} anew, delete the folder first.");
      deckFolder.Create();
      var archivedPackage = ArchivedPackage.Of(package);
      var collectionDatabase = deckFolder.CopyTheCollectionDatabaseOf(package);
      var media = _mediaStore.StoreTheMediaOf(package);

      using var database = AnkiPackageDatabase.InPackage(package);
      var deck = new AnkiDeck(database);
      var lines = deck.FieldRoles.SelectMany(roles => database.NotesOf(roles.NoteType).Select(note => ArchivedNoteLine.Of(note, roles, media))).ToList();
      var noteLinesFile = deckFolder.WriteTheNoteLines(lines);
      deckFolder.WriteTheCompanion(ArchivedDeckCompanion.Of(archivedPackage, collectionDatabase, deck, media));

      return new ArchivedDeck(package.Name, lines.Count, lines.Count(it => it.SourceData is not null), collectionDatabase.Length, noteLinesFile.Length, media, MediaReferencesToFilesThePackageLacks(lines));
   }

   static int MediaReferencesToFilesThePackageLacks(IEnumerable<ArchivedNoteLine> lines) =>
      lines.Select(it => it.SourceData).OfType<ArchivedSentenceSourceData>().SelectMany(it => it.Audio.Concat(it.Screenshots)).Count(it => it.Hash is null);
}
