using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.TagProperties;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
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
   /// Notifies this collection every time it has loaded again (<see cref="ReloadFromBackend"/>): at startup, after every Anki sync, and when asked to.<br/>
   /// The reload replaces every note object, so anything holding on to one must look it up again by its <see cref="NoteId"/>.
   /// </summary>
   public IObservable<JPCollection> ReloadedFromBackend => _reloadedFromBackend;

   readonly SimpleObservable<JPNote> _noteUpdated = new();

   /// <summary>Notifies every note in the collection that is updated, whatever its type, on the thread that updated it.</summary>
   public IObservable<JPNote> NoteUpdated => _noteUpdated;

   readonly SimpleObservable<JPNote> _noteDeleted = new();

   /// <summary>Notifies every note <see cref="Delete"/> deleted, on the thread that deleted it, after it and its media are gone.</summary>
   public IObservable<JPNote> NoteDeleted => _noteDeleted;

   readonly SimpleObservable<JPNote> _studyingStatusChanged = new();

   /// <summary>
   /// Notifies every note whose studying status changed, on the thread that changed it: when JAStudio suspended or unsuspended its cards,<br/>
   /// or Anki reported a change to one of its cards (<see cref="UpdateCardStudyingStatus"/>).
   /// </summary>
   /// <remarks>
   /// The studying status is held in memory, not stored with the note, so changing it updates no note, and <see cref="NoteUpdated"/> doesn't notify it.<br/>
   /// A reload sets every note's studying status without notifying it here: <see cref="ReloadedFromBackend"/> notifies the reload.
   /// </remarks>
   public IObservable<JPNote> StudyingStatusChanged => _studyingStatusChanged;

   /// <summary>Called by <paramref name="note"/> after its studying status changed, so that <see cref="StudyingStatusChanged"/> notifies it.</summary>
   internal void NotifyThatTheStudyingStatusChangedFor(JPNote note) => _studyingStatusChanged.OnNext(note);

   /// <summary>
   /// The notes, of every type, that match <paramref name="search"/>, found by checking every note in memory.<br/>
   /// They come in no particular order: whoever shows them sorts them.
   /// </summary>
   public List<JPNote> NotesMatching(NoteCondition search) => [..AllNotes().Where(search.Matches)];

   /// <summary>The tag paths in use in the collection right now, for the tag condition's editor to offer. See <see cref="Search.TagProperties.TagPathsInUse"/>.</summary>
   public TagPathsInUse TagPathsInUse() => new(AllNotes());

   /// <summary>Every tag some note in the collection has, each once. Unlike <see cref="TagPathsInUse"/>, it has no parent paths that aren't tags themselves.</summary>
   public IReadOnlyList<Tag> TagsInUse() => [..AllNotes().SelectMany(it => it.Tags).Distinct()];

   /// <summary>The notes, of every type, with a tag in <paramref name="branch"/>, which renaming or deleting the branch changes.</summary>
   public List<JPNote> NotesWithATagIn(TagBranch branch) => [..AllNotes().Where(note => note.Tags.Any(branch.Contains))];

   /// <summary>The tag tree of the tags in the collection right now, with the number of notes in each branch, for browsing the tags.</summary>
   public TagTree TagTree() => new(AllNotes());

   IReadOnlyList<JPNote> AllNotes() => [..Vocab.All(), ..Kanji.All(), ..Sentences.All()];

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

   /// <summary>Called by Anki when it suspended or unsuspended a card, or added a note: sets the studying status of the card's note, if JAStudio has the note.</summary>
   // ReSharper disable once UnusedMember.Global called from python
   public void UpdateCardStudyingStatus(CardStudyingStatus status)
   {
      if(NoteFromExternalId(status.ExternalNoteId) is not {} note) return;

      note.SetStudyingStatus(status);
      NotifyThatTheStudyingStatusChangedFor(note);
   }

   public JPCollection(
      IBackendNoteCreator backendNoteCreator,
      NoteServices noteServices,
      INoteRepository noteRepository,
      MediaFileIndex mediaFileIndex,
      MediaStorageService mediaStorage,
      IBackendDataLoader backendDataLoader)
   {
      using var _ = this.Log().Info().MethodExecutionTime();

      NoteServices = noteServices;
      _repository = noteRepository;
      _mediaFileIndex = mediaFileIndex;
      _mediaStorage = mediaStorage;
      _backendDataLoader = backendDataLoader;
      DictLookup = new DictLookup(this);
      VocabNoteGeneratedData = new VocabNoteGeneratedData();
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
   readonly MediaStorageService _mediaStorage;
   readonly IBackendDataLoader _backendDataLoader;

   /// <summary>
   /// Permanently deletes <paramref name="note"/> from JAStudio: its corpus data, the note in this collection, and its media files in the media layer<br/>
   /// that no other note uses. Their copies in Anki's media folder are left to Anki's "Check Media". Its Anki note, if it has one in this Anki profile,<br/>
   /// is kept or deleted as <paramref name="itsAnkiNote"/> says. Deleting a note in Anki instead only forgets its Anki note<br/>
   /// (<see cref="NoteCacheBase{TNote}.ExternalNoteRemoved"/>).
   /// </summary>
   public void Delete(JPNote note, ItsAnkiNote itsAnkiNote)
   {
      if(itsAnkiNote == ItsAnkiNote.IsDeleted) NoteServices.CardOperations.DeleteNote(note.GetId());
      _repository.Delete(note.GetId());
      RemoveFromItsCache();
      DeleteTheMediaNoOtherNoteUses();
      _noteDeleted.OnNext(note);
      return;

      void DeleteTheMediaNoOtherNoteUses()
      {
         var filesOfTheNote = note.MediaReferences.Select(it => it.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
         if(filesOfTheNote.Count == 0) return;

         var filesOtherNotesUse = AllNotes().SelectMany(it => it.MediaReferences).Select(it => it.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
         filesOfTheNote.Where(it => !filesOtherNotesUse.Contains(it) && _mediaFileIndex.ContainsByOriginalFileName(it))
                       .ToList()
                       .ForEach(_mediaStorage.DeleteFile);
      }

      void RemoveFromItsCache()
      {
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

   /// <summary>
   /// Clears this collection and loads it again: the notes from the corpus checkout, and the studying statuses and Anki note ids from Anki. It runs<br/>
   /// at startup, after every Anki sync, and when asked to, by the workspace's Local Actions > "Reload the collection from disk" or JAStudio's MCP<br/>
   /// server, to take in changes made to the corpus checkout outside JAStudio.
   /// </summary>
   /// <returns>The notes whose files changed on disk while this collection held them, as <see cref="ReloadFromBackendAfter"/> describes.</returns>
   public IReadOnlyList<JPNote> ReloadFromBackend() => ReloadFromBackendAfter(() => {});

   /// <summary>
   /// Makes <paramref name="changeToTheCorpusCheckout"/>, such as a git fast-forward of the corpus checkout, while this collection holds no notes, so that<br/>
   /// nothing JAStudio does meanwhile saves a note over the change, then loads this collection again, as <see cref="ReloadFromBackend"/> does. It loads<br/>
   /// again even when the change fails.
   /// </summary>
   /// <returns>
   /// The notes whose files changed on disk while this collection held them: each one's data now differs from the same note's as held before. Each<br/>
   /// is passed on as updated (<see cref="NoteUpdated"/>), so that what follows note updates catches up, such as its Anki note. Notes this collection<br/>
   /// didn't hold before aren't among them: none at startup, nor after a sync, which clears the collection when it starts.
   /// </returns>
   public IReadOnlyList<JPNote> ReloadFromBackendAfter(Action changeToTheCorpusCheckout)
   {
      var notesHeldBefore = AllNotes().ToDictionary(it => it.GetId());
      ClearCaches();
      try
      {
         changeToTheCorpusCheckout();
      }
      finally
      {
         LoadFromBackend();
      }

      List<JPNote> notesChangedOnDisk = [.. AllNotes().Where(ChangedOnDisk)];
      notesChangedOnDisk.ForEach(it => it.UpdateInCache());
      return notesChangedOnDisk;

      bool ChangedOnDisk(JPNote note) => notesHeldBefore.TryGetValue(note.GetId(), out var theNoteAsHeldBefore) && !note.HasTheSameStoredDataAs(theNoteAsHeldBefore);
   }

   void LoadFromBackend()
   {
      using var runner = NoteServices.TaskRunner.Current("=== Populating JAStudio data from file system ===");

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
