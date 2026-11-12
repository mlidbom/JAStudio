using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.SysUtils.Collections.Generic;

/// <summary>The subsets of a list: every way of choosing some of its items, none or all of them among them.</summary>
static class SubsetsExtensions
{
   extension<T>(IReadOnlyList<T> @this)
   {
      /// <summary>
      /// Every subset of these items, each with its items in their order, the empty one and the one with all of them among them: <c>2^n</c> for <c>n</c><br/>
      /// items. In the order counting in binary gives, the first item the lowest bit: <c>[A, B, C]</c> gives <c>[]</c>, <c>[A]</c>, <c>[B]</c>,<br/>
      /// <c>[A, B]</c>, <c>[C]</c>, <c>[A, C]</c>, <c>[B, C]</c> and <c>[A, B, C]</c>.
      /// </summary>
      internal IReadOnlyList<IReadOnlyList<T>> Subsets()
      {
         List<IReadOnlyList<T>> subsets = [[]];
         foreach(var item in @this)
            subsets.AddRange([.. subsets.Select(IReadOnlyList<T> (it) => [.. it, item])]);

         return subsets;
      }
   }
}
