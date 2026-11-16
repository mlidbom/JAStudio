using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.Teventive.StateStore.Storage;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Core.Storage.Dto;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Storage;

/// <summary>
/// <see cref="CorpusNoteFiles"/>, the notes' files in a corpus checkout as a state store's storage, writes a note's file under a temporary name when<br/>
/// the unit of work prepares, and gives it its name only when it commits: until then, and when it rolls back, the file is as it was.
/// </summary>
public class When_a_state_store_keeps_the_notes_in_the_corpus_note_files : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _corpusCheckout = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"JAStudio_corpus_note_files_{Guid.NewGuid():N}"));
   protected readonly CorpusNoteFiles NoteFiles;
   protected readonly ITaggregateReference<VocabNote> VocabReference;
   protected VocabNote Vocab => VocabReference.Current;
   protected readonly ITaggregateReference<KanjiNote> KanjiReference;
   protected KanjiNote Kanji => KanjiReference.Current;
   protected readonly ITaggregateReference<SentenceNote> SentenceReference;
   protected SentenceNote Sentence => SentenceReference.Current;

   public When_a_state_store_keeps_the_notes_in_the_corpus_note_files()
   {
      NoteFiles = new CorpusNoteFiles(_corpusCheckout.FullName);
      VocabReference = CreateVocab("走る", "to run", "はしる").Reference();
      KanjiReference = CreateKanji("走", "run", "ソウ", "はし.る").Reference();
      SentenceReference = CreateSentence("家に帰る").Reference();
      NoteFiles.Prepare([SnapshotOf(Vocab), SnapshotOf(Kanji), SnapshotOf(Sentence)], []).Commit();
   }

   public new void Dispose()
   {
      base.Dispose();
      _corpusCheckout.Delete(recursive: true);
   }

   protected static TaggregateSnapshot SnapshotOf(VocabNote vocab) => new(typeof(VocabNote), vocab.Id, vocab.TakeSnapshot());
   static TaggregateSnapshot SnapshotOf(KanjiNote kanji) => new(typeof(KanjiNote), kanji.Id, kanji.TakeSnapshot());
   static TaggregateSnapshot SnapshotOf(SentenceNote sentence) => new(typeof(SentenceNote), sentence.Id, sentence.TakeSnapshot());

   protected string TheFileOf(JPNote note) => File.ReadAllText(TheFilesOf(note).Single().FullName);

   protected IReadOnlyList<FileInfo> TheFilesOf(JPNote note) => _corpusCheckout.GetFiles($"{note.GetId().Value}.json", SearchOption.AllDirectories);

   /// <summary>The vocab, if any, as loading every note from the files gives it.</summary>
   protected VocabDto? TheVocabLoaded() =>
      NoteFiles.LoadAll().Where(it => it.TaggregateClass == typeof(VocabNote)).Select(it => (VocabDto)it.Snapshot).SingleOrDefault();

   /// <summary>The snapshot cache in each folder of notes, from which <see cref="CorpusNoteFiles.LoadAll"/> loads what no file has changed since.</summary>
   protected IReadOnlyList<FileInfo> TheSnapshotCaches => [.. new[] { "kanji", "vocab", "sentences" }.Select(it => new FileInfo(Path.Combine(_corpusCheckout.FullName, it, "snapshot.bin")))];

   protected IReadOnlyList<string> TheTemporaryFiles => [.. _corpusCheckout.GetFiles("*.being-*", SearchOption.AllDirectories).Select(it => it.Name)];

   public class loading_every_note : When_a_state_store_keeps_the_notes_in_the_corpus_note_files
   {
      readonly IReadOnlyList<TaggregateSnapshot> _loaded;

      public loading_every_note() => _loaded = new CorpusNoteFiles(_corpusCheckout.FullName).LoadAll();

      [XF] public void gives_a_snapshot_of_each_note_of_its_class_with_its_id() =>
         _loaded.Select(it => $"{it.TaggregateClass.Name} {it.Id.Value}").Order().Must().SequenceEqual(
            new[] { $"{nameof(VocabNote)} {Vocab.GetId().Value}", $"{nameof(KanjiNote)} {Kanji.GetId().Value}", $"{nameof(SentenceNote)} {Sentence.GetId().Value}" }.Order());

      [XF] public void the_vocabs_snapshot_is_its_DTO() =>
         _loaded.Single(it => it.TaggregateClass == typeof(VocabNote)).Snapshot.Must().Satisfy(it => it is VocabDto { CorpusAnswer: "to run" });
   }

   public class when_a_changed_vocab_is_prepared_to_be_saved : When_a_state_store_keeps_the_notes_in_the_corpus_note_files
   {
      readonly string _theFileBefore;
      readonly IPreparedWrite _write;

      public when_a_changed_vocab_is_prepared_to_be_saved()
      {
         _theFileBefore = TheFileOf(Vocab);
         Vocab.Edit(it => it.SetCorpusAnswer("to dash"));
         _write = NoteFiles.Prepare([SnapshotOf(Vocab)], []);
      }

      [XF] public void its_file_is_still_as_it_was() => TheFileOf(Vocab).Must().Be(_theFileBefore);
      [XF] public void its_new_file_waits_under_a_temporary_name() => TheTemporaryFiles.Must().SequenceEqual([$"{Vocab.GetId().Value}.being-written"]);

      public class and_the_write_is_committed : when_a_changed_vocab_is_prepared_to_be_saved
      {
         public and_the_write_is_committed() => _write.Commit();

         [XF] public void its_file_holds_the_change() => TheFileOf(Vocab).Must().Contain("to dash");
         [XF] public void no_temporary_file_remains() => TheTemporaryFiles.Must().BeEmpty();
      }

      public class and_the_write_is_discarded : when_a_changed_vocab_is_prepared_to_be_saved
      {
         public and_the_write_is_discarded() => _write.Discard();

         [XF] public void its_file_is_as_it_was() => TheFileOf(Vocab).Must().Be(_theFileBefore);
         [XF] public void no_temporary_file_remains() => TheTemporaryFiles.Must().BeEmpty();
      }
   }

   public class when_the_vocab_is_prepared_to_be_deleted : When_a_state_store_keeps_the_notes_in_the_corpus_note_files
   {
      readonly IPreparedWrite _write;

      public when_the_vocab_is_prepared_to_be_deleted() => _write = NoteFiles.Prepare([], [new TaggregateToDelete(typeof(VocabNote), Vocab.Id)]);

      [XF] public void its_file_is_still_there() => TheFilesOf(Vocab).Must().HaveCount(1);

      public class and_the_write_is_committed : when_the_vocab_is_prepared_to_be_deleted
      {
         public and_the_write_is_committed() => _write.Commit();

         [XF] public void its_file_is_gone() => TheFilesOf(Vocab).Must().BeEmpty();
         [XF] public void loading_every_note_leaves_it_out() => TheVocabLoaded().Must().BeNull();
      }

      public class and_the_write_is_discarded : when_the_vocab_is_prepared_to_be_deleted
      {
         public and_the_write_is_discarded() => _write.Discard();

         [XF] public void its_file_remains() => TheFilesOf(Vocab).Must().HaveCount(1);
      }
   }

   /// <summary>
   /// Loading every note writes a snapshot cache in each folder of notes, which the next load reads instead of the files, except for the files<br/>
   /// changed since it was written. Here it was written an hour ago, so a file changed now is newer than it, however coarse the file system's clock.
   /// </summary>
   public class once_every_note_was_loaded_an_hour_ago : When_a_state_store_keeps_the_notes_in_the_corpus_note_files
   {
      public once_every_note_was_loaded_an_hour_ago()
      {
         NoteFiles.LoadAll();
         foreach(var it in TheSnapshotCaches) it.LastWriteTimeUtc = DateTime.UtcNow.AddHours(-1);
      }

      [XF] public void each_folder_of_notes_holds_its_snapshot_cache() => TheSnapshotCaches.Where(it => !it.Exists).Must().BeEmpty();

      public class and_the_vocabs_file_is_changed : once_every_note_was_loaded_an_hour_ago
      {
         public and_the_vocabs_file_is_changed()
         {
            Vocab.Edit(it => it.SetCorpusAnswer("to dash"));
            NoteFiles.Prepare([SnapshotOf(Vocab)], []).Commit();
         }

         [XF] public void loading_every_note_gives_the_vocab_as_its_file_now_holds_it() => TheVocabLoaded().Must().Satisfy(it => it is { CorpusAnswer: "to dash" });
      }

      /// <summary>As git deletes it, checking out a commit the vocab isn't in.</summary>
      public class and_the_vocabs_file_is_deleted : once_every_note_was_loaded_an_hour_ago
      {
         public and_the_vocabs_file_is_deleted() => TheFilesOf(Vocab).Single().Delete();

         [XF] public void loading_every_note_leaves_the_vocab_out() => TheVocabLoaded().Must().BeNull();
      }
   }
}
