using System.Collections.Generic;
using System.IO;
using System.Linq;

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

   /// <summary>Every parsing case, by name.</summary>
   public static IReadOnlyList<ParsingCase> AllCases() => [.. CasesFolder.EnumerateFiles("*.json").Select(ParsingCase.Read).OrderBy(it => it.Name)];

   public static ParsingCase CaseNamed(string name) => ParsingCase.Read(new FileInfo(Path.Combine(CasesFolder.FullName, $"{name}.json")));
}
