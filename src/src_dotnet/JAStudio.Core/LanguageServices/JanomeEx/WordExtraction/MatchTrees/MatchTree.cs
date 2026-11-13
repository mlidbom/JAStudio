using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.MatchTrees;

/// <summary>
/// A sentence's matches arranged by which shown match covers each, so that how the sentence parsed shows at a glance: the matches shown, in sentence<br/>
/// order, each with the matches not shown under it (<see cref="MatchTreeNode.Covered"/>), and the matches no shown match covers at their own place.<br/>
/// What a sentence's Parsing tab shows. See <c>dev_docs/jastudio-workspace.md</c>, "Edit mode becomes the sentence's Parsing tab".
/// </summary>
/// <remarks>
/// A shown match covers the not-shown matches of the words that start in its place: its own word's other matches, such as another vocab with the same<br/>
/// form, those of the words it shadows, which start after it within it or at its start and are no longer, and their other variants'. A longer word<br/>
/// that starts where a shown one does, such as a compound hidden by a setting, isn't covered by it: it stands at its own place, before it.<br/>
/// <br/>
/// The matches are those the sentence breakdown lists in edit mode: every match of the analysis's indexing variants (<see cref="TextAnalysis.IndexingWordVariants"/>).
/// </remarks>
public sealed class MatchTree
{
   MatchTree(IReadOnlyList<MatchTreeNode> roots) => Roots = roots;

   /// <summary>The matches shown, in sentence order, and the matches no shown match covers, each at its place, before a shown match starting there.</summary>
   public IReadOnlyList<MatchTreeNode> Roots { get; }

   /// <summary>The matches of <paramref name="analysis"/> arranged as a tree.</summary>
   public static MatchTree Of(TextAnalysis analysis)
   {
      var matches = analysis.IndexingWordVariants.SelectMany(it => it.Matches).ToList();
      var shownMatches = analysis.DisplayMatches.ToHashSet();
      var coveredByTheShownMatch = matches.Where(it => !shownMatches.Contains(it))
                                          .Select(it => (Match: it, CoveredBy: TheShownMatchCovering(it, shownMatches)))
                                          .Where(it => it.CoveredBy is not null)
                                          .ToLookup(it => it.CoveredBy!, it => it.Match);

      return new MatchTree([.. matches.Where(it => shownMatches.Contains(it) || TheShownMatchCovering(it, shownMatches) is null)
                                      .Select(it => new MatchTreeNode(it, shownMatches.Contains(it), [.. coveredByTheShownMatch[it]]))]);
   }

   /// <summary>
   /// The shown match among <paramref name="shownMatches"/> that covers <paramref name="match"/>, a match not shown, or none: the one whose word spans<br/>
   /// the token <paramref name="match"/>'s word starts at, the latest starting if two do, unless <paramref name="match"/>'s word starts where it does and<br/>
   /// is longer.
   /// </summary>
   static Match? TheShownMatchCovering(Match match, IReadOnlyCollection<Match> shownMatches)
   {
      var word = match.Word;
      var start = word.StartLocation.TokenIndex;
      var covering = shownMatches.Where(it => it.Word.StartLocation.TokenIndex <= start && start <= it.Word.EndLocation.TokenIndex)
                                 .OrderByDescending(it => it.Word.StartLocation.TokenIndex)
                                 .FirstOrDefault();
      return covering is not null && IsLongerAndStartsWhere(word, covering.Word) ? null : covering;

      static bool IsLongerAndStartsWhere(CandidateWord word, CandidateWord shownWord) =>
         word.StartLocation.TokenIndex == shownWord.StartLocation.TokenIndex && word.LocationCount > shownWord.LocationCount;
   }
}
