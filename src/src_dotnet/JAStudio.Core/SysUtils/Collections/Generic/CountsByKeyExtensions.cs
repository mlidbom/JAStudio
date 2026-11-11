using System.Collections.Generic;

namespace JAStudio.Core.SysUtils.Collections.Generic;

/// <summary>A dictionary of counts, each key's count of how often it was found.</summary>
static class CountsByKeyExtensions
{
   extension<TKey>(Dictionary<TKey, int> @this) where TKey : notnull
   {
      /// <summary>Counts <paramref name="key"/> once more: its count, 0 when it has none yet, goes up by one.</summary>
      internal void CountOneMore(TKey key) => @this[key] = @this.GetValueOrDefault(key) + 1;
   }
}
