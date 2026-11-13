using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Reporting;
using static System.FormattableString;

namespace JAStudio.Core.LanguageServices.TokenizerDictionaries.Comparing;

/// <summary>
/// The report of a <see cref="TokenizerDictionaryComparison"/>, written to a folder (<see cref="ReportFolder"/>): <c>report.md</c> with the totals<br/>
/// and the most common of each kind of difference, and a tab-separated file of each kind in full, with up to <see cref="Tally{TThing}.ExamplesKept"/><br/>
/// examples of each.
/// </summary>
public sealed class TokenizerDictionaryComparisonReport
{
   readonly TokenizerDictionaryComparison _comparison;
   readonly ReportFolder _folder;

   TokenizerDictionaryComparisonReport(TokenizerDictionaryComparison comparison, ReportFolder folder)
   {
      _comparison = comparison;
      _folder = folder;
   }

   string FirstsName => _comparison.First.Name;
   string SecondsName => _comparison.Second.Name;

   /// <summary>Writes the report of <paramref name="comparison"/> into <paramref name="reportFolder"/>, created if need be.</summary>
   public static void WriteTo(DirectoryInfo reportFolder, TokenizerDictionaryComparison comparison)
   {
      var report = new TokenizerDictionaryComparisonReport(comparison, new ReportFolder(reportFolder));
      report._folder.Title($"How {comparison.First.Name} and {comparison.Second.Name} tokenize the same sentences");
      report.WriteTheTotals();
      report.WriteTheCuttingDifferences();
      report.WriteTheCorrespondences("Parts of speech", "parts-of-speech", "Each part of speech of the tokens both cut alike, and how often.", comparison.PartsOfSpeech);
      report.WriteTheCorrespondences("Inflection types", "inflection-types", "The inflection types of the tokens both cut alike that either gives one.", comparison.InflectionTypes);
      report.WriteTheCorrespondences("Inflection forms", "inflection-forms", "The inflection forms of the tokens both cut alike that either gives one.", comparison.InflectionForms);
      report.WriteTheCorrespondences("Base forms that differ", "base-forms", "The tokens both cut alike and know, whose base forms differ.", comparison.BaseForms);
      report.WriteTheCorrespondences("Words that differ",
                                     "words",
                                     "The tokens both cut alike and know, whose words differ: a lemma, or, for IPADIC, which has none, the base form.",
                                     comparison.Words);
      report.WriteTheCorrespondences($"Unknown to {comparison.First.Name} only",
                                     "unknown-to-the-first-only",
                                     $"The tokens both cut alike that {comparison.First.Name} doesn't know and {comparison.Second.Name} does, with {comparison.Second.Name}'s part of speech.",
                                     comparison.UnknownToTheFirstOnly);
      report.WriteTheCorrespondences($"Unknown to {comparison.Second.Name} only",
                                     "unknown-to-the-second-only",
                                     $"The tokens both cut alike that {comparison.Second.Name} doesn't know and {comparison.First.Name} does, with {comparison.First.Name}'s part of speech.",
                                     comparison.UnknownToTheSecondOnly);
      report._folder.SaveTheMarkdown();
   }

   void WriteTheTotals() =>
      _folder.Section("Totals",
                      Invariant($"{_comparison.Sentences:N0} sentences, {_comparison.SentencesCutAlike:N0} ({Percent(_comparison.SentencesCutAlike, _comparison.Sentences)}) cut into the same tokens by both"),
                      Invariant($"{FirstsName}: {_comparison.FirstsTokens:N0} tokens, {_comparison.FirstsUnknownTokens:N0} ({Percent(_comparison.FirstsUnknownTokens, _comparison.FirstsTokens)}) unknown to it"),
                      Invariant($"{SecondsName}: {_comparison.SecondsTokens:N0} tokens, {_comparison.SecondsUnknownTokens:N0} ({Percent(_comparison.SecondsUnknownTokens, _comparison.SecondsTokens)}) unknown to it"),
                      Invariant($"{_comparison.TokensCutAlike:N0} tokens cut alike by both: {Percent(_comparison.TokensCutAlike, _comparison.FirstsTokens)} of {FirstsName}'s, {Percent(_comparison.TokensCutAlike, _comparison.SecondsTokens)} of {SecondsName}'s"));

   void WriteTheCuttingDifferences()
   {
      var differences = _comparison.CuttingDifferences.MostFoundFirst();
      _folder.Section("Where they cut differently",
                      Invariant($"{differences.Count:N0} different differences, found {_comparison.CuttingDifferences.Total:N0} times"),
                      Described($"{SecondsName} cuts {FirstsName}'s one token into several", differences.Where(it => it.Thing.SecondCutsFiner)),
                      Described($"{SecondsName} cuts {FirstsName}'s several tokens as one", differences.Where(it => it.Thing.SecondCutsCoarser)),
                      Described("Both cut into several tokens, at different places", differences.Where(it => !it.Thing.SecondCutsFiner && !it.Thing.SecondCutsCoarser)));
      _folder.TableWithTheRestInAFile("cutting-differences",
                                      [FirstsName, SecondsName, "Found", "Example sentences"],
                                      [.. differences.Select(it => new[] { it.Thing.FirstCut, it.Thing.SecondCut, Invariant($"{it.Count:N0}"), ExamplesOf(it) })]);
      return;

      static string Described(string kind, IEnumerable<Tally<CuttingDifference>.Counted> differences)
      {
         var ofTheKind = differences.ToList();
         return Invariant($"{kind}: {ofTheKind.Count:N0} differences, found {ofTheKind.Sum(it => (long)it.Count):N0} times");
      }
   }

   void WriteTheCorrespondences(string heading, string fileName, string whatTheyAre, Tally<Correspondence> correspondences)
   {
      var counted = correspondences.MostFoundFirst();
      _folder.Section(heading, whatTheyAre, Invariant($"{counted.Count:N0} different, found {correspondences.Total:N0} times"));
      _folder.TableWithTheRestInAFile(fileName,
                                      [FirstsName, SecondsName, "Found", "Examples"],
                                      [.. counted.Select(it => new[] { it.Thing.First, it.Thing.Second, Invariant($"{it.Count:N0}"), ExamplesOf(it) })]);
   }

   static string ExamplesOf<TThing>(Tally<TThing>.Counted counted) where TThing : notnull => string.Join(" / ", counted.Examples);

   static string Percent(long part, long whole) => whole == 0 ? "-" : Invariant($"{100.0 * part / whole:F1}%");
}
