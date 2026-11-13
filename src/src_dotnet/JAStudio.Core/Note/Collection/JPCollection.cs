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
using JAStudio.Core.SysUtils.Threading;

namespace JAStudio.Core.Note.Collection;

public class JPCollection
{
   public VocabCollection Vocab { get; }
   public KanjiCollection Kanji { get; }
   public SentenceCollection Sentences { get; }

   public bool IsInitialized { get; private set; }

   readonly SimpleObservable<JPCollection> _reloadedFromBackend = new();

   /// <summary>
   /// Notifies this collection every time it has loaded again (<see cref="ReloadFromBackend"/>): at startup, and when asked to.<br/>
   /// The reload replaces every note object, so anything holding on to one must look it up again by its <see cref="NoteId"/>.
   /// </summary>
   public IObservable<JPCollection> ReloadedFromBackend => _reloadedFromBackend;

   readonly SimpleObservable<JPNote> _noteUpdated = new();

   /// <summary>Notifies every note in the collection that is updated, whatever its type, on the thread that updated it.</summary>
   public IObservable<JPNote> NoteUpdated => _noteUpdated;

   readonly SimpleObservable<JPNote> _noteDeleted = new();

   /// <summary>Notifies every note <see cref="Delete"/> deleted, on the thread that deleted it, after it and its media are gone.</summary>
   public IObservable<JPNote> NoteDeleted => _noteDeleted;

   readonly SimpleObservable<JPNote> _studyNoteChanged = new();

   /// <summary>
   /// Notifies every note whose study-note in Anki changed, on the thread that changed it: its studying status, when JAStudio suspended or unsuspended<br/>
   /// its cards or Anki reported a change to one of them (<see cref="UpdateCardStudyingStatus"/>), its <see cref="JPNote.AnkiTags"/>, or its study-note<br/>
   /// itself, created or deleted, as reading the study-notes from Anki again finds (<see cref="ReadTheStudyNotesFromAnkiAgain"/>).
   /// </summary>
   /// <remarks>
   /// Both are held in memory, not stored with the note, so changing them updates no note, and <see cref="NoteUpdated"/> doesn't notify them.<br/>
   /// A reload sets every note's without notifying them here: <see cref="ReloadedFromBackend"/> notifies the reload.
   /// </remarks>
   public IObservable<JPNote> StudyNoteChanged => _studyNoteChanged;

   /// <summary>Called after <paramref name="note"/>'s studying status or Anki tags changed, so that <see cref="StudyNoteChanged"/> notifies it.</summary>
   internal void NotifyThatTheStudyNoteChangedFor(JPNote note) => _studyNoteChanged.OnNext(note);

   /// <summary>
   /// Renames every word-reference that names <paramref name="oldName"/> exactly to <paramref name="newName"/>, after the vocab that had the one name got the<br/>
   /// other: in every vocab's related vocab and compound parts (<see cref="VocabCollection.RenameTheReferences"/>), and every kanji's primary vocab<br/>
   /// (<see cref="KanjiCollection.RenameThePrimaryVocab"/>).
   /// </summary>
   internal void RenameTheWordReferences(WordReference oldName, WordReference newName)
   {
      Vocab.RenameTheReferences(oldName, newName);
      Kanji.RenameThePrimaryVocab(oldName, newName);
   }

   /// <summary>
   /// The notes, of every type, that match <paramref name="search"/>, found by checking every note in memory.<br/>
   /// They come in no particular order: whoever shows them sorts them.
   /// </summary>
   public List<JPNote> NotesMatching(NoteCondition search) => [..AllNotes().Where(search.Matches)];

   /// <summary>The tag paths in use in the collection right now, for the tag condition's editor to offer. See <see cref="Search.TagProperties.TagPathsInUse"/>.</summary>
   public TagPathsInUse TagPathsInUse() => new(AllNotes());

   /// <summary>Every tag some note in the collection has, each once. Unlike <see cref="TagPathsInUse"/>, it has no parent paths that aren't tags themselves.</summary>
   public IReadOnlyList<Tag> TagsInUse() => [..AllNotes().SelectMany(it => it.AllTags).Distinct()];

