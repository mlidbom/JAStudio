using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A folder holding notes as the corpus repository stores them, one JSON file per note, and the shared-conditions their match-conditions use by name:<br/>
/// the corpus checkout (<see cref="SourceCheckout.CorpusCheckout"/>), or the parsing cases' stripped-down corpus (<see cref="ParsingCasesFolder.StrippedDownCorpus"/>).<br/>
/// Only its vocab matter to the parsing cases.
/// </summary>
public sealed class CorpusFolder(DirectoryInfo folder)
{
   readonly DirectoryInfo _folder = folder;

   DirectoryInfo VocabFolder => new(Path.Combine(_folder.FullName, "vocab"));

   /// <summary>The folder of the shared-conditions, as the corpus repository keeps them, one JSON file each: none until a shared-condition is defined.</summary>
   DirectoryInfo SharedConditionsFolder => new(Path.Combine(_folder.FullName, "shared-conditions"));

   /// <summary>
   /// Adds the vocab of this folder to <paramref name="app"/>'s collection, read as JAStudio reads the corpus's notes. This folder's shared-conditions come<br/>
   /// first, since the vocab's match-conditions use them by name: copied to <paramref name="app"/>'s own corpus folder, and loaded from there as JAStudio<br/>
   /// loads them, by reloading the collection.
   /// </summary>
   public void AddTheVocabTo(CoreApp app)
   {
      CopyTheSharedConditionsTo(new CorpusFolder(new DirectoryInfo(app.Services.Resolve<IEnvironmentPaths>().DatabaseDir)));
      app.Collection.ReloadFromBackend();
      foreach(var vocab in ReadTheVocab(app.Services.Resolve<NoteSerializer>())) app.Collection.Vocab.Add(vocab);
   }

   /// <summary>
   /// The vocab of this folder, read as JAStudio reads the corpus's notes, with <paramref name="serializer"/>, whose JAStudio has this folder's shared-conditions<br/>
   /// (<see cref="AddTheVocabTo"/>).
   /// </summary>
   public IEnumerable<VocabNote> ReadTheVocab(NoteSerializer serializer) => VocabFiles().Select(file => serializer.DeserializeVocab(File.ReadAllText(file.FullName)));

   /// <summary>
   /// Replaces the vocab of this folder with <paramref name="vocab"/>, written as JAStudio writes the corpus's notes, and its shared-conditions with those of<br/>
   /// <paramref name="theCorpusOfTheVocab"/>, which the vocab's match-conditions use.
   /// </summary>
   public void ReplaceTheVocabWith(IEnumerable<VocabNote> vocab, NoteSerializer serializer, CorpusFolder theCorpusOfTheVocab)
   {
      DeleteTheFilesIn(VocabFolder);

      var repository = new FileSystemNoteRepository(serializer, _folder.FullName);
      foreach(var note in vocab) repository.Save(note);

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
   /// read as no vocab, it would refresh the stripped-down corpus to nothing.
   /// </summary>
   IReadOnlyList<FileInfo> VocabFiles() => [.. VocabFolder.EnumerateFiles("*.json", SearchOption.AllDirectories)];
}
