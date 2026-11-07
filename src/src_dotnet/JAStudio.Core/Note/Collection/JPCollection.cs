using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Note.Collection;

public class JPCollection
{
   public VocabCollection Vocab { get; }
   public KanjiCollection Kanji { get; }
   public SentenceCollection Sentences { get; }

   public bool IsInitialized { get; private set; }

   readonly SimpleObservable<JPCollection> _reloadedFromBackend = new();

   /// <summary>
   /// Notifies this collection every time <see cref="ReloadFromBackend"/> completes: at startup, and after every Anki sync.<br/>
   /// The reload replaces every note object, so anything holding on to one must look it up again by its <see cref="NoteId"/>.
   /// </summary>
   public IObservable<JPCollection> ReloadedFromBackend => _reloadedFromBackend;

   readonly SimpleObservable<JPNote> _noteUpdated = new();

   /// <summary>Notifies every note in the collection that is updated, whatever its type, on the thread that updated it.</summary>
   public IObservable<JPNote> NoteUpdated => _noteUpdated;

   /// <summary>
   /// The notes, of every type, that match <paramref name="search"/>, found by checking every note in memory.<br/>
   /// They come in no particular order: whoever shows them sorts them.
   /// </summary>
   public List<JPNote> NotesMatching(NoteCondition search) => [..AllNotes().Where(search.Matches)];

   IEnumerable<JPNote> AllNotes() => [..Vocab.All(), ..Kanji.All(), ..Sentences.All()];

   public JPNote? NoteFromNoteId(NoteId noteId)
   {
      JPNote? note = Vocab.WithIdOrNone(noteId);
      if(note != null) return note;

      note = Kanji.WithIdOrNone(noteId);
      if(note != null) return note;

      return Sentences.WithIdOrNone(noteId);
   }

   /// <summary> Look up a note by its external long ID./// </summary>
   // ReSharper disable once UnusedMember.Global called from python
   public JPNote? NoteFromExternalId(long externalNoteId)
   {
      var noteId = NoteServices.ExternalNoteIdMap.FromExternalId(externalNoteId);
      return noteId != null ? NoteFromNoteId(noteId) : null;
   }

   /// <summary> Returns the external long note ID for the given domain NoteId. Returns 0 if no mapping is found.</summary>
   // ReSharper disable once UnusedMember.Global called from python
   public long GetExternalNoteId(NoteId noteId) => NoteServices.ExternalNoteIdMap.ToExternalId(noteId) ?? 0;

   public NoteServices NoteServices { get; }
   public VocabNoteFactory VocabNoteFactory { get; }
   public DictLookup DictLookup { get; }
   public VocabNoteGeneratedData VocabNoteGeneratedData { get; }

   // ReSharper disable once UnusedMember.Global called from python
   public void UpdateCardStudyingStatus(CardStudyingStatus status)
   {
      var note = NoteFromExternalId(status.ExternalNoteId);
      note?.SetStudyingStatus(status);
   }

   public JPCollection(
      IBackendNoteCreator backendNoteCreator,
      NoteServices noteServices,
      INoteRepository noteRepository,
      MediaFileIndex mediaFileIndex,
      IBackendDataLoader backendDataLoader)
   {
      using var _ = this.Log().Info().MethodExecutionTime();

      NoteServices = noteServices;
      _repository = noteRepository;
      _mediaFileIndex = mediaFileIndex;
      _backendDataLoader = backendDataLoader;
      DictLookup = new DictLookup(this);
      VocabNoteGeneratedData = new VocabNoteGeneratedData(DictLookup);
      VocabNoteFactory = new VocabNoteFactory(DictLookup, this, noteServices);

      Vocab = new VocabCollection(backendNoteCreator, NoteServices);
      Kanji = new KanjiCollection(backendNoteCreator, NoteServices);
      Sentences = new SentenceCollection(backendNoteCreator, NoteServices);

      Kanji.Cache.NoteUpdated.Subscribe(note => _repository.Save(note));
      Vocab.Cache.NoteUpdated.Subscribe(note => _repository.Save(note));
      Sentences.Cache.NoteUpdated.Subscribe(note => _repository.Save(note));

      Kanji.Cache.NoteUpdated.Subscribe(_noteUpdated.OnNext);
      Vocab.Cache.NoteUpdated.Subscribe(_noteUpdated.OnNext);
      Sentences.Cache.NoteUpdated.Subscribe(_noteUpdated.OnNext);
   }