   /// <summary>
   /// The notes, of every type, with a tag in <paramref name="branch"/>, which renaming or deleting the branch changes: of their own, or below <c>anki::</c>,<br/>
   /// of their study-notes in Anki, where renaming or deleting changes every Anki note with one (<see cref="AnkiTagChanges"/>).
   /// </summary>
   public List<JPNote> NotesWithATagIn(TagBranch branch) => [..AllNotes().Where(note => note.AllTags.Any(branch.Contains))];

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
      NotifyThatTheStudyNoteChangedFor(note);
   }

   /// <summary>
   /// Reads from Anki again what this collection holds of each note's study-note, as the load does, without loading the corpus again: its Anki note id,<br/>
   /// the studying statuses of its cards and its tags (<see cref="IBackendDataLoader"/>). Each note whose study-note differs from before is notified<br/>
   /// through <see cref="StudyNoteChanged"/>. For when Anki may have changed the study-notes, such as after an Anki operation that changed notes, undo<br/>
   /// and redo included. It shows no progress.
   /// </summary>
   /// <remarks>
   /// It shows no progress since it runs in the background so often, after every Anki operation that changed notes: Anki's editor saves its note each<br/>
   /// time typing pauses, and each save is such an operation. A progress window would also take the focus from the editor's field, and the editor saves<br/>
   /// its note when the field loses the focus: each read would then start the next one.
   /// </remarks>
   public void ReadTheStudyNotesFromAnkiAgain() => _oneLoadOrReadFromAnkiAtATime.Do(() =>
   {
      using var showingNoProgress = NoteServices.TaskRunner.Current("Reading the study-notes from Anki again", forceHide: true);
      var ankiNoteIdsHeldWhenTheReadBegan = NoteServices.ExternalNoteIdMap.Snapshot();
      GiveEachNoteItsStudyNoteFrom(_backendDataLoader.Load(NoteServices.TaskRunner), ankiNoteIdsHeldWhenTheReadBegan).ForEach(NotifyThatTheStudyNoteChangedFor);
   });

   public JPCollection(
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

      Vocab = new VocabCollection(NoteServices);
      Kanji = new KanjiCollection(NoteServices);
      Sentences = new SentenceCollection(NoteServices);

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

   /// <summary>Keeps a load of this collection and a read of the study-notes from Anki from running at the same time, which would mix what they read.</summary>
   readonly OneAtATime _oneLoadOrReadFromAnkiAtATime = new();

   /// <summary>
   /// Permanently deletes <paramref name="note"/> from JAStudio: its corpus data, the note in this collection, and its media files in the media layer<br/>
   /// that no other note uses. Their copies in Anki's media folder are left to Anki's "Check Media". Its Anki note, if it has one in this Anki profile,<br/>
   /// is kept or deleted as <paramref name="itsAnkiNote"/> says. A deleted vocab leaves its web of perfect synonyms, whose other vocab keep it as<br/>
   /// a synonym (<see cref="Vocabulary.RelatedVocab.PerfectSynonyms.LeaveTheWebAsTheVocabIsDeleted"/>). Deleting a note in Anki instead only forgets<br/>
   /// its Anki note (<see cref="NoteCacheBase{TNote}.ExternalNoteRemoved"/>).
   /// </summary>
   public void Delete(JPNote note, ItsAnkiNote itsAnkiNote)
   {
      TakeItOutOfItsWebOfPerfectSynonyms();
      if(itsAnkiNote == ItsAnkiNote.IsDeleted) NoteServices.CardOperations.DeleteNote(note.GetId());
      _repository.Delete(note.GetId());
      RemoveFromItsCache();
      DeleteTheMediaNoOtherNoteUses();
      _noteDeleted.OnNext(note);
      return;

      void TakeItOutOfItsWebOfPerfectSynonyms()
      {
         if(note is VocabNote vocab) vocab.RelatedNotes.PerfectSynonyms.LeaveTheWebAsTheVocabIsDeleted();
      }

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

   /// <summary>Empties this collection, and forgets every note's Anki note id, as a load starts.</summary>
   void ClearCaches() => this.Log().Warning().MethodExecutionTime(() =>
   {
      IsInitialized = false;
      NoteServices.ExternalNoteIdMap.Clear();
      Vocab.Cache.Clear();
      Kanji.Cache.Clear();
      Sentences.Cache.Clear();
   });

   /// <summary>
   /// Clears this collection and loads it again: the notes from the corpus checkout, and the studying statuses and Anki note ids from Anki. It runs<br/>
   /// at startup, and when asked to, by the workspace's Local Actions > "Reload the collection from disk" or JAStudio's MCP server, to take in changes<br/>
   /// made to the corpus checkout outside JAStudio. After a sync, or as another Anki profile opens, only the study-notes are read again<br/>
   /// (<see cref="ReadTheStudyNotesFromAnkiAgain"/>), since Anki holds only them.
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
   /// didn't hold before aren't among them: none at startup.
   /// </returns>
   public IReadOnlyList<JPNote> ReloadFromBackendAfter(Action changeToTheCorpusCheckout) => _oneLoadOrReadFromAnkiAtATime.Do(() =>
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
   });

   void LoadFromBackend()
   {
      using var runner = NoteServices.TaskRunner.Current("=== Populating JAStudio data from file system ===");

      runner.RunIndeterminate("Loading the shared-conditions", NoteServices.SharedConditions.Load); // before the notes: a vocab's match-condition uses them by name

      var repoLoad = TaskCE.Run(LoadFromRepository);
      var mediaIndexBuild = TaskCE.Run(() => _mediaFileIndex.Build());
      var dictionaryLoad = TaskCE.Run(() => runner.RunIndeterminate("Loading the dictionary", DictLookup.LoadTheDictionary));

      var ankiNoteIdsHeldWhenTheReadBegan = NoteServices.ExternalNoteIdMap.Snapshot();
      var backendDataTask = Task.Run(() => _backendDataLoader.Load(NoteServices.TaskRunner));

      Task.WaitAll(repoLoad, mediaIndexBuild, backendDataTask, dictionaryLoad);

      runner.RunIndeterminate("Giving each note what Anki holds of its study-note", () => GiveEachNoteItsStudyNoteFrom(backendDataTask.Result, ankiNoteIdsHeldWhenTheReadBegan));

      IsInitialized = true;
      _reloadedFromBackend.OnNext(this);
   }

   /// <summary>
   /// Gives each note what <paramref name="anki"/> holds of its study-note: its Anki note id, the studying statuses of its cards and its tags. A study-note<br/>
   /// whose note the collection hasn't, such as one added in Anki, has no note to give them to. A note whose study-note was among<br/>
   /// <paramref name="ankiNoteIdsHeldWhenTheReadBegan"/>, and is gone from Anki, is left with none.
   /// </summary>
   /// <returns>The notes whose study-note differs from before: at a load, every note with one.</returns>
   List<JPNote> GiveEachNoteItsStudyNoteFrom(BackendData anki, IReadOnlyDictionary<long, NoteId> ankiNoteIdsHeldWhenTheReadBegan)
   {
      List<JPNote> notesWhoseStudyNoteChanged = [];
      foreach(var note in NoteServices.ExternalNoteIdMap.TakeIn(anki.IdMappings, ankiNoteIdsHeldWhenTheReadBegan).Select(NoteFromNoteId).OfType<JPNote>())
      {
         note.ReadItsStudyNoteFrom([], []);
         notesWhoseStudyNoteChanged.Add(note);
      }

      foreach(var (ankiNoteId, noteId) in anki.IdMappings)
      {
         if(NoteFromNoteId(noteId) is not {} note) continue;

         var itsStudyNoteDiffers = note.ReadItsStudyNoteFrom(anki.StudyingStatusesOf(ankiNoteId), anki.TagsOf(ankiNoteId)) || ItIsANewStudyNote(ankiNoteId, noteId);
         if(itsStudyNoteDiffers) notesWhoseStudyNoteChanged.Add(note);
      }

      return notesWhoseStudyNoteChanged;

      bool ItIsANewStudyNote(long ankiNoteId, NoteId noteId) => !(ankiNoteIdsHeldWhenTheReadBegan.TryGetValue(ankiNoteId, out var heldThen) && heldThen.Equals(noteId));
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
