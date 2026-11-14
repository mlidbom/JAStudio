using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Core.SysUtils.IO;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// A folder holding notes as the corpus repository stores them, one JSON file per note: the corpus checkout (<see cref="SourceCheckout.CorpusCheckout"/>),<br/>
/// or the parsing cases' stripped-down corpus (<see cref="ParsingCasesFolder.StrippedDownCorpus"/>). Only its vocab matter to the parsing cases.
/// </summary>
public sealed class CorpusFolder(DirectoryInfo folder)
{
   readonly DirectoryInfo _folder = folder;

   DirectoryInfo VocabFolder => new(Path.Combine(_folder.FullName, "vocab"));

   /// <summary>The vocab of this folder, read as JAStudio reads the corpus's notes.</summary>
   public IEnumerable<VocabNote> ReadTheVocab(NoteSerializer serializer) => VocabFiles().Select(file => serializer.DeserializeVocab(File.ReadAllText(file.FullName)));

   /// <summary>Replaces the vocab of this folder with <paramref name="vocab"/>, written as JAStudio writes the corpus's notes.</summary>
   public void ReplaceTheVocabWith(IEnumerable<VocabNote> vocab, NoteSerializer serializer)
   {
      DeleteTheVocabFiles();

      var repository = new FileSystemNoteRepository(serializer, _folder.FullName);
      foreach(var note in vocab) repository.Save(note);
   }

   /// <summary>
   /// Deletes the files of the vocab, but not their folders: Windows deletes a folder that something such as the IDE watches only once it lets go, so<br/>
   /// writing into the same path straight after deleting the folder fails with <see cref="DirectoryNotFoundException"/>. An empty folder left behind is<br/>
   /// nothing to git.
   /// </summary>
   void DeleteTheVocabFiles()
   {
      if(!VocabFolder.Exists) return;
      foreach(var file in VocabFolder.EnumerateFiles("*", SearchOption.AllDirectories)) file.Delete();
   }

   public override string ToString() => _folder.FullName;

   /// <summary>
   /// The file of each vocab of this folder. A folder without vocab fails loudly, such as a corpus checkout whose submodule isn't initialized:<br/>
   /// read as no vocab, it would refresh the stripped-down corpus to nothing.
   /// </summary>
   IReadOnlyList<FileInfo> VocabFiles() => VocabFolder.TheFilesInPathOrder("*.json");
}
