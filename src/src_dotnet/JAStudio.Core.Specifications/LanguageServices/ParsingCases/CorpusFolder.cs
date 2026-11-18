using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Teventive.StateStore;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Core.SysUtils.IO;
using System;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A folder holding notes as the corpus repository stores them, one JSON file per note, and the shared-conditions their match-conditions use by name:<br/>
/// the corpus checkout (<see cref="SourceCheckout.CorpusCheckout"/>), or the parsing cases' corpus (<see cref="ParsingCasesFolder.Corpus"/>).<br/>
/// Its vocab are what the parsing cases are parsed against, and the corpus checkout's sentences that are parsing cases are what the case files are<br/>
/// generated from.
/// </summary>
public sealed class CorpusFolder(DirectoryInfo folder)
{
   readonly DirectoryInfo _folder = folder;

   DirectoryInfo VocabFolder => new(Path.Combine(_folder.FullName, "vocab"));
   DirectoryInfo SentencesFolder => new(Path.Combine(_folder.FullName, "sentences"));

   /// <summary>The folder of the shared-conditions, as the corpus repository keeps them, one JSON file each: none until a shared-condition is defined.</summary>
   DirectoryInfo SharedConditionsFolder => new(Path.Combine(_folder.FullName, "shared-conditions"));

   /// <summary>
   /// Loads the vocab of this folder into <paramref name="app"/>'s collection, as JAStudio loads the corpus when it starts: its files, and the<br/>
   /// shared-conditions' files, which the vocab's match-conditions use by name, are written into <paramref name="app"/>'s own corpus checkout, the vocab's<br/>
   /// into the state store's files, kept in memory, and the collection is loaded again.
   /// </summary>
   public void AddTheVocabTo(CoreApp app)
   {
      var corpusCheckoutOfTheApp = new DirectoryInfo(app.Services.Resolve<IEnvironmentPaths>().DatabaseDir);
      CopyTheSharedConditionsTo(new CorpusFolder(corpusCheckoutOfTheApp));
      var storesFiles = app.Services.Resolve<ITaggregateFilesInMemory>();
      foreach(var file in VocabFiles()) storesFiles.WriteAllText(Path.Combine(corpusCheckoutOfTheApp.FullName, Path.GetRelativePath(_folder.FullName, file.FullName)), File.ReadAllText(file.FullName));
      app.Collection.ReloadFromBackend();
   }

   /// <summary>
   /// The vocab of this folder, read as JAStudio reads the corpus's notes, with <paramref name="serializer"/>, whose JAStudio has this folder's shared-conditions<br/>
   /// (<see cref="AddTheVocabTo"/>).
   /// </summary>
   public IEnumerable<VocabNote> ReadTheVocab(NoteSerializer serializer) => VocabFiles().Select(file => serializer.DeserializeVocab(File.ReadAllText(file.FullName)));

   /// <summary>
   /// The sentences of this folder that are parsing cases (<see cref="SentenceNote.ParsingCase"/>), read as JAStudio reads the corpus's notes, with<br/>
   /// <paramref name="serializer"/>, whose JAStudio has this folder's vocab (<see cref="AddTheVocabTo"/>). They aren't added to its collection.
   /// </summary>
   public IReadOnlyList<SentenceNote> ReadTheSentencesThatAreParsingCases(NoteSerializer serializer) =>
   [
      .. SentencesFolder.TheFilesInPathOrder("*.json")
                        .Select(file => File.ReadAllText(file.FullName))
                        .Where(json => json.Contains("\"parsingCase\"", StringComparison.Ordinal))
                        .Select(serializer.DeserializeSentence)
                        .Where(it => it.ParsingCase != null)
   ];

   /// <summary>
   /// Replaces the vocab of this folder with <paramref name="vocab"/>, written as JAStudio writes the corpus's notes, and its shared-conditions with those of<br/>
   /// <paramref name="theCorpusOfTheVocab"/>, which the vocab's match-conditions use.
   /// </summary>
   public void ReplaceTheVocabWith(IEnumerable<VocabNote> vocab, CorpusFolder theCorpusOfTheVocab)
   {
      DeleteTheFilesIn(VocabFolder);
      var noteFiles = new CorpusNoteFiles(_folder);
      foreach(var it in vocab) noteFiles.Write(it);
      theCorpusOfTheVocab.CopyTheSharedConditionsTo(this);
   }

   /// <summary>Replaces the shared-conditions of <paramref name="destination"/> with this folder's.</summary>
   void CopyTheSharedConditionsTo(CorpusFolder destination)
   {
      DeleteTheFilesIn(destination.SharedConditionsFolder);
      if(!SharedConditionsFolder.Exists) return;

      destination.SharedConditionsFolder.Create();
      foreach(var file in SharedConditionsFolder.EnumerateFiles("*.json")) file.CopyTo(Path.Combine(destination.SharedConditionsFolder.FullName, file.Name));
   }

   /// <summary>
   /// Deletes the files in <paramref name="folder"/>, but not the folders: Windows deletes a folder that something such as the IDE watches only once it lets<br/>
   /// go, so writing into the same path straight after deleting the folder fails with <see cref="DirectoryNotFoundException"/>. An empty folder left behind<br/>
   /// is nothing to git.
   /// </summary>
   static void DeleteTheFilesIn(DirectoryInfo folder)
   {
      if(!folder.Exists) return;
      foreach(var file in folder.EnumerateFiles("*", SearchOption.AllDirectories)) file.Delete();
   }

   public override string ToString() => _folder.FullName;

   /// <summary>
   /// The file of each vocab of this folder. A folder without vocab fails loudly, such as a corpus checkout whose submodule isn't initialized:<br/>
   /// read as no vocab, it would refresh the parsing cases' corpus to nothing.
   /// </summary>
   IReadOnlyList<FileInfo> VocabFiles() => VocabFolder.TheFilesInPathOrder("*.json");
}
