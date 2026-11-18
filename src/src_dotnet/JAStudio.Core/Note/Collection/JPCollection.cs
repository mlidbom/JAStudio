using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Internals.Logging;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using Compze.Abstractions;
using Compze.Tessaging.TessageBus;
using Compze.Teventive.MemoryImageStateStore;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.TagProperties;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
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

   /// <summary>
   /// Why the collection has no notes to use yet, or null once a load of them has succeeded (<see cref="ReloadFromBackend"/>): JAStudio hasn't<br/>
   /// loaded it yet, or every load so far failed, as the last one's exception says. JAStudio's MCP server answers its tools that read the notes<br/>
   /// with it meanwhile.
   /// </summary>
   public string? WhyItHasNoNotesToUseYet { get; private set; } = "JAStudio hasn't loaded the collection yet.";

   readonly NoteUnitsOfWork _unitsOfWork;

   /// <summary>
   /// Runs <paramref name="work"/> in a unit of work in which notes are changed: the one running on this thread, or else a new one, which commits<br/>
   /// once it is done. Every change to a note runs in one, as <c>note.Edit(it => ...)</c> does (<see cref="EditingANote"/>). What follows changes<br/>
   /// to notes observes the tevents the notes published in it, each unit of work's together, once it has committed<br/>
   /// (<see cref="IDynamicTeventObservers"/>).
   /// </summary>
   public void InAUnitOfWork(Action<NoteUnitOfWork> work) => _unitsOfWork.Run(work);

   /// <summary>The unit of work running on this thread (<see cref="InAUnitOfWork"/>). Fails when none is.</summary>
   internal NoteUnitOfWork TheRunningUnitOfWork => _unitsOfWork.Running;

   /// <summary>
   /// Publishes that what the collection holds of <paramref name="note"/>'s study-note in the spaced repetition system changed<br/>
   /// (<see cref="IStudyNoteChanged"/>): its studying status, when JAStudio suspended or unsuspended its cards, or whatever reading the study-notes<br/>
   /// again finds changed (<see cref="ReadTheStudyNotesFromTheSRSAgain"/>), as a sync does: the studying status of a card, its<br/>
   /// <see cref="JPNote.AnkiTags"/>, or its study-note itself, created or deleted.
   /// </summary>
   /// <remarks>
   /// None of it is stored with the note, so no unit of work changes the note for it. A reload sets every note's without publishing it for each:<br/>
   /// <see cref="ICollectionReloaded"/> tells of the reload.
   /// </remarks>
   internal void PublishThatTheStudyNoteChangedFor(JPNote note) => _tevents.Value.Publish(new StudyNoteChanged(note.GetId()));

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
   /// loading the corpus again: its id, the studying statuses of its cards and its tags (<see cref="ISRSStudyNotes.ReadAll"/>). For each note whose study-note<br/>
   /// differs from before, an <see cref="IStudyNoteChanged"/> is published. For when the study-notes may differ from what this collection holds<br/>
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

   /// <summary>Reads the study-notes again, gives each note its study-note from them, and publishes the change for each note whose study-note differs from before or is gone.</summary>
   StudyNotesReadFromTheSRS ReadTheStudyNotesAgain()
   {
      var studyNoteIdsHeldWhenTheReadBegan = NoteServices.StudyNoteIds.Snapshot();
      var read = GiveEachNoteItsStudyNoteFrom(_studyNotesInTheSRS.ReadAll(), studyNoteIdsHeldWhenTheReadBegan);
      foreach(var note in read.NotesWhoseStudyNoteIsGone.Concat(read.NotesWhoseStudyNoteChanged))
         PublishThatTheStudyNoteChangedFor(note);
      return read;
   }

   /// <remarks>
   /// The memory image state store that keeps the notes (<see cref="IMemoryImageStateStore"/>), and its storage, are resolved at their first<br/>
   /// use: a host may only know where<br/>
   /// the corpus is once it has started.
   /// </remarks>
   public JPCollection(
      NoteServices noteServices,
      IRootResolver resolver,
      MediaFileIndex mediaFileIndex,
      MediaStorageService mediaStorage,
      ISRSStudyNotes studyNotesInTheSRS)
   {
      using var _ = this.Log().Info().MethodExecutionTime();

      NoteServices = noteServices;
      _store = new Lazy<IMemoryImageStateStore>(resolver.Resolve<IMemoryImageStateStore>);
      _tevents = new Lazy<IIndependentTeventPublisher>(resolver.Resolve<IIndependentTeventPublisher>);
      _mediaFileIndex = mediaFileIndex;
      _mediaStorage = mediaStorage;
      _studyNotesInTheSRS = studyNotesInTheSRS;
      _unitsOfWork = new NoteUnitsOfWork(resolver);
      DictLookup = new DictLookup(this);
      VocabNoteGeneratedData = new VocabNoteGeneratedData();
      VocabNoteFactory = new VocabNoteFactory(DictLookup, this, noteServices);

      Vocab = new VocabCollection(NoteServices, resolver, _unitsOfWork);
      Kanji = new KanjiCollection(NoteServices, resolver, _unitsOfWork);
      Sentences = new SentenceCollection(NoteServices, resolver, _unitsOfWork);
   }

   readonly Lazy<IMemoryImageStateStore> _store;
   readonly Lazy<IIndependentTeventPublisher> _tevents;
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
   /// (<see cref="Vocabulary.Tevents.IVocabTevent.Deleted"/>). Deleting a note's study-note in Anki instead leaves<br/>
   /// the note in the corpus and in JAStudio: the next read of the study-notes finds it gone, and the note is then no longer studied.
   /// </summary>
   /// <remarks>
   /// The note is deleted as it is when the deletion begins, whatever instance of it <paramref name="note"/> is, in a unit of work, which deletes its<br/>
   /// study-note and its media files once it has committed.
   /// </remarks>
   public void Delete(JPNote note, ItsStudyNote itsStudyNote) => InAUnitOfWork(unitOfWork =>
   {
      var theNoteAsItIs = unitOfWork.ForUpdate(note);
      theNoteAsItIs.Delete();
      unitOfWork.OnceCommitted(() =>
      {
         if(itsStudyNote == ItsStudyNote.IsDeleted) _studyNotesInTheSRS.DeleteTheStudyNoteOf(theNoteAsItIs.GetId());
         DeleteTheMediaFilesNoNoteUses(theNoteAsItIs.MediaReferences.Select(it => it.FileName));
      });
   });

   /// <summary>
   /// Discards <paramref name="note"/>'s TTS audio, so that it has none (<see cref="Media.AudioReferenceList.DiscardTheTextToSpeechAudio"/>), and deletes<br/>
   /// its files from the media layer, unless another note uses them, as a file HyperTTS made may be shared by notes with the same text. Their copies<br/>
   /// in Anki's media folder are left to Anki's "Check Media", as a deleted note's are. Its recordings stay.
   /// </summary>
   public void DiscardTheTextToSpeechAudioOf(IHasTextToSpeechAudio note)
   {
      List<string> itsFiles = [];
      note.Edit(it =>
      {
         itsFiles = [.. it.Audio.TextToSpeech.Select(audio => audio.FileName)];
         it.Audio.DiscardTheTextToSpeechAudio();
      });
      DeleteTheMediaFilesNoNoteUses(itsFiles);
   }

   /// <summary>Deletes those of <paramref name="fileNames"/> that no note in the collection uses, and that the media layer has, from the media layer.</summary>
   void DeleteTheMediaFilesNoNoteUses(IEnumerable<string> fileNames)
   {
      var candidates = fileNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
      if(candidates.Count == 0) return;

      var filesNotesUse = AllNotes().SelectMany(it => it.MediaReferences).Select(it => it.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
      candidates.Where(it => !filesNotesUse.Contains(it) && _mediaFileIndex.ContainsByOriginalFileName(it))
                .ToList()
                .ForEach(_mediaStorage.DeleteFile);
   }

   /// <summary>Forgets every note's Anki note id, as a load starts.</summary>
   void ForgetTheStudyNoteIds()
   {
      IsInitialized = false;
      NoteServices.StudyNoteIds.Clear();
   }

   /// <summary>
   /// Loads this collection again: the notes from the corpus checkout, and the studying statuses and Anki note ids from Anki. It runs at startup,<br/>
   /// and when asked to, by the workspace's Local Actions > "Reload the collection from disk" or JAStudio's MCP server, to take in changes made to<br/>
   /// the corpus checkout outside JAStudio. After a sync, or as another Anki profile opens, only the study-notes are read again<br/>
   /// (<see cref="ReadTheStudyNotesFromTheSRSAgain"/>), since Anki holds only them.
   /// </summary>
   /// <returns>The notes whose files changed on disk while this collection held them, as <see cref="ReloadFromBackendAfter"/> describes.</returns>
   public IReadOnlyList<JPNote> ReloadFromBackend() => ReloadFromBackendAfter(() => {});

   /// <summary>
   /// Writes every note's file anew, as JAStudio stores the note now, changed or not, which JAStudio otherwise never does: a unit of work writes only<br/>
   /// the files of the notes it changed. To see what loading and saving does to the notes' files, as after a change to how they're stored.
   /// </summary>
   /// <remarks>
   /// One commit of the memory image state store (<see cref="IMemoryImageStateStore.WriteAllToStorage"/>), while units of work wait, as at a<br/>
   /// reload. It changes no note, and<br/>
   /// the next start reads every note's file once, since every file changed.
   /// </remarks>
   public void WriteEveryNoteAnew() => _store.Value.WriteAllToStorage();

   /// <summary>
   /// Makes <paramref name="changeToTheCorpusCheckout"/>, such as a git fast-forward of the corpus checkout, while no unit of work runs, so that<br/>
   /// nothing JAStudio does meanwhile saves a note over the change, then loads this collection again, as <see cref="ReloadFromBackend"/> does.<br/>
   /// When the change fails, the notes are left as they were, and the study-notes are read again.
   /// </summary>
   /// <returns>
   /// The notes whose files changed on disk while this collection held them: each one's data now differs from the same note's as held before.<br/>
   /// Notes this collection didn't hold before aren't among them: none at startup. The reload is published, with the notes it added, changed and<br/>
   /// removed (<see cref="ICollectionReloaded"/>), so that what follows changes catches up, such as the workspace's pages, unless the change fails.
   /// </returns>
   public IReadOnlyList<JPNote> ReloadFromBackendAfter(Action changeToTheCorpusCheckout) => _oneLoadOrReadFromTheSRSAtATime.Do(() =>
   {
      ForgetTheStudyNoteIds();
      var reloaded = LoadFromBackendTellingWhyItFailedIfNoLoadHasSucceeded(changeToTheCorpusCheckout);
      WhyItHasNoNotesToUseYet = null;

      List<JPNote> notesChangedOnDisk = [.. reloaded.Changed.Select(it => NoteFromNoteId((NoteId)it)).OfType<JPNote>()];
      _tevents.Value.Publish(new CollectionReloaded(NoteIds(reloaded.Added), NoteIds(reloaded.Changed), NoteIds(reloaded.Removed)));
      return notesChangedOnDisk;

      static IReadOnlyCollection<NoteId> NoteIds(IReadOnlyCollection<TaggregateId> taggregateIds) => [.. taggregateIds.Cast<NoteId>()];
   });

   /// <summary>
   /// Loads the notes again after <paramref name="changeToTheCorpusCheckout"/> (<see cref="LoadFromBackendAfter"/>). While no load has succeeded yet,<br/>
   /// a load that fails leaves its exception as <see cref="WhyItHasNoNotesToUseYet"/>, and goes on failing with it. Once one has, the notes stay usable.
   /// </summary>
   ChangesFoundInStorage LoadFromBackendTellingWhyItFailedIfNoLoadHasSucceeded(Action changeToTheCorpusCheckout)
   {
      try
      {
         return LoadFromBackendAfter(changeToTheCorpusCheckout);
      }
      catch(Exception exception) when(WhyItHasNoNotesToUseYet != null)
      {
         WhyItHasNoNotesToUseYet = $"JAStudio's load of the collection failed, and none has succeeded yet: {exception}";
         throw;
      }
   }

   /// <summary>
   /// Loads the notes again after <paramref name="changeToTheCorpusCheckout"/>, the shared-conditions first, since a vocab's match-condition uses them by<br/>
   /// name; and meanwhile the dictionary, the media layer's index and the study-notes. The memory image state store keeps every unit of work<br/>
   /// waiting until the<br/>
   /// notes are loaded.
   /// </summary>
   ChangesFoundInStorage LoadFromBackendAfter(Action changeToTheCorpusCheckout)
   {
      using var runner = NoteServices.TaskRunner.Current("=== Populating JAStudio data from file system ===");

      var dictionaryLoad = TaskCE.Run(() => runner.RunIndeterminate("Loading the dictionary", DictLookup.LoadTheDictionary));
      var studyNoteIdsHeldWhenTheReadBegan = NoteServices.StudyNoteIds.Snapshot();
      var studyNotesRead = TaskCE.Run(() => runner.RunIndeterminate("Reading the study-notes from Anki", _studyNotesInTheSRS.ReadAll));
      Task? mediaIndexBuild = null;
      try
      {
         return runner.RunIndeterminate("Loading the notes", () => _store.Value.LoadFromStorage(() =>
         {
            changeToTheCorpusCheckout();
            mediaIndexBuild = TaskCE.Run(() => _mediaFileIndex.Build());
            runner.RunIndeterminate("Loading the shared-conditions", NoteServices.SharedConditions.Load);
         }));
      }
      finally
      {
         Task.WaitAll(mediaIndexBuild ?? TaskCE.Run(() => _mediaFileIndex.Build()), studyNotesRead, dictionaryLoad);

         runner.RunIndeterminate("Giving each note what Anki holds of its study-note", () => GiveEachNoteItsStudyNoteFrom(studyNotesRead.Result, studyNoteIdsHeldWhenTheReadBegan));

         IsInitialized = true;
      }
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
      notesWhoseStudyNoteThenIsGone.ForEach(it => NoteServices.WhatTheSRSHoldsOfTheStudyNotes.Forget(it.GetId()));

      List<JPNote> notesWhoseStudyNoteChanged = [];
      foreach(var studyNote in studyNotesRead)
      {
         if(studyNote.NoteId is not {} noteId || NoteFromNoteId(noteId) is not {} note) continue;

         var itsStudyNoteDiffers = NoteServices.WhatTheSRSHoldsOfTheStudyNotes.TakeIn(noteId, studyNote.CardStudyingStatuses, studyNote.Tags) || ItIsANewStudyNote(studyNote.StudyNoteId, noteId);
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

}
