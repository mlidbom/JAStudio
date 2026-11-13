using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.MeCab.Comparing;

/// <summary>
/// A difference in where two tokenizer dictionaries cut a stretch of text into tokens (<see cref="AlignedSpan"/>), as the tokens' surfaces<br/>
/// separated by <c>|</c>: IPADIC's <c>ひとつ</c> against UniDic's <c>ひと|つ</c>.
/// </summary>
/// <param name="FirstCut">The first dictionary's tokens.</param>
/// <param name="FirstTokens">How many tokens the first dictionary cut the stretch into.</param>
/// <param name="SecondCut">The second dictionary's tokens.</param>
/// <param name="SecondTokens">How many tokens the second dictionary cut the stretch into.</param>
public sealed record CuttingDifference(string FirstCut, int FirstTokens, string SecondCut, int SecondTokens)
{
   /// <summary>The difference in where <paramref name="span"/>'s two dictionaries cut it.</summary>
   public static CuttingDifference In(AlignedSpan span) => new(CutOf(span.First), span.First.Count, CutOf(span.Second), span.Second.Count);

   /// <summary>Whether the second dictionary cut the first's one token into several.</summary>
   public bool SecondCutsFiner => FirstTokens == 1;

   /// <summary>Whether the second dictionary cut the first's several tokens as one.</summary>
   public bool SecondCutsCoarser => SecondTokens == 1;

   static string CutOf(IEnumerable<RawMeCabToken> tokens) => string.Join('|', tokens.Select(it => it.Surface));
}
