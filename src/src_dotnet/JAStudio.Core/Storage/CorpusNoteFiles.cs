using System.Collections.Generic;
using System.IO;
using Compze.Teventive.MemoryImageStateStore;
using Compze.Teventive.MemoryImageStateStore.MemoryPack;
using Compze.Teventive.MemoryImageStateStore.Testing;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage;

/// <summary>
/// The notes' files in a corpus checkout, a JSON file for each note, where the memory image state store that keeps the notes stores them:<br/>
/// Compze's storage of<br/>
/// taggregates in files (<see cref="MemoryImageStateStoreBuilder.StoredInJsonFiles{TService}"/>), each file holding exactly what<br/>
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
   /// The folders of the corpus checkout that hold the notes' files, one for each class of note, as <see cref="DeclareIn(MemoryImageStateStoreBuilder)"/><br/>
   /// declares them to the store: where git looks for the notes' files that changed.
   /// </summary>
   public static IReadOnlyList<string> NoteFolders { get; } = [VocabFolder, KanjiFolder, SentencesFolder];

   /// <summary>
   /// Declares that <paramref name="store"/> keeps the notes in their files in the corpus checkout of the <see cref="CorpusNoteFiles"/> the<br/>
   /// container holds, and starts from a cache of them, which reads only the files changed since it was written. Compze keeps each note's file<br/>
   /// as JAStudio always has: <c>&lt;its class's folder&gt;/&lt;first two hex digits of its id&gt;/&lt;id&gt;.json</c>.
   /// </summary>
   internal static void DeclareIn(MemoryImageStateStoreBuilder store) => DeclareIn(store, onAFileSystemInMemory: false);

   /// <summary>
   /// As <see cref="DeclareIn(MemoryImageStateStoreBuilder)"/>, with the files kept in memory rather than on the disk, for the specifications: the corpus<br/>
   /// checkout is then only where the files' paths begin. They're read and changed through Compze's <see cref="IFileSystemInMemory"/>.
   /// </summary>
   internal static void DeclareOnAFileSystemInMemoryIn(MemoryImageStateStoreBuilder store) => DeclareIn(store, onAFileSystemInMemory: true);

   static void DeclareIn(MemoryImageStateStoreBuilder store, bool onAFileSystemInMemory) =>
      store.StoredInJsonFiles((CorpusNoteFiles files) => files.CorpusCheckout,
                          NoteSerializer.JsonOptions,
                          files =>
                          {
                             files.StartingFromAMemoryPackCache();
                             if(onAFileSystemInMemory) files.OnAFileSystemInMemory();
                             files.FolderOf<VocabNote>(VocabFolder)
                                  .FolderOf<KanjiNote>(KanjiFolder)
                                  .FolderOf<SentenceNote>(SentencesFolder);
                          });
}
