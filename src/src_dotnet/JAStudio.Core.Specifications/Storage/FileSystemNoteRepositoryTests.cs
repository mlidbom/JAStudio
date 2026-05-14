using System;
using System.IO;
using System.Linq;
using System.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.Storage;
using JAStudio.Core.TaskRunners;
using Xunit;

namespace JAStudio.Core.Specifications.Storage;

public class FileSystemNoteRepositoryTests : SpecificationUsingACollection, IDisposable
{
   readonly NoteSerializer _serializer;
   readonly string _tempDir;
   readonly FileSystemNoteRepository _repo;

   public FileSystemNoteRepositoryTests()
   {
      _serializer = GetService<NoteSerializer>();
      _tempDir = Path.Combine(Path.GetTempPath(), $"JAStudio_test_{Guid.NewGuid():N}");
      _repo = new FileSystemNoteRepository(_serializer, GetService<TaskRunner>(), GetService<BackgroundTaskManager>(), _tempDir);
   }

   public new void Dispose()
   {
      base.Dispose();
      if(Directory.Exists(_tempDir))
         Directory.Delete(_tempDir, recursive: true);
   }

   // --- Individual kanji note roundtrips ---

   [Fact]
   public void AllKanjiNotes_RoundtripThroughFileSystem()
   {
      var allKanji = NoteServices.Collection.Kanji.All();
      Assert.NotEmpty(allKanji);

      foreach(var kanji in allKanji)
         _repo.Save(kanji);

      var loaded = _repo.LoadAll();
      Assert.Equal(allKanji.Count, loaded.Kanji.Count);

      foreach(var kanji in allKanji)
      {
         var roundtripped = loaded.Kanji.Single(k => k.GetId() == kanji.GetId());
         Assert.Equal(_serializer.Serialize(kanji), _serializer.Serialize(roundtripped));
      }
   }

   // --- Individual vocab note roundtrips ---

   [Fact]
   public void AllVocabNotes_RoundtripThroughFileSystem()
   {
      var allVocab = NoteServices.Collection.Vocab.All();
      Assert.NotEmpty(allVocab);

      foreach(var vocab in allVocab)
         _repo.Save(vocab);

      var loaded = _repo.LoadAll();
      Assert.Equal(allVocab.Count, loaded.Vocab.Count);

      foreach(var vocab in allVocab)
      {
         var roundtripped = loaded.Vocab.Single(v => v.GetId() == vocab.GetId());
         Assert.Equal(_serializer.Serialize(vocab), _serializer.Serialize(roundtripped));
      }
   }

   // --- Individual sentence note roundtrips ---

   [Fact]
   public void AllSentenceNotes_RoundtripThroughFileSystem()
   {
      var allSentences = NoteServices.Collection.Sentences.All();
      Assert.NotEmpty(allSentences);

      foreach(var sentence in allSentences)
         _repo.Save(sentence);

      var loaded = _repo.LoadAll();
      Assert.Equal(allSentences.Count, loaded.Sentences.Count);

      foreach(var sentence in allSentences)
      {
         var roundtripped = loaded.Sentences.Single(s => s.GetId() == sentence.GetId());
         Assert.Equal(_serializer.Serialize(sentence), _serializer.Serialize(roundtripped));
      }
   }

   // --- AllNotesData roundtrip through serializer ---

   [Fact]
   public void AllNotesData_RoundtripThroughSerializer()
   {
      var allData = BuildAllNotesData();

      var json = _serializer.Serialize(allData);
      var roundtripped = _serializer.DeserializeAll(json);

      AssertAllNotesDataMatch(allData, roundtripped);
   }

   // --- AllNotesData save/load individual files ---

   [Fact]
   public void SaveAll_And_LoadAll_PreservesAllFields()
   {
      var allData = BuildAllNotesData();

      _repo.SaveAll(allData);
      var loaded = _repo.LoadAll();

      AssertAllNotesDataMatch(allData, loaded);
   }

   [Fact]
   public void SaveAll_And_LoadAll_PreservesNoteCounts()
   {
      var allData = BuildAllNotesData();

      _repo.SaveAll(allData);
      var loaded = _repo.LoadAll();

      Assert.Equal(allData.Kanji.Count, loaded.Kanji.Count);
      Assert.Equal(allData.Vocab.Count, loaded.Vocab.Count);
      Assert.Equal(allData.Sentences.Count, loaded.Sentences.Count);
   }

