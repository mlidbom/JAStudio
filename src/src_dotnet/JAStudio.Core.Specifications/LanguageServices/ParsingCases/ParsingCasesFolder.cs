using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Must;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// Where the parsing cases live in the source checkout: the folder of this namespace, with the case files in <c>Cases</c>, and the<br/>
/// stripped-down corpus they're parsed against in <c>StrippedDownCorpus</c>. Both are source, committed with the specifications.
/// </summary>
static class ParsingCasesFolder
{
   static readonly DirectoryInfo Folder = new(Path.Combine(SourceCheckout.Folder.FullName, "src", "src_dotnet", "JAStudio.Core.Specifications", "LanguageServices", "ParsingCases"));

   static DirectoryInfo CasesFolder => new(Path.Combine(Folder.FullName, "Cases"));

   /// <summary>
   /// The parsing cases' stripped-down corpus: every vocab of the corpus checkout that parsing the cases looked up, as of the last run of the<br/>
   /// full mode (<see cref="In_the_full_mode"/>). The cases parse against it exactly as against the corpus checkout, but in a fraction of the time,<br/>
   /// and on machines without the corpus, such as CI's.
   /// </summary>
   public static CorpusFolder StrippedDownCorpus => new(new DirectoryInfo(Path.Combine(Folder.FullName, "StrippedDownCorpus")));

   /// <summary>
   /// Replaces the case files with one generated from each of <paramref name="parsingCasesOfTheCorpus"/>, sentences of the corpus that are parsing<br/>
   /// cases, naming each vocab by its question in <paramref name="collection"/>. Returns the case of each sentence.
   /// </summary>
   public static IReadOnlyDictionary<SentenceNote, ParsingCaseFile> ReplaceTheCasesWithThoseOf(IReadOnlyList<SentenceNote> parsingCasesOfTheCorpus, JPCollection collection)
   {
      // A corpus checkout without parsing cases fails loudly, such as one from before the cases moved into the corpus: read as no cases, it would
      // delete every case file.
      parsingCasesOfTheCorpus.Must().NotBeEmpty();

      var sentencesByTheNameOfTheirCase = parsingCasesOfTheCorpus.ToLookup(ParsingCaseFile.NameOfTheCaseOf);
      sentencesByTheNameOfTheirCase.Where(it => it.Count() > 1).Select(it => it.Key).Must().BeEmpty();

      foreach(var file in CasesFolder.EnumerateFiles("*.json")) file.Delete();
      return parsingCasesOfTheCorpus.ToDictionary(sentence => sentence, sentence => ParsingCaseFile.WriteTheFileOf(sentence, CasesFolder, collection));
   }

   /// <summary>Every parsing case, by name.</summary>
   public static IReadOnlyList<ParsingCaseFile> AllCases() => [.. CasesFolder.EnumerateFiles("*.json").Select(ParsingCaseFile.Read).OrderBy(it => it.Name)];

   public static ParsingCaseFile CaseNamed(string name) => ParsingCaseFile.Read(new FileInfo(Path.Combine(CasesFolder.FullName, $"{name}.json")));
}
