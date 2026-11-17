using System;
using System.IO;
using System.Text.Json;
using Compze.Teventive.StateStore;
using Compze.Teventive.StateStore.MemoryPack;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage;

/// <summary>
/// The notes' files in a corpus checkout, a JSON file for each note, where the state store that keeps the notes stores them: Compze's storage of<br/>
/// taggregates in files (<see cref="StateStoreBuilder.StoredInFiles{TService}"/>), each file holding exactly what<br/>
/// <see cref="NoteSerializer.JsonOptions"/> make of the note's snapshot, its DTO. Also writes a note's file outside any unit of work<br/>
/// (<see cref="Write"/>), for the tools that check how the notes are stored.
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
   /// container holds, and starts from a cache of them, which reads only the files changed since it was written. Each note's file is declared,<br/>
   /// rather than left to Compze's default, which is the same, since <see cref="Write"/> writes the same file.
   /// </summary>
   internal static void DeclareIn(StateStoreBuilder store) =>
      store.StoredInFiles((CorpusNoteFiles files) => files.CorpusCheckout,
                          NoteSerializer.JsonOptions,
                          files => files.Of<VocabNote>(VocabFolder, FileIn)
                                        .Of<KanjiNote>(KanjiFolder, FileIn)
                                        .Of<SentenceNote>(SentencesFolder, FileIn)
                                        .CachedWithMemoryPack());

   /// <summary>The file of <paramref name="note"/>, whether it has been written or not.</summary>
   public FileInfo FileOf(JPNote note) => FileIn(note, new DirectoryInfo(Path.Combine(CorpusCheckout.FullName, FolderOf(note))));

   /// <summary>
   /// Writes <paramref name="note"/>'s file as the state store writes it, as the note is now, changed or not, which a unit of work does only for the<br/>
   /// notes it changes: to see what loading and saving does to the notes' files, as after a change to how they are stored, or to change a note's<br/>
   /// file as an edit on disk would.
   /// </summary>
   /// <remarks>
   /// It writes outside any unit of work, so a unit of work committing the same note meanwhile may write over it, or it over the unit of work's.<br/>
   /// todo:compze: it writes the file as Compze's storage does, exactly what <see cref="NoteSerializer.JsonOptions"/> make of the snapshot, in UTF-8<br/>
   ///    with no byte order mark. Once Compze has a way to have its storage write every taggregate, changed or not, use it instead.
   /// </remarks>
   public void Write(JPNote note)
   {
      var file = FileOf(note);
      file.Directory?.Create();
      var snapshot = note.ToDto();
      File.WriteAllText(file.FullName, JsonSerializer.Serialize(snapshot, snapshot.GetType(), NoteSerializer.JsonOptions));
   }

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