   readonly INoteRepository _repository;
   readonly MediaFileIndex _mediaFileIndex;
   readonly IBackendDataLoader _backendDataLoader;

   /// <summary>Permanently delete a note from corpus data, Anki, and the in-memory cache.</summary>
   public void Delete(JPNote note)
   {
      _repository.Delete(note.GetId());

      if(NoteServices.ExternalNoteIdMap.ToExternalId(note.GetId()).HasValue)
      {
         // In Anki: delete from there; the notes_will_be_deleted hook fires reactively,
         // calling ExternalNoteRemoved which handles cache eviction automatically.
         NoteServices.CardOperations.DeleteNote(note.GetId());
      }
      else
      {
         // Not yet in Anki: evict from the in-memory cache manually.
         switch(note)
         {
            case VocabNote vocab: Vocab.Cache.RemoveFromCache(vocab); break;
            case KanjiNote kanji: Kanji.Cache.RemoveFromCache(kanji); break;
            case SentenceNote sentence: Sentences.Cache.RemoveFromCache(sentence); break;
         }
      }
   }

   /// <summary>Clear all in-memory caches. Called when the backend DB is about to become unreliable (e.g. sync starting, profile closing).</summary>
   public void ClearCaches() => this.Log().Warning().MethodExecutionTime(() =>
   {
      IsInitialized = false;
      NoteServices.ExternalNoteIdMap.Clear();
      Vocab.Cache.Clear();
      Kanji.Cache.Clear();
      Sentences.Cache.Clear();
   });

   /// <summary>Clear and reload all caches. Called after sync or collection reload.</summary>
   public void ReloadFromBackend()
   {
      using var runner = NoteServices.TaskRunner.Current("=== Populating JAStudio data from file system ===");

      ClearCaches();
      var repoLoad = TaskCE.Run(LoadFromRepository);
      var mediaIndexBuild = TaskCE.Run(() => _mediaFileIndex.Build());

      var backendDataTask = Task.Run(() => _backendDataLoader.Load(NoteServices.TaskRunner));

      Task.WaitAll(repoLoad, mediaIndexBuild, backendDataTask);

      var backendData = backendDataTask.Result;
      foreach(var (externalId, noteId) in backendData.IdMappings)
         NoteServices.ExternalNoteIdMap.Register(externalId, noteId);

      var vocabStatuses = backendData.StudyingStatuses
                                     .Where(s => s.NoteTypeName == NoteTypes.Vocab)
                                     .GroupBy(s => s.ExternalNoteId)
                                     .ToDictionary(g => g.Key, g => g.ToList());
      var kanjiStatuses = backendData.StudyingStatuses
                                     .Where(s => s.NoteTypeName == NoteTypes.Kanji)
                                     .GroupBy(s => s.ExternalNoteId)
                                     .ToDictionary(g => g.Key, g => g.ToList());
      var sentenceStatuses = backendData.StudyingStatuses
                                        .Where(s => s.NoteTypeName == NoteTypes.Sentence)
                                        .GroupBy(s => s.ExternalNoteId)
                                        .ToDictionary(g => g.Key, g => g.ToList());

      runner.RunIndeterminate("Setting studying statuses",
                              () =>
                              {
                                 Vocab.Cache.SetStudyingStatuses(vocabStatuses);
                                 Kanji.Cache.SetStudyingStatuses(kanjiStatuses);
                                 Sentences.Cache.SetStudyingStatuses(sentenceStatuses);
                              });

      IsInitialized = true;
      _reloadedFromBackend.OnNext(this);
   }

   void LoadFromRepository()
   {
      using var runner = NoteServices.TaskRunner.Current("Loading notes from repository");

      var allNotes = _repository.LoadAll();

      Task.WaitAll(
         runner.RunIndeterminateAsync("Pushing kanji notes into cache", () => Kanji.Cache.AddAllToCache(allNotes.Kanji)),
         runner.RunIndeterminateAsync("Pushing vocab notes into cache", () => Vocab.Cache.AddAllToCache(allNotes.Vocab)),
         runner.RunIndeterminateAsync("Pushing sentence notes into cache", () => Sentences.Cache.AddAllToCache(allNotes.Sentences)));
   }

}
