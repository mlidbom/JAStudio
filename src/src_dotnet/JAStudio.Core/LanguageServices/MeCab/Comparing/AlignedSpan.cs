using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;

namespace JAStudio.Core.LanguageServices.MeCab.Comparing;

/// <summary>
/// A stretch of a sentence that two tokenizer dictionaries both start and end tokens at, with the tokens each cut it into: the smallest such<br/>
/// stretches, so where both cut alike, a stretch is one token in each, and where they cut differently, it's the whole of the difference, such<br/>
/// as IPADIC's ひとつ and UniDic's ひと and つ.
/// </summary>
/// <param name="First">The first dictionary's tokens in the stretch.</param>
/// <param name="Second">The second dictionary's tokens in the stretch.</param>
public sealed record AlignedSpan(IReadOnlyList<RawMeCabToken> First, IReadOnlyList<RawMeCabToken> Second)
{
   /// <summary>Whether both dictionaries cut the stretch as one token, the same one.</summary>
   public bool IsCutAlike => First.Count == 1 && Second.Count == 1;

   /// <summary>The stretches of a sentence that <paramref name="first"/> and <paramref name="second"/>, its tokens as two dictionaries cut it, both start and end tokens at.</summary>
   public static IReadOnlyList<AlignedSpan> Of(IReadOnlyList<RawMeCabToken> first, IReadOnlyList<RawMeCabToken> second)
   {
      var spans = new List<AlignedSpan>();
      var firstIndex = 0;
      var secondIndex = 0;
      while(firstIndex < first.Count && secondIndex < second.Count)
      {
         var firstSpanStart = firstIndex;
         var secondSpanStart = secondIndex;
         var firstEnd = first[firstIndex++].End;
         var secondEnd = second[secondIndex++].End;
         while(firstEnd != secondEnd)
         {
            if(firstEnd < secondEnd) firstEnd = first[firstIndex++].End;
            else secondEnd = second[secondIndex++].End;
         }

         spans.Add(new AlignedSpan(TokensBetween(first, firstSpanStart, firstIndex), TokensBetween(second, secondSpanStart, secondIndex)));
      }

      Contract.Argument.Assert(firstIndex == first.Count && secondIndex == second.Count,
                               () => $"The two tokenizations don't end together: {string.Join(' ', first.Select(it => it.Surface))} and {string.Join(' ', second.Select(it => it.Surface))}");
      return spans;

      static RawMeCabToken[] TokensBetween(IReadOnlyList<RawMeCabToken> tokens, int start, int end) => [.. tokens.Skip(start).Take(end - start)];
   }
}
