using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Analysis;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Analysis;

/// <summary>Counts the candidate words of the collection's one sentence, 今日は雨だ, whose four tokens are 今日, は, 雨 and だ.</summary>
public class When_counting_the_candidate_words : McpServerSpecification
{
   readonly CandidateWordCounts _counts;

   public When_counting_the_candidate_words()
   {
      CreateSentence("今日は雨だ");
      _counts = ResultOf<CandidateWordCounts>(CallTool("count_the_candidate_words", new Dictionary<string, object?>()));
   }

   [XF] public void it_reports_that_it_analyzed_the_sentence() => _counts.Sentences.Must().Be(1);
   [XF] public void it_counts_the_ten_candidate_words_four_tokens_make() => _counts.Built.Must().Be(10);

   [XF] public void it_counts_four_of_one_token_three_of_two_two_of_three_and_one_of_all_four() =>
      _counts.ByTokensSpanned.Select(it => (it.Tokens, it.Built)).Must().SequenceEqual([(1, 4), (2, 3), (3, 2), (4, 1)]);

   [XF] public void it_counts_no_more_known_candidate_words_than_were_built() => (_counts.Known <= _counts.Built).Must().BeTrue();
   [XF] public void it_counts_no_more_valid_candidate_words_than_are_known() => (_counts.Valid <= _counts.Known).Must().BeTrue();
}
