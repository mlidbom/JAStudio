using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases.Breakdowns;

/// <summary>
/// A sentence's breakdown, the words it displays, under one combination of the options that change it: of all the<br/>
/// <see cref="BreakdownParsingOption.All"/>, those whose value changes what this sentence's breakdown displays. Any option not named doesn't.
/// </summary>
/// <param name="Options">The value of each option that changes the breakdown, by its <see cref="BreakdownParsingOption.Name"/>. Empty when none does.</param>
public sealed record BreakdownUnderOptions(IReadOnlyDictionary<string, bool> Options, IReadOnlyList<BreakdownWord> DisplayedWords)
{
   /// <summary>The options this breakdown is under, to read, such as <c>with hideAllCompounds on</c>. Breakdowns under the same options have the same.</summary>
   [JsonIgnore]
   public string OptionsDescription =>
      Options.Count == 0
         ? "under any options"
         : $"with {string.Join(", ", BreakdownParsingOption.All.Where(it => Options.ContainsKey(it.Name)).Select(it => $"{it.Name} {(Options[it.Name] ? "on" : "off")}"))}";

   /// <summary>Whether this is the same breakdown as <paramref name="other"/>: the same words displayed in the same order, under the same options.</summary>
   public bool IsTheSameAs(BreakdownUnderOptions other) =>
      OptionsDescription == other.OptionsDescription &&
      DisplayedWords.Count == other.DisplayedWords.Count &&
      DisplayedWords.Zip(other.DisplayedWords).All(it => it.First.IsTheSameWordAs(it.Second));

   /// <summary>How <paramref name="actual"/>'s displayed words differ from this breakdown's, the expected ones, a line per difference. Empty when they don't.</summary>
   public IReadOnlyList<string> HowTheDisplayedWordsDiffer(BreakdownUnderOptions actual)
   {
      var displayedButNotExpected = WordsWithoutTheSameWordIn(actual.DisplayedWords, DisplayedWords);
      var expectedButNotDisplayed = WordsWithoutTheSameWordIn(DisplayedWords, actual.DisplayedWords);
      if(displayedButNotExpected.Count == 0 && expectedButNotDisplayed.Count == 0 && !IsTheSameAs(actual))
         return [$"the breakdown {OptionsDescription} displays the expected words in another order: {string.Join("; ", actual.DisplayedWords)}"];

      return [.. displayedButNotExpected.Select(it => $"the breakdown {OptionsDescription} displays, but isn't expected to: {it}"),
              .. expectedButNotDisplayed.Select(it => $"the breakdown {OptionsDescription} is expected to display, but doesn't: {it}")];
   }

   /// <summary>The words of <paramref name="words"/> the same as none of <paramref name="otherWords"/>, each of which is the same as one at most.</summary>
   static List<BreakdownWord> WordsWithoutTheSameWordIn(IEnumerable<BreakdownWord> words, IEnumerable<BreakdownWord> otherWords)
   {
      var otherWordsNotYetMatched = otherWords.ToList();
      var wordsWithoutTheSame = new List<BreakdownWord>();
      foreach(var word in words)
      {
         var indexOfTheSameWord = otherWordsNotYetMatched.FindIndex(it => it.IsTheSameWordAs(word));
         if(indexOfTheSameWord < 0) wordsWithoutTheSame.Add(word);
         else otherWordsNotYetMatched.RemoveAt(indexOfTheSameWord);
      }

      return wordsWithoutTheSame;
   }
}