   // --- Snapshot save/load ---

   [Fact]
   public void SaveSnapshot_And_LoadSnapshot_PreservesAllFields()
   {
      var allData = BuildAllNotesData();

      _repo.SaveSnapshot(allData);
      var loaded = _repo.LoadSnapshot();

      AssertAllNotesDataMatch(allData, loaded);
   }

   [Fact]
   public void SaveSnapshot_And_LoadSnapshot_ProducesIdenticalJson()
   {
      var allData = BuildAllNotesData();

      _repo.SaveSnapshot(allData);
      var loaded = _repo.LoadSnapshot();

      var originalJson = _serializer.Serialize(allData);
      var loadedJson = _serializer.Serialize(loaded);

      Assert.Equal(originalJson, loadedJson);
   }

   // --- Snapshot-aware incremental loading ---

   [Fact]
   public void LoadAll_WithSnapshot_MergesChangedFiles()
   {
      var allData = BuildAllNotesData();
      _repo.SaveAll(allData);
      _repo.SaveSnapshot(allData);

      // Modify one vocab note's JSON file (touch it newer than the snapshot)
      var vocab = allData.Vocab.First();
      Thread.Sleep(50); // ensure file timestamp is after snapshot
      _repo.Save(vocab);

      var loaded = _repo.LoadAll();
      Assert.Equal(allData.Vocab.Count, loaded.Vocab.Count);
   }

   [Fact]
   public void LoadAll_WithSnapshot_TracksDeletes()
   {
      var allData = BuildAllNotesData();
      _repo.SaveAll(allData);
      _repo.SaveSnapshot(allData);

      // Delete one kanji note's JSON file
      var kanjiToDelete = allData.Kanji.First();
      var kanjiDir = Path.Combine(_tempDir, "kanji");
      var bucket = kanjiToDelete.GetId().Value.ToString("N")[..2];
      var filePath = Path.Combine(kanjiDir, bucket, $"{kanjiToDelete.GetId().Value}.json");
      File.Delete(filePath);

      var loaded = _repo.LoadAll();
      Assert.Equal(allData.Kanji.Count - 1, loaded.Kanji.Count);
      Assert.DoesNotContain(loaded.Kanji, k => k.GetId() == kanjiToDelete.GetId());
   }

   [Fact]
   public void LoadAll_WithoutSnapshot_FallsBackToJsonAndCreatesSnapshot()
   {
      var allData = BuildAllNotesData();
      _repo.SaveAll(allData);

      var loaded = _repo.LoadAll();
      AssertAllNotesDataMatch(allData, loaded);

      // Per-type snapshots should have been created
      Assert.True(File.Exists(Path.Combine(_tempDir, "kanji", "snapshot.bin")));
      Assert.True(File.Exists(Path.Combine(_tempDir, "vocab", "snapshot.bin")));
      Assert.True(File.Exists(Path.Combine(_tempDir, "sentences", "snapshot.bin")));
   }

   // --- Helpers ---

   AllNotesData BuildAllNotesData() =>
      new(NoteServices.Collection.Kanji.All(),
          NoteServices.Collection.Vocab.All(),
          NoteServices.Collection.Sentences.All());

   void AssertAllNotesDataMatch(AllNotesData original, AllNotesData roundtripped)
   {
      Assert.Equal(original.Kanji.Count, roundtripped.Kanji.Count);
      Assert.Equal(original.Vocab.Count, roundtripped.Vocab.Count);
      Assert.Equal(original.Sentences.Count, roundtripped.Sentences.Count);

      for(var i = 0; i < original.Kanji.Count; i++)
         Assert.Equal(_serializer.Serialize(original.Kanji[i]), _serializer.Serialize(roundtripped.Kanji[i]));

      for(var i = 0; i < original.Vocab.Count; i++)
         Assert.Equal(_serializer.Serialize(original.Vocab[i]), _serializer.Serialize(roundtripped.Vocab[i]));

      for(var i = 0; i < original.Sentences.Count; i++)
         Assert.Equal(_serializer.Serialize(original.Sentences[i]), _serializer.Serialize(roundtripped.Sentences[i]));
   }
}
