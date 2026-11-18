using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Teventive.MemoryImageStateStore;
using Compze.Teventive.MemoryImageStateStore.Testing;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases;
using JAStudio.Core.SysUtils.IO;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core.Specifications.Fixtures;

/// <summary>
/// The corpus the specifications' collection starts with, as the files of a corpus checkout: the notes' files, which the memory image state<br/>
/// store keeps, and<br/>
/// the shared-conditions' files, which the vocab's match-conditions use. Made once for each <see cref="DataNeeded"/>, by a JAStudio that creates<br/>
/// the notes, then loaded by each specification's JAStudio as JAStudio loads the corpus when it starts (<see cref="LoadInto"/>).
/// </summary>
/// <remarks>
/// The vocab are those of the parsing cases' corpus (<see cref="ParsingCasesFolder.Corpus"/>), extracted from the corpus;<br/>
/// the kanji and the sentences are made from the sample data (<see cref="KanjiSpec"/>, <see cref="SentenceSpec"/>), the sentences parsed against the<br/>
/// vocab. Each specification's JAStudio has files of its own, so the files are written into each, which takes a millisecond or so.
/// </remarks>
sealed class TheSpecificationsCorpus
{
   readonly IReadOnlyList<FileInTheCorpus> _filesOfTheNotes;
   readonly IReadOnlyList<FileInTheCorpus> _filesOnTheDisk;

   TheSpecificationsCorpus(IReadOnlyList<FileInTheCorpus> filesOfTheNotes, IReadOnlyList<FileInTheCorpus> filesOnTheDisk)
   {
      _filesOfTheNotes = filesOfTheNotes;
      _filesOnTheDisk = filesOnTheDisk;
   }

   /// <summary>The corpus with the notes <paramref name="data"/> says, made by a JAStudio of its own, which creates them.</summary>
   public static TheSpecificationsCorpus With(DataNeeded data)
   {
      using var maker = CollectionFactory.StartAnApp(NotesKept.InMemory);
      CreateTheNotes(data, maker.CoreApp);
      var corpusCheckout = new DirectoryInfo(maker.CoreApp.Services.Resolve<IEnvironmentPaths>().DatabaseDir);
      return new TheSpecificationsCorpus(TheFilesOfTheNotesIn(maker.CoreApp, corpusCheckout), TheFilesOnTheDiskIn(corpusCheckout));
   }

   /// <summary>
   /// Writes these files into <paramref name="app"/>'s corpus checkout, the notes' files into the memory image state store's files, then loads<br/>
   /// the collection<br/>
   /// again, as JAStudio loads the corpus when it starts.
   /// </summary>
   public void LoadInto(CoreApp app)
   {
      var corpusCheckout = new DirectoryInfo(app.Services.Resolve<IEnvironmentPaths>().DatabaseDir);
      var storesFiles = app.Services.Resolve<IFileSystemInMemory>();
      foreach(var file in _filesOfTheNotes) storesFiles.WriteAllText(file.PathIn(corpusCheckout), file.Text);
      foreach(var file in _filesOnTheDisk) file.WriteTo(corpusCheckout);
      app.Collection.ReloadFromBackend();
   }

   static void CreateTheNotes(DataNeeded data, CoreApp app)
   {
      var noteServices = app.Services.NoteServices;

      if(data.HasFlag(DataNeeded.Kanji))
      {
         foreach(var kanjiSpec in KanjiSpec.TestKanjiList)
         {
            var kanji = KanjiNote.Create(noteServices, kanjiSpec.Question, kanjiSpec.Answer);
            KanjiReadingsNotation.AddTo(kanji, kanjiSpec.OnReadings, kanjiSpec.KunReadings);
         }
      }

      if(data.HasFlag(DataNeeded.Vocabulary)) ParsingCasesFolder.Corpus.AddTheVocabTo(app);

      if(data.HasFlag(DataNeeded.Sentences))
      {
         foreach(var sentence in SentenceSpec.TestSentenceList) SentenceNote.CreateTestNote(noteServices, sentence.Question, sentence.Answer);
      }
   }

   /// <summary>The files of the notes, as the memory image state store of <paramref name="app"/> keeps them in memory, without the store's own files.</summary>
   static IReadOnlyList<FileInTheCorpus> TheFilesOfTheNotesIn(CoreApp app, DirectoryInfo corpusCheckout)
   {
      var storesFiles = app.Services.Resolve<IFileSystemInMemory>();
      return [.. storesFiles.Paths
                            .Select(path => new FileInTheCorpus(Path.GetRelativePath(corpusCheckout.FullName, path), storesFiles.ReadAllText(path)))
                            .Where(it => !it.IsTheMemoryImageStateStoresOwn)];
   }

   /// <summary>The files on the disk in <paramref name="corpusCheckout"/>, such as the shared-conditions'.</summary>
   static IReadOnlyList<FileInTheCorpus> TheFilesOnTheDiskIn(DirectoryInfo corpusCheckout) =>
      [.. corpusCheckout.TheFilesInPathOrder("*").Select(file => new FileInTheCorpus(Path.GetRelativePath(corpusCheckout.FullName, file.FullName), File.ReadAllText(file.FullName)))];

   /// <summary>A file of the corpus: its path in the corpus checkout, such as <c>vocab\81\81020a41-d636-4ee9-a158-2443d8750482.json</c>, and its text.</summary>
   sealed record FileInTheCorpus(string PathInTheCorpus, string Text)
   {
      /// <summary>Whether it's one of the memory image state store's own files, in its <c>.state-store</c> folder, such as the cache it starts from.</summary>
      public bool IsTheMemoryImageStateStoresOwn => PathInTheCorpus.StartsWith(".state-store", StringComparison.OrdinalIgnoreCase);

      public string PathIn(DirectoryInfo corpusCheckout) => Path.Combine(corpusCheckout.FullName, PathInTheCorpus);

      public void WriteTo(DirectoryInfo corpusCheckout)
      {
         var file = new FileInfo(PathIn(corpusCheckout));
         file.Directory?.Create();
         File.WriteAllText(file.FullName, Text);
      }
   }
}
