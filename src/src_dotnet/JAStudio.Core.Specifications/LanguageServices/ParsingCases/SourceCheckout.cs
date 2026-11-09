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

   /// <summary>The corpus checkout, the corpus repository's submodule in the source checkout, which JAStudio inside Anki works on.</summary>
   public static CorpusFolder CorpusCheckout => new(new DirectoryInfo(Path.Combine(Folder.FullName, "src", "jas_database")));

   static DirectoryInfo TheFolderHoldingTheSolution()
   {
      for(var folder = new DirectoryInfo(AppContext.BaseDirectory); folder != null; folder = folder.Parent)
         if(folder.EnumerateFiles("JAStudio.slnx").Any()) return folder;

      throw new InvalidOperationException($"No folder above {AppContext.BaseDirectory} holds JAStudio.slnx.");
   }
}
