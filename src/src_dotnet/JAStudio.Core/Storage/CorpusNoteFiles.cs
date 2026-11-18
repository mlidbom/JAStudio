using System;
using System.IO;
using Compze.Teventive.StateStore;
using Compze.Teventive.StateStore.MemoryPack;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage;

/// <summary>
/// The notes' files in a corpus checkout, a JSON file for each note, where the state store that keeps the notes stores them: Compze's storage of<br/>
/// taggregates in files (<see cref="StateStoreBuilder.StoredInJsonFiles{TService}"/>), each file holding exactly what<br/>
/// <see cref="NoteSerializer.JsonOptions"/> make of the note's snapshot, its DTO.
/// </summary>
/// <remarks>
/// Each class of note has a folder of its own in the corpus checkout, <c>vocab</c>, <c>kanji</c> and <c>sentences</c>, and each note's file in it is<br/>
/// <c>&lt;first two hex digits of its id&gt;/&lt;id&gt;.json</c>, as JAStudio has always stored them. Compze keeps files of its own in<br/>
/// <c>.state-store</c> in the corpus checkout, which the corpus repository ignores: the journals of its commits, and the cache it starts from,<br/>
/// a MemoryPack file of each class's snapshots.<br/>
/// review-naming: CorpusNoteFiles is a name taggregates-opus chose (2026-11-16).
/// </remarks>
public sealed class CorpusNoteFiles(DirectoryInfo corpusCheckout)
{
   const string VocabFolder = "vocab";
   const string KanjiFolder = "kanji";
   const string SentencesFolder = "sentences";

   /// <summary>The corpus checkout whose notes' files these are, such as <see cref="IEnvironmentPaths.DatabaseDir"/>.</summary>
   public DirectoryInfo CorpusCheckout { get; } = corpusCheckout;

   /// <summary>
   /// Declares that <paramref name="store"/> keeps the notes in their files in the corpus checkout of the <see cref="CorpusNoteFiles"/> the<br/>
   /// container holds, and starts from a cache of them, which reads only the files changed since it was written. Compze keeps each note's file<br/>
   /// as JAStudio always has: <c>&lt;its class's folder&gt;/&lt;first two hex digits of its id&gt;/&lt;id&gt;.json</c>.
   /// </summary>
   internal static void DeclareIn(StateStoreBuilder store) => DeclareIn(store, keptInMemory: false);

   /// <summary>
   /// As <see cref="DeclareIn(StateStoreBuilder)"/>, with the files kept in memory rather than on the disk, for the specifications: the corpus<br/>
   /// checkout is then only where the files' paths begin. They're read and changed through Compze's <see cref="ITaggregateFilesInMemory"/>.
   /// </summary>
   internal static void DeclareKeptInMemoryIn(StateStoreBuilder store) => DeclareIn(store, keptInMemory: true);

   static void DeclareIn(StateStoreBuilder store, bool keptInMemory) =>
      store.StoredInJsonFiles((CorpusNoteFiles files) => files.CorpusCheckout,
                          NoteSerializer.JsonOptions,
                          files =>
                          {
                             files.StartingFromAMemoryPackCache();
                             if(keptInMemory) files.KeptInMemory();
                             files.FolderOf<VocabNote>(VocabFolder)
                                  .FolderOf<KanjiNote>(KanjiFolder)
                                  .FolderOf<SentenceNote>(SentencesFolder);
                          });

   /// <summary>The file of <paramref name="note"/>, whether it has been written or not.</summary>
   /// <remarks>
   /// todo:compze: it works out the file's path by Compze's layout of a state store's files, which only Compze should know. Once Compze can give the<br/>
   ///    path of a taggregate's file, use that instead, and delete <see cref="FileIn"/>.
   /// </remarks>
   public FileInfo FileOf(JPNote note) => FileIn(note, new DirectoryInfo(Path.Combine(CorpusCheckout.FullName, FolderOf(note))));

   static string FolderOf(JPNote note) => note switch
   {
      VocabNote => VocabFolder,
      KanjiNote => KanjiFolder,
      SentenceNote => SentencesFolder,
      _ => throw new ArgumentOutOfRangeException(nameof(note), note.GetType(), "A kind of note the corpus doesn't store.")
   };

   static FileInfo FileIn(JPNote note, DirectoryInfo folderOfItsClass)
   {
      var id = note.GetId().Value;
      return new FileInfo(Path.Combine(folderOfItsClass.FullName, id.ToString("N")[..2], $"{id}.json"));
   }
}
