using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.Teventive.StateStore.Storage;
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
   protected readonly VocabNote Vocab;
   protected readonly KanjiNote Kanji;
   protected readonly SentenceNote Sentence;

   public When_a_state_store_keeps_the_notes_in_the_corpus_note_files()
   {
      NoteFiles = new CorpusNoteFiles(_corpusCheckout.FullName);
      Vocab = CreateVocab("走る", "to run", "はしる");
      Kanji = CreateKanji("走", "run", "ソウ", "はし.る");
      Sentence = CreateSentence("家に帰る");
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

   protected string TheFileOf(JPNote note) => File.ReadAllText(_corpusCheckout.GetFiles($"{note.GetId().Value}.json", SearchOption.AllDirectories).Single().FullName);

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
         Vocab.Edit(it => it.CorpusAnswer.Set("to dash"));
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
}
