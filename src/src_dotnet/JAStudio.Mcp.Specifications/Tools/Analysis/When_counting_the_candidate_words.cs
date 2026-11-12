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
   [XF] public void it_counts_a_candidate_word_of_one_token_for_each_of_the_four_tokens() => _counts.ByTokensSpanned.Single(it => it.Tokens == 1).Built.Must().Be(4);
   [XF] public void it_counts_no_candidate_word_spanning_more_than_the_four_tokens() => _counts.ByTokensSpanned.Max(it => it.Tokens).Must().BeLessThanOrEqualTo(4);

   [XF] public void it_counts_no_more_known_candidate_words_than_were_built() => (_counts.Known <= _counts.Built).Must().BeTrue();
   [XF] public void it_counts_no_more_valid_candidate_words_than_are_known() => (_counts.Valid <= _counts.Known).Must().BeTrue();
}
