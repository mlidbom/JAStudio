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
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;
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
   /// its cards, and whatever reading the study-notes from Anki again finds changed (<see cref="ReadTheStudyNotesFromTheSRSAgain"/>), as a sync does:<br/>
   /// the studying status of a card, its <see cref="JPNote.AnkiTags"/>, or its study-note itself, created or deleted.
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

   /// <summary>The note that the study-note with the id <paramref name="studyNoteId"/> belongs to, or null when no note is known to have it.</summary>
   // ReSharper disable once UnusedMember.Global called from python
   public JPNote? NoteOfTheStudyNote(long studyNoteId)
   {
      var noteId = NoteServices.StudyNoteIds.NoteIdOfTheStudyNote(studyNoteId);
      return noteId != null ? NoteFromNoteId(noteId) : null;
   }

   public NoteServices NoteServices { get; }
   public VocabNoteFactory VocabNoteFactory { get; }
   public DictLookup DictLookup { get; }
   public VocabNoteGeneratedData VocabNoteGeneratedData { get; }

   /// <summary>
   /// Reads from the SRS, the spaced repetition system, again what this collection holds of each note's study-note, as the load does, without<br/>
   /// loading the corpus again: its id, the studying statuses of its cards and its tags (<see cref="ISRSStudyNotes.ReadAll"/>). Each note whose study-note<br/>
   /// differs from before is notified through <see cref="StudyNoteChanged"/>. For when the study-notes may differ from what this collection holds<br/>
   /// of them: after JAStudio changed Anki's tags (<see cref="AnkiTagChanges"/>), and when another Anki profile opened. A sync reads them so too<br/>
   /// (<see cref="ReadTheStudyNotesFromTheSRSAgainAndCompareThem"/>). It shows no progress.
   /// </summary>
   /// <remarks>review-naming:studying: ReadTheStudyNotesFromTheSRSAgain was ReadTheStudyNotesFromAnkiAgain (Claude, 2026-11-15).</remarks>
   public void ReadTheStudyNotesFromTheSRSAgain() => _oneLoadOrReadFromTheSRSAtATime.Do(() => { ReadTheStudyNotesAgain(); });

   /// <summary>
   /// Reads the study-notes again, as <see cref="ReadTheStudyNotesFromTheSRSAgain"/> does, and compares each with the study-note JAStudio would write<br/>
   /// for its note now, before a load can empty this collection: what a sync with the spaced repetition system starts with (<see cref="SRSSync"/>).
   /// </summary>
   internal (StudyNotesReadFromTheSRS Read, StudyNoteComparison Comparison) ReadTheStudyNotesFromTheSRSAgainAndCompareThem() => _oneLoadOrReadFromTheSRSAtATime.Do(() =>
   {
      var read = ReadTheStudyNotesAgain();
      return (read, new StudyNoteComparison(this, read.StudyNotes));
   });

   /// <summary>Reads the study-notes again, gives each note its study-note from them, and notifies each note whose study-note differs from before or is gone.</summary>
   StudyNotesReadFromTheSRS ReadTheStudyNotesAgain()
   {
      var studyNoteIdsHeldWhenTheReadBegan = NoteServices.StudyNoteIds.Snapshot();
      var read = GiveEachNoteItsStudyNoteFrom(_studyNotesInTheSRS.ReadAll(), studyNoteIdsHeldWhenTheReadBegan);
      foreach(var note in read.NotesWhoseStudyNoteIsGone.Concat(read.NotesWhoseStudyNoteChanged))
         NotifyThatTheStudyNoteChangedFor(note);
      return read;
   }

   public JPCollection(
      NoteServices noteServices,
      INoteRepository noteRepository,
      MediaFileIndex mediaFileIndex,
      MediaStorageService mediaStorage,
      ISRSStudyNotes studyNotesInTheSRS)
   {
      using var _ = this.Log().Info().MethodExecutionTime();

      NoteServices = noteServices;
      _repository = noteRepository;
      _mediaFileIndex = mediaFileIndex;
      _mediaStorage = mediaStorage;
      _studyNotesInTheSRS = studyNotesInTheSRS;
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
   readonly ISRSStudyNotes _studyNotesInTheSRS;

   /// <summary>
   /// Keeps a load of this collection and a read of the study-notes from the spaced repetition system from running at the same time, which would<br/>
   /// mix what they read.
   /// </summary>
   readonly OneAtATime _oneLoadOrReadFromTheSRSAtATime = new();

   /// <summary>
   /// Permanently deletes <paramref name="note"/> from JAStudio: its corpus data, the note in this collection, and its media files in the media layer<br/>
   /// that no other note uses. Their copies in Anki's media folder are left to Anki's "Check Media". Its study-note, if it has one, is kept or<br/>
   /// deleted as <paramref name="itsStudyNote"/> says. A deleted vocab leaves its web of perfect synonyms, whose other vocab keep it as a synonym<br/>
   /// (<see cref="Vocabulary.RelatedVocab.PerfectSynonyms.LeaveTheWebAsTheVocabIsDeleted"/>). Deleting a note's study-note in Anki instead leaves<br/>
   /// the note in the corpus and in JAStudio: the next read of the study-notes finds it gone, and the note is then no longer studied.
   /// </summary>
   public void Delete(JPNote note, ItsStudyNote itsStudyNote)
   {
      TakeItOutOfItsWebOfPerfectSynonyms();
      if(itsStudyNote == ItsStudyNote.IsDeleted) _studyNotesInTheSRS.DeleteTheStudyNoteOf(note.GetId());
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
      NoteServices.StudyNoteIds.Clear();
      Vocab.Cache.Clear();
      Kanji.Cache.Clear();
      Sentences.Cache.Clear();
   });

   /// <summary>
   /// Clears this collection and loads it again: the notes from the corpus checkout, and the studying statuses and Anki note ids from Anki. It runs<br/>
   /// at startup, and when asked to, by the workspace's Local Actions > "Reload the collection from disk" or JAStudio's MCP server, to take in changes<br/>
   /// made to the corpus checkout outside JAStudio. After a sync, or as another Anki profile opens, only the study-notes are read again<br/>
   /// (<see cref="ReadTheStudyNotesFromTheSRSAgain"/>), since Anki holds only them.
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
   /// is passed on as updated (<see cref="NoteUpdated"/>), so that what follows note updates catches up, such as the workspace's pages. Notes this<br/>
   /// collection didn't hold before aren't among them: none at startup.
   /// </returns>
   public IReadOnlyList<JPNote> ReloadFromBackendAfter(Action changeToTheCorpusCheckout) => _oneLoadOrReadFromTheSRSAtATime.Do(() =>
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

      var studyNoteIdsHeldWhenTheReadBegan = NoteServices.StudyNoteIds.Snapshot();
      var studyNotesRead = TaskCE.Run(() => runner.RunIndeterminate("Reading the study-notes from Anki", _studyNotesInTheSRS.ReadAll));

      Task.WaitAll(repoLoad, mediaIndexBuild, studyNotesRead, dictionaryLoad);

      runner.RunIndeterminate("Giving each note what Anki holds of its study-note", () => GiveEachNoteItsStudyNoteFrom(studyNotesRead.Result, studyNoteIdsHeldWhenTheReadBegan));

      IsInitialized = true;
      _reloadedFromBackend.OnNext(this);
   }

   /// <summary>
   /// Gives each note what the spaced repetition system holds of its study-note, as <paramref name="studyNotesRead"/> from it: its id, the studying<br/>
   /// statuses of its cards and its tags. A study-note that holds no note's id, or whose note the collection hasn't, such as one added in Anki,<br/>
   /// has no note to give them to. A note whose study-note was among <paramref name="studyNoteIdsHeldWhenTheReadBegan"/>, and is gone from the<br/>
   /// spaced repetition system, is left with none.
   /// </summary>
   /// <returns>What was read, with the notes whose study-note differs from before, at a load every note with one, and those whose study-note is gone.</returns>
   StudyNotesReadFromTheSRS GiveEachNoteItsStudyNoteFrom(IReadOnlyList<StudyNoteInTheSRS> studyNotesRead, IReadOnlyDictionary<long, NoteId> studyNoteIdsHeldWhenTheReadBegan)
   {
      List<JPNote> notesWhoseStudyNoteThenIsGone = [.. NoteServices.StudyNoteIds.TakeIn(NoteIdOfEachStudyNoteThatHoldsOne(), studyNoteIdsHeldWhenTheReadBegan).Select(NoteFromNoteId).OfType<JPNote>()];
      notesWhoseStudyNoteThenIsGone.ForEach(it => it.ReadItsStudyNoteFrom([], []));

      List<JPNote> notesWhoseStudyNoteChanged = [];
      foreach(var studyNote in studyNotesRead)
      {
         if(studyNote.NoteId is not {} noteId || NoteFromNoteId(noteId) is not {} note) continue;

         var itsStudyNoteDiffers = note.ReadItsStudyNoteFrom(studyNote.CardStudyingStatuses, studyNote.Tags) || ItIsANewStudyNote(studyNote.StudyNoteId, noteId);
         if(itsStudyNoteDiffers) notesWhoseStudyNoteChanged.Add(note);
      }

      return new StudyNotesReadFromTheSRS(studyNotesRead, notesWhoseStudyNoteChanged, [.. notesWhoseStudyNoteThenIsGone.Where(ItHasNoOtherStudyNoteNow)]);

      bool ItHasNoOtherStudyNoteNow(JPNote note) => !note.HasAStudyNote();

      Dictionary<long, NoteId> NoteIdOfEachStudyNoteThatHoldsOne()
      {
         Dictionary<long, NoteId> noteIdOfEachStudyNote = [];
         foreach(var studyNote in studyNotesRead)
            if(studyNote.NoteId is {} noteId) noteIdOfEachStudyNote[studyNote.StudyNoteId] = noteId;
         return noteIdOfEachStudyNote;
      }

      bool ItIsANewStudyNote(long studyNoteId, NoteId noteId) => !(studyNoteIdsHeldWhenTheReadBegan.TryGetValue(studyNoteId, out var heldThen) && heldThen.Equals(noteId));
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
