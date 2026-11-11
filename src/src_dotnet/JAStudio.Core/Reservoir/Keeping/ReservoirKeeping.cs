using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Reservoir.Keeping.Format;
using JAStudio.Core.Reservoir.Keeping.Media;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.Core.Reservoir.Keeping;

/// <summary>
/// Keeps the reservoir's sources as JAStudio's own, so that nothing depends on where they were found: a deck's collection database as it is, with<br/>
/// a companion file saying where it came from and how its note types map to sentences; every note as a line of JSON, its fields as they are and<br/>
/// the sentence it gives; and its media, normalized, in the <see cref="MediaStore"/>. The text folder is meant for git; the media store isn't.
/// </summary>
/// <remarks>
/// The text folder holds <c>manifest.json</c> (<see cref="KeptManifest"/>), and for each deck a <see cref="KeptDeckFolder"/>, <c>decks/&lt;package name&gt;/</c>,<br/>
/// with its collection database, <c>deck.json</c> (<see cref="KeptDeckCompanion"/>) and <c>notes.jsonl</c> (<see cref="KeptNoteLine"/>). The format<br/>
/// is meant never to change: should it have to, the sources are kept anew in a new folder (see <c>dev_docs/todo/expanding-the-sentence-corpus.md</c>,<br/>
/// "Keeping the reservoir safe").
/// </remarks>
public sealed class ReservoirKeeping
{
   /// <summary>The version of the format of the files kept, which <c>manifest.json</c> and each <c>deck.json</c> record.</summary>
   public const int FormatVersion = 1;

   const string Description = "Sources of JAStudio's sentence reservoir, kept as JAStudio's own. decks/<package name>/ holds a deck's collection database as its "
                            + "package had it, deck.json (the package, how its note types map to sentences, and its media files by the SHA-256 of their normalized "
                            + "content, which names them in the media store), and notes.jsonl, a line of JSON per note: its fields as they are, and the sentence it gives.";

   readonly DirectoryInfo _textFolder;
   readonly MediaStore _mediaStore;

   /// <summary>Keeps sources in <paramref name="textFolder"/>, created with its manifest if need be, and their media in <paramref name="mediaStore"/>.</summary>
   public ReservoirKeeping(DirectoryInfo textFolder, MediaStore mediaStore)
   {
      textFolder.Create();
      _textFolder = textFolder;
      _mediaStore = mediaStore;
      KeptJson.WriteDocument(new FileInfo(Path.Combine(textFolder.FullName, "manifest.json")), new KeptManifest(FormatVersion, Description));
   }

   /// <summary>Keeps the deck in <paramref name="package"/>, an .apkg file: its database, its media, its notes as lines, and its companion file.</summary>
   public KeptDeck Keep(FileInfo package)
   {
      var keptPackage = KeptPackage.Of(package);
      var deckFolder = KeptDeckFolder.For(package, keptPackage, _textFolder);
      var collectionDatabase = deckFolder.CopyTheCollectionDatabaseOf(package);
      var media = _mediaStore.StoreTheMediaOf(package);

      using var database = AnkiPackageDatabase.InPackage(package);
      var deck = new AnkiDeck(database);
      var lines = deck.FieldRoles.SelectMany(roles => database.NotesOf(roles.NoteType).Select(note => KeptNoteLine.Of(note, roles, media, keptPackage))).ToList();
      var noteLinesFile = deckFolder.WriteTheNoteLines(lines);
      deckFolder.WriteTheCompanion(KeptDeckCompanion.Of(keptPackage, collectionDatabase, deck, media));

      return new KeptDeck(package.Name, lines.Count, lines.Count(it => it.SourceData is not null), collectionDatabase.Length, noteLinesFile.Length, media, MediaReferencesToFilesThePackageLacks(lines));
   }

   static int MediaReferencesToFilesThePackageLacks(IEnumerable<KeptNoteLine> lines) =>
      lines.Select(it => it.SourceData).OfType<KeptSentenceSourceData>().SelectMany(it => it.Audio.Concat(it.Screenshots)).Count(it => it.Hash is null);
}
