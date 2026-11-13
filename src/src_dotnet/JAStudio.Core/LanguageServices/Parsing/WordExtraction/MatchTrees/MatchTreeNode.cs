using System.Collections.Generic;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.MatchTrees;

/// <summary>
/// A match at the top of a sentence's <see cref="MatchTree"/>: a match shown, with the matches not shown that it <paramref name="Covered"/>, or a match not<br/>
/// shown that no shown match covers, with none.
/// </summary>
/// <param name="Match">The match.</param>
/// <param name="IsShown">Whether the <paramref name="Match"/> is shown, rather than being one no shown match covers.</param>
/// <param name="Covered">
/// The matches not shown of the words that start in the shown <paramref name="Match"/>'s place: its own word's other matches, and those of the words it<br/>
/// shadows, in sentence order, the longer word first where two start at one place.
/// </param>
public sealed record MatchTreeNode(Match Match, bool IsShown, IReadOnlyList<Match> Covered);
