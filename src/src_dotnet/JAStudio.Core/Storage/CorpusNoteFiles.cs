using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Teventive.StateStore.Storage;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Storage;

/// <summary>
/// The notes' files in a corpus checkout, a file for each note, as the storage of a state store that keeps the notes: it loads every note's<br/>
/// snapshot, its DTO, from its file, and writes the files of the notes a unit of work changed, each as JAStudio has always written it.
/// </summary>
/// <remarks>
/// review-naming: CorpusNoteFiles is a name taggregates-opus chose (2026-11-16).<br/>
/// wip: <see cref="FileSystemNoteRepository"/> writes the same files, until the state store keeps the notes (dev_docs/taggregates/shared/the-migration.md, step 7).
/// </remarks>
public partial class CorpusNoteFiles : ITaggregateStorage
{
   readonly JsonFilesystemObjectRepository<KanjiDto> _kanji;
   readonly JsonFilesystemObjectRepository<VocabDto> _vocab;
   readonly JsonFilesystemObjectRepository<SentenceDto> _sentences;

   /// <summary>The notes' files in the corpus checkout <paramref name="corpusCheckoutFolder"/>.</summary>
   public CorpusNoteFiles(string corpusCheckoutFolder)
   {
      _kanji = new JsonFilesystemObjectRepository<KanjiDto>(Path.Combine(corpusCheckoutFolder, "kanji"), NoteSerializer.JsonOptions);
      _vocab = new JsonFilesystemObjectRepository<VocabDto>(Path.Combine(corpusCheckoutFolder, "vocab"), NoteSerializer.JsonOptions);
      _sentences = new JsonFilesystemObjectRepository<SentenceDto>(Path.Combine(corpusCheckoutFolder, "sentences"), NoteSerializer.JsonOptions);
   }

   public IReadOnlyList<TaggregateSnapshot> LoadAll() =>
   [
      .. _kanji.LoadAll().Select(it => new TaggregateSnapshot(typeof(KanjiNote), new KanjiId(it.Id), it)),
      .. _vocab.LoadAll().Select(it => new TaggregateSnapshot(typeof(VocabNote), new VocabId(it.Id), it)),
      .. _sentences.LoadAll().Select(it => new TaggregateSnapshot(typeof(SentenceNote), new SentenceId(it.Id), it))
   ];

   /// <summary>
   /// Writes each file of <paramref name="toSave"/> under a temporary name, for committing to give it its name. When writing one fails, those<br/>
   /// already written are discarded, and the failure goes on to the state store, which rolls the unit of work back.
   /// </summary>
   public IPreparedWrite Prepare(IReadOnlyList<TaggregateSnapshot> toSave, IReadOnlyList<TaggregateToDelete> toDelete)
   {
      var prepared = new List<IPreparedWrite>();
      try
      {
         foreach(var note in toSave) prepared.Add(PrepareSaving(note));
         foreach(var note in toDelete) prepared.Add(PrepareDeleting(note));
      }
      catch
      {
         new PreparedWrites(prepared).Discard();
         throw;
      }

      return new PreparedWrites(prepared);
   }

   IPreparedWrite PrepareSaving(TaggregateSnapshot note) => note.Snapshot switch
   {
      KanjiDto kanji => _kanji.PrepareSaving(kanji),
      VocabDto vocab => _vocab.PrepareSaving(vocab),
      SentenceDto sentence => _sentences.PrepareSaving(sentence),
      _ => throw new ArgumentException($"{note.TaggregateClass.FullName}'s snapshot is a {note.Snapshot.GetType().FullName}, which isn't a note's DTO.")
   };

   IPreparedWrite PrepareDeleting(TaggregateToDelete note)
   {
      if(note.TaggregateClass == typeof(KanjiNote)) return _kanji.PrepareDeleting(note.Id.Value);
      if(note.TaggregateClass == typeof(VocabNote)) return _vocab.PrepareDeleting(note.Id.Value);
      if(note.TaggregateClass == typeof(SentenceNote)) return _sentences.PrepareDeleting(note.Id.Value);
      throw new ArgumentException($"{note.TaggregateClass.FullName} isn't a class of note.");
   }
}
