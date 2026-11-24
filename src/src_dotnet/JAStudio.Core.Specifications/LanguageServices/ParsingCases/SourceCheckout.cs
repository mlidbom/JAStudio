using System;
using System.IO;
using System.Linq;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// The checkout of JAStudio's source repository these specifications were built from: the folder holding <c>JAStudio.slnx</c>, found above the<br/>
/// folder the specifications run in.
/// </summary>
static class SourceCheckout
{
   public static DirectoryInfo Folder { get; } = TheFolderHoldingTheSolution();

   /// <summary>
   /// The corpus checkout, the corpus repository's submodule in the source checkout, which JAStudio inside Anki works on. It must be checked out:<br/>
   /// a new linked worktree's isn't, as nothing in the Definition of Done reads it, and <c>dev_scripts/check-out-the-corpus.ps1</c> checks it out.
   /// </summary>
   public static CorpusFolder CorpusCheckout =>
      Path.Exists(Path.Combine(TheCorpusCheckoutsFolder.FullName, ".git"))
         ? new CorpusFolder(TheCorpusCheckoutsFolder)
         : throw new InvalidOperationException($"The corpus isn't checked out in {TheCorpusCheckoutsFolder.FullName}: run dev_scripts/check-out-the-corpus.ps1 in {Folder.FullName}, which takes about a minute.");

   static DirectoryInfo TheCorpusCheckoutsFolder => new(Path.Combine(Folder.FullName, "src", "jas_database"));

   static DirectoryInfo TheFolderHoldingTheSolution()
   {
      for(var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
         if(folder.EnumerateFiles("JAStudio.slnx").Any()) return folder;

      throw new InvalidOperationException($"No folder above {AppContext.BaseDirectory} holds JAStudio.slnx.");
   }
}
