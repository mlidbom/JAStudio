using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JAStudio.Core.LanguageServices.TokenizerDictionaries;
using JAStudio.Core.LanguageServices.TokenizerDictionaries.Comparing;
using JAStudio.Core.Reservoir;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio's batch commands for tokenizer dictionaries (<see cref="TokenizerDictionary"/>), run as<br/>
/// <c>JAStudio.HeadlessHost tokenizer-dictionaries &lt;command&gt;</c> followed by <c>--name value</c> pairs. Each runs to its end and exits,<br/>
/// reporting as it goes on standard output.
/// </summary>
static class TokenizerDictionaryCommands
{
   public const string CommandName = "tokenizer-dictionaries";

   /// <summary>What <c>--first</c> or <c>--second</c> is given to compare the IPADIC that MeCab.DotNet comes with (<see cref="TokenizerDictionary.TheBundledIpadic"/>).</summary>
   const string TheBundledIpadic = "bundled-ipadic";

   const string Usage = $"""
                         tokenizer-dictionaries compare --first <folder, or {TheBundledIpadic}> --second <folder, or {TheBundledIpadic}> --reservoir-file <file> --sentences <count> --report-folder <folder>
                         """;

   public static int Run(string[] args) => args switch
   {
      ["compare", .. var values] => Compare(CommandLineValues.From(values)),
      _ => throw new ArgumentException($"Unknown tokenizer-dictionaries command: {string.Join(' ', args)}{Environment.NewLine}{Usage}")
   };

   /// <summary>
   /// Compares how the tokenizer dictionaries <c>--first</c> and <c>--second</c> tokenize up to <c>--sentences</c> of the reservoir's sentences,<br/>
   /// spread evenly over it (<see cref="ReservoirFile.SentencesSpreadEvenly"/>), and writes the report (<see cref="TokenizerDictionaryComparisonReport"/>)<br/>
   /// into <c>--report-folder</c>. The reservoir is only read.
   /// </summary>
   static int Compare(CommandLineValues values)
   {
      var first = TheDictionaryGivenAs("first");
      var second = TheDictionaryGivenAs("second");
      using var reservoir = ReservoirFile.OpenOrCreate(values.ExistingFile("reservoir-file"));
      Console.WriteLine("Reading the sentences...");
      var sentences = reservoir.SentencesSpreadEvenly(values.Integer("sentences")).Select(it => it.Text).ToList();
      Console.WriteLine($"Comparing {first} and {second} over {sentences.Count:N0} sentences...");
      var watch = Stopwatch.StartNew();
      var comparison = TokenizerDictionaryComparison.Of(first, second, sentences);
      Console.WriteLine($"Compared in {watch.Elapsed.TotalSeconds:F0} s. Writing the report...");
      TokenizerDictionaryComparisonReport.WriteTo(new DirectoryInfo(values.Value("report-folder")), comparison);
      return 0;

      TokenizerDictionary TheDictionaryGivenAs(string name) =>
         values.Value(name) == TheBundledIpadic ? TokenizerDictionary.TheBundledIpadic : TokenizerDictionary.In(values.ExistingFolder(name));
   }
}
