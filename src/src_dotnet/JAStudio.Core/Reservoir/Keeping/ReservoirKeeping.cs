using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Reservoir.Keeping.Format;
using JAStudio.Core.Reservoir.Sources;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Reservoir.Keeping;

/// <summary>
/// Keeps the reservoir's sources as JAStudio's own, so that nothing depends on where they were found: a deck's collection database as it is, with<br/>
/// a companion file saying where it came from and how its note types map to sentences; every note as a line of JSON, its fields as they are and<br/>
/// the sentence it gives; and its media, normalized, in the <see cref="MediaStore"/>. The text folder is meant for git; the media store isn't.
/// </summary>
/// <remarks>
/// The text folder holds <c>manifest.json</c>, and for each deck <c>decks/&lt;package name&gt;/</c> with its collection database, <c>deck.json</c><br/>
/// (<see cref="KeptDeckCompanion"/>) and <c>notes.jsonl</c> (<see cref="KeptNoteLine"/>). The format is meant never to change: should it have to,<br/>
/// the sources are kept anew in a new folder (see <c>dev_docs/todo/expanding-the-sentence-corpus.md</c>, "Keeping the reservoir safe").
/// </remarks>
public sealed class ReservoirKeeping
{
   /// <summary>The version of the format of the files kept, which <c>manifest.json</c> and each <c>deck.json</c> record.</summary>
   public const int FormatVersion = 1;

   const string Description = "Sources of JAStudio's sentence reservoir, kept as JAStudio's own. decks/<package name>/ holds a deck's collection database as its "
                            + "package had it, deck.json (the package, how its note types map to sentences, and its media files by the SHA-256 of their normalized "
                            + "content, which names them in the media store), and notes.jsonl, a line of JSON per note: its fields as they are, and the sentence it gives.";

   static readonly JsonSerializerOptions LineJson = new()
                                                    {
                                                       PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                       Encoder = FullUnicodeJsonEncoder.Instance,
                                                       DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                                                    };

   static readonly JsonSerializerOptions DocumentJson = new(LineJson) { WriteIndented = true };

   readonly DirectoryInfo _textFolder;
   readonly MediaStore _media;

   /// <summary>Keeps sources in <paramref name="textFolder"/>, created with its manifest if need be, and their media in <paramref name="media"/>.</summary>
   public ReservoirKeeping(DirectoryInfo textFolder, MediaStore media)
   {
      textFolder.Create();
      _textFolder = textFolder;
      _media = media;
      WriteJson(new FileInfo(Path.Combine(textFolder.FullName, "manifest.json")), new KeptManifest(FormatVersion, Description));
   }

   /// <summary>Keeps the deck in <paramref name="package"/>, an .apkg file: its database, its companion file, its notes as lines, and its media.</summary>
   public KeptDeck Keep(FileInfo package)
   {
      var deckFolder = _textFolder.CreateSubdirectory(Path.Combine("decks", FolderNameOf(package)));
      var collectionDatabase = CopyTheCollectionDatabase(package, deckFolder);
      var watch = Stopwatch.StartNew();
      var media = AnkiPackageMedia.In(package).AsParallel().Select(_media.Store).ToList();
      var mediaTime = watch.Elapsed;
      var hashesByName = media.ToDictionary(it => it.OriginalName, it => it.Hash);

      using var database = AnkiPackageDatabase.InPackage(package);
      var deck = new AnkiDeck(database);
      var lines = deck.FieldRoles.SelectMany(roles => database.NotesOf(roles.NoteType).Select(note => LineOf(note, roles, hashesByName))).ToList();
      var notesFile = new FileInfo(Path.Combine(deckFolder.FullName, "notes.jsonl"));
      File.WriteAllLines(notesFile.FullName, lines.Select(it => JsonSerializer.Serialize(it, LineJson)));
      WriteJson(new FileInfo(Path.Combine(deckFolder.FullName, "deck.json")),
                new KeptDeckCompanion(FormatVersion, PackageOf(package), collectionDatabase.Name, [.. deck.FieldRoles.Select(MappingOf)],
                                      [.. media.Select(it => new KeptMediaFile(it.OriginalName, it.Hash, it.GainStepsApplied))]));

      return new KeptDeck(package.Name, lines.Count, lines.Count(it => it.SourceData is not null), collectionDatabase.Length, notesFile.Length,
                          media.Count, media.Sum(it => (long)it.OriginalLength), media.Count(it => it.WasNewToTheStore), MediaReferencesLackingAFile(lines), mediaTime);
   }

