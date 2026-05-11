using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Storage;

public class FileSystemNoteRepository : INoteRepository
{
   readonly NoteServices _noteServices;
   readonly JsonFilesystemObjectRepository<KanjiData> _kanjiRepository;
   readonly JsonFilesystemObjectRepository<VocabData> _vocabRepository;
   readonly JsonFilesystemObjectRepository<SentenceData> _sentenceRepository;

   public FileSystemNoteRepository(NoteSerializer serializer, TaskRunner taskRunner, BackgroundTaskManager backgroundTaskManager, IEnvironmentPaths paths)
      : this(serializer, taskRunner, backgroundTaskManager, paths.DatabaseDir) {}

   public FileSystemNoteRepository(NoteSerializer serializer, TaskRunner taskRunner, BackgroundTaskManager backgroundTaskManager, string rootDir)
   {
      _noteServices = serializer.NoteServices;
      _kanjiRepository = new JsonFilesystemObjectRepository<KanjiData>(Path.Combine(rootDir, "kanji"), NoteSerializer.JsonOptions);
      _vocabRepository = new JsonFilesystemObjectRepository<VocabData>(Path.Combine(rootDir, "vocab"), NoteSerializer.JsonOptions);
      _sentenceRepository = new JsonFilesystemObjectRepository<SentenceData>(Path.Combine(rootDir, "sentences"), NoteSerializer.JsonOptions);
   }

   public void Save(KanjiNote note) => _kanjiRepository.Save(KanjiNoteConverter.ToCorpusData(note));

   public void Save(VocabNote note) => _vocabRepository.Save(VocabNoteConverter.ToCorpusData(note));

   public void Save(SentenceNote note) => _sentenceRepository.Save(SentenceNoteConverter.ToCorpusData(note));

   public void Delete(NoteId noteId)
   {
      switch(noteId)
      {
         case VocabId: _vocabRepository.Delete(noteId.Value); break;
         case KanjiId: _kanjiRepository.Delete(noteId.Value); break;
         case SentenceId: _sentenceRepository.Delete(noteId.Value); break;
      }
   }

   public void SaveAll(AllNotesData data)
   {
      foreach(var note in data.Kanji) Save(note);
      foreach(var note in data.Vocab) Save(note);
      foreach(var note in data.Sentences) Save(note);
   }

   public AllNotesData LoadAll()
   {
      var kanjiData = _kanjiRepository.LoadAll();
      var vocabData = _vocabRepository.LoadAll();
      var sentenceData = _sentenceRepository.LoadAll();

      return ToAllNotesData(kanjiData, vocabData, sentenceData);
   }

   public void SaveSnapshot(AllNotesData data)
   {
      _kanjiRepository.SaveSnapshot(data.Kanji.Select(KanjiNoteConverter.ToCorpusData).ToList());
      _vocabRepository.SaveSnapshot(data.Vocab.Select(VocabNoteConverter.ToCorpusData).ToList());
      _sentenceRepository.SaveSnapshot(data.Sentences.Select(SentenceNoteConverter.ToCorpusData).ToList());
   }

   public AllNotesData LoadSnapshot()
   {
      var kanjiData = _kanjiRepository.LoadSnapshot();
      var vocabData = _vocabRepository.LoadSnapshot();
      var sentenceData = _sentenceRepository.LoadSnapshot();

      return ToAllNotesData(kanjiData, vocabData, sentenceData);
   }

   AllNotesData ToAllNotesData(List<KanjiData> kanjiData, List<VocabData> vocabData, List<SentenceData> sentenceData) =>
      new(kanjiData.Select(d => new KanjiNote(_noteServices, d)).ToList(),
          vocabData.Select(d => new VocabNote(_noteServices, d)).ToList(),
          sentenceData.Select(d => new SentenceNote(_noteServices, d)).ToList());
}
