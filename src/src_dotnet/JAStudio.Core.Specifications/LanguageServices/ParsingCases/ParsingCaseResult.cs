using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases.Breakdowns;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases;

/// <summary>
/// What a parsing case's sentence parses to: its parsing result as the corpus stores it, and its sentence breakdown under every combination of<br/>
/// the options that change it. A case expects one exactly (<see cref="ParsingCase"/>).
/// </summary>
/// <param name="ParsedWords">The stored parsing result: every valid match, displayed or not.</param>
/// <param name="Breakdowns">The breakdown under each combination of the options that change it: one, under any options, when none does.</param>
public sealed record ParsingCaseResult(IReadOnlyList<ParsedWordReported> ParsedWords, IReadOnlyList<BreakdownUnderOptions> Breakdowns)
{
   /// <summary>Whether this is exactly the result <paramref name="other"/> is, the questions of the vocab included.</summary>
   public bool IsTheSameAs(ParsingCaseResult other) =>
      ParsedWords.SequenceEqual(other.ParsedWords) &&
      Breakdowns.Count == other.Breakdowns.Count &&
      Breakdowns.Zip(other.Breakdowns).All(it => it.First.OptionsDescription == it.Second.OptionsDescription && it.First.DisplayedWords.SequenceEqual(it.Second.DisplayedWords));

   /// <summary>The names of the options that change the breakdown, in the order of <see cref="BreakdownParsingOption.All"/>.</summary>
   public IReadOnlyList<string> OptionsChangingTheBreakdown =>
      [.. BreakdownParsingOption.All.Select(it => it.Name).Where(name => Breakdowns.Any(breakdown => breakdown.Options.ContainsKey(name)))];
}