   static KeptNoteLine LineOf(RawNote note, DeckFieldRoles roles, IReadOnlyDictionary<string, string> hashesByName) =>
      new(Guid.NewGuid(),
          note.AnkiNoteId,
          roles.NoteType.Name,
          note.Tags,
          roles.NoteType.FieldNames.ToDictionary(name => name, note.Field),
          roles.SentenceOf(note) is {} sentence ? SourceDataOf(sentence, hashesByName) : null);

   static KeptSentenceSourceData SourceDataOf(SourceSentence sentence, IReadOnlyDictionary<string, string> hashesByName)
   {
      return new KeptSentenceSourceData(sentence.Text.Text, sentence.Translation ?? "", sentence.Position, ReferencesTo(sentence.AudioFile), ReferencesTo(sentence.ImageFile));

      // A deck can refer to a media file its package lacks: the reference is kept, with no hash.
      IReadOnlyList<KeptMediaReference> ReferencesTo(string? fileName) => fileName is null ? [] : [new KeptMediaReference(fileName, hashesByName.GetValueOrDefault(fileName))];
   }

   static int MediaReferencesLackingAFile(IEnumerable<KeptNoteLine> lines) =>
      lines.Select(it => it.SourceData).OfType<KeptSentenceSourceData>().SelectMany(it => it.Audio.Concat(it.Screenshots)).Count(it => it.Hash is null);

   static KeptNoteTypeMapping MappingOf(DeckFieldRoles roles) =>
      new(roles.NoteType.Name, roles.NoteType.FieldNames, roles.JapaneseField, roles.JapaneseFieldHoldsFurigana, roles.JapaneseFieldHoldsWordsRatherThanSentences,
          roles.TranslationField, roles.AudioField, roles.ImageField);

   static KeptPackage PackageOf(FileInfo package)
   {
      using var stream = package.OpenRead();
      return new KeptPackage(package.Name, package.Length, Convert.ToHexStringLower(SHA256.HashData(stream)));
   }

   /// <summary>Copies the collection database <paramref name="package"/> is read from into <paramref name="deckFolder"/>, as the package has it, and gives the copy.</summary>
   static FileInfo CopyTheCollectionDatabase(FileInfo package, DirectoryInfo deckFolder)
   {
      using var archive = ZipFile.OpenRead(package.FullName);
      var entry = AnkiPackageDatabase.CollectionFileNamesNewestFirst.Select(archive.GetEntry).OfType<ZipArchiveEntry>().First();
      var copy = new FileInfo(Path.Combine(deckFolder.FullName, entry.Name));
      entry.ExtractToFile(copy.FullName, overwrite: true);
      copy.Refresh();
      return copy;
   }

   /// <summary>The name of the folder a deck is kept in: its package's file name without the extension, any character a file name can't hold made <c>_</c>.</summary>
   static string FolderNameOf(FileInfo package) =>
      string.Concat(Path.GetFileNameWithoutExtension(package.Name).Select(it => Path.GetInvalidFileNameChars().Contains(it) ? '_' : it)).TrimEnd(' ', '.');

   static void WriteJson<T>(FileInfo file, T value) => File.WriteAllText(file.FullName, JsonSerializer.Serialize(value, DocumentJson));
}
