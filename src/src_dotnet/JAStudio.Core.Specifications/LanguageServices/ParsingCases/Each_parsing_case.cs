using System.Linq;
using Compze.Must;
using Xunit;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// The everyday mode of the parsing cases: each case (<see cref="ParsingCase"/>) parsed against the stripped-down corpus<br/>
/// (<see cref="ParsingCasesFolder.StrippedDownCorpus"/>), which parses it as the corpus checkout (<see cref="SourceCheckout.CorpusCheckout"/>) did when<br/>
/// the full mode last refreshed it (<see cref="In_the_full_mode"/>). A case fails when its parsing result differs from the expected one in any<br/>
/// way, and says how its words differ.
/// </summary>
public class Each_parsing_case(JAStudioWithTheStrippedDownCorpus jastudio) : IClassFixture<JAStudioWithTheStrippedDownCorpus>
{
   readonly JAStudioWithTheStrippedDownCorpus _jastudio = jastudio;

   public static TheoryData<string> CaseNames => [.. ParsingCasesFolder.AllCases().Select(it => it.Name)];

   [Theory, MemberData(nameof(CaseNames))]
   public void parses_against_the_stripped_down_corpus_exactly_as_the_case_expects(string caseName)
   {
      var parsingCase = ParsingCasesFolder.CaseNamed(caseName);
      parsingCase.HowTheParsedWordsDifferFromTheExpected(_jastudio.Parse(parsingCase), _jastudio.Collection).Must().BeEmpty();
   }
}
