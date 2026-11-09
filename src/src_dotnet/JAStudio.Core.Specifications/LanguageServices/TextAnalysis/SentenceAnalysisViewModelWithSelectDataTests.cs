using JAStudio.Core.Specifications.Fixtures;
using Xunit;

namespace JAStudio.Core.Specifications.LanguageServices.TextAnalysis;

/// <summary>
/// The one case of these specifications not converted to a parsing case (<see cref="ParsingCases.ParsingCase"/>): it checks every match of the<br/>
/// analysis, valid or not, which a parsing case doesn't hold. Against vocab copied by hand. Tests ported from
/// test_sentence_analysis_viewmodel_with_select_data.py.
/// </summary>
public class SentenceAnalysisViewModelWithSelectDataTests : SentenceAnalysisViewModelCommon
{
   public SentenceAnalysisViewModelWithSelectDataTests() : base(DataNeeded.Vocabulary) {}

   [Theory]
   [InlineData("風の強さに驚きました", "風の強い", "風", "の", "強さ", "強", "強い", "さ", "に", "驚き", "驚く", "まし:ませ", "まし", "ます", "た")]
   public void AllWordsEqual(string sentence, params string[] expectedOutput)
   {
      AssertAllWordsEqual(sentence, expectedOutput);
   }
}
