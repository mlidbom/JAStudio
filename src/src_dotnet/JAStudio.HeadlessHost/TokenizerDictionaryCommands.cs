using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JAStudio.Core.LanguageServices.MeCab;
using JAStudio.Core.LanguageServices.MeCab.Comparing;
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

   /// <summary>What a dictionary option is given to name the IPADIC that MeCab.DotNet comes with (<see cref="TokenizerDictionary.TheBundledIpadic"/>).</summary>
   const string TheBundledIpadic = "bundled-ipadic";

   /// <summary>What a dictionary option is given to name the UniDic JAStudio tokenizes with (<see cref="TokenizerDictionary.UniDic"/>).</summary>
   const string TheUniDic = "unidic";

   const string Usage = $"""
                         tokenizer-dictionaries compare --first <dictionary> --second <dictionary> --reservoir-file <file> --sentences <count> --report-folder <folder>
                         tokenizer-dictionaries tokenize --dictionary <dictionary> --text <text>
                         where <dictionary> is a dictionary's folder, {TheBundledIpadic} or {TheUniDic}
                         """;

   public static int Run(string[] args) => args switch
   {
      ["compare", .. var values] => Compare(CommandLineValues.From(values)),
      ["tokenize", .. var values] => Tokenize(CommandLineValues.From(values)),
      _ => throw new ArgumentException($"Unknown tokenizer-dictionaries command: {string.Join(' ', args)}{Environment.NewLine}{Usage}")
   };

   /// <summary>
   /// Compares how the tokenizer dictionaries <c>--first</c> and <c>--second</c> tokenize up to <c>--sentences</c> of the reservoir's sentences,<br/>
   /// spread evenly over it (<see cref="ReservoirFile.SentencesSpreadEvenly"/>), and writes the report (<see cref="TokenizerDictionaryComparisonReport"/>)<br/>
   /// into <c>--report-folder</c>. The reservoir is only read.
   /// </summary>
   static int Compare(CommandLineValues values)
   {
      var first = TheDictionaryGivenAs("first", values);
      var second = TheDictionaryGivenAs("second", values);
      using var reservoir = ReservoirFile.OpenOrCreate(values.ExistingFile("reservoir-file"));
      Console.WriteLine("Reading the sentences...");
      var sentences = reservoir.SentencesSpreadEvenly(values.Integer("sentences")).Select(it => it.Text).ToList();
      Console.WriteLine($"Comparing {first} and {second} over {sentences.Count:N0} sentences...");
      var watch = Stopwatch.StartNew();
      var comparison = TokenizerDictionaryComparison.Of(first, second, sentences);
      Console.WriteLine($"Compared in {watch.Elapsed.TotalSeconds:F0} s. Writing the report...");
      TokenizerDictionaryComparisonReport.WriteTo(new DirectoryInfo(values.Value("report-folder")), comparison);
      return 0;
   }

   /// <summary>
   /// Prints each token the tokenizer dictionary <c>--dictionary</c> cuts <c>--text</c> into, as the dictionary gives it (<see cref="RawMeCabToken"/>),<br/>
   /// before any of JAStudio's corrections: a line each, with its part of speech, inflection, base form, lemma, reading and pronunciation.
   /// </summary>
   static int Tokenize(CommandLineValues values)
   {
      var dictionary = TheDictionaryGivenAs("dictionary", values);
      var text = values.Value("text");
      Console.WriteLine($"{text}, as {dictionary} tokenizes it:");
      foreach(var token in dictionary.Tokenize(text))
         Console.WriteLine(LineOf(token));
      return 0;

      static string LineOf(RawMeCabToken token) =>
         string.Join('\t',
                     $"   {token.Start,3} {token.Surface}{(token.IsUnknownToTheDictionary ? " (unknown)" : "")}",
                     token.PartOfSpeech,
                     $"{token.InflectionType} {token.InflectionForm}",
                     $"base {token.BaseForm}",
                     $"lemma {token.Lemma ?? RawMeCabToken.Nothing}",
                     $"reading {token.Reading}",
                     $"pronunciation {token.Pronunciation}");
   }

   static TokenizerDictionary TheDictionaryGivenAs(string name, CommandLineValues values) => values.Value(name) switch
   {
      TheBundledIpadic => TokenizerDictionary.TheBundledIpadic,
      TheUniDic => TokenizerDictionary.UniDic,
      _ => TokenizerDictionary.In(values.ExistingFolder(name))
   };
}
