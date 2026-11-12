using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Analysis.Timing;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Analysis;

/// <summary>Times parsing the collection's two sentences, 明日学校に行く and 今日は雨だ, on two threads.</summary>
public class When_timing_parsing_every_sentence : McpServerSpecification
{
   readonly TimeToParseEverySentence _time;

   public When_timing_parsing_every_sentence()
   {
      CreateSentence("明日学校に行く");
      CreateSentence("今日は雨だ");
      _time = ResultOf<TimeToParseEverySentence>(CallTool("time_parsing_every_sentence", new Dictionary<string, object?> { ["threads"] = 2 }));
   }

   [XF] public void it_reports_that_it_parsed_both_sentences() => _time.Sentences.Must().Be(2);
   [XF] public void it_reports_that_it_parsed_on_two_threads() => _time.Threads.Must().Be(2);
   [XF] public void it_reports_how_long_tokenizing_took() => _time.Tokenizing.Seconds.Must().BeGreaterThan(0);
   [XF] public void it_reports_how_long_analyzing_took() => _time.Analyzing.Seconds.Must().BeGreaterThan(0);
   [XF] public void it_reports_how_much_memory_analyzing_allocated() => _time.Analyzing.AllocatedMegabytes.Must().BeGreaterThan(0);
   [XF] public void it_reports_the_most_private_memory_the_process_used_while_analyzing() => _time.Analyzing.PeakMemoryUse.PrivateMegabytes.Must().BeGreaterThan(0);
}
