using System.Collections.Generic;
using Compze.Internals.SystemCE.LinqCE;

namespace JAStudio.Core.SysUtils.Collections.Generic;

static class SetDictionaryExtensions
{
   extension<TKey, TItem>(IDictionary<TKey, HashSet<TItem>> @this)
   {
      internal void RemoveFromSet(TKey key, TItem item)
      {
         if(@this.TryGetValue(key, out var set))
         {
            set.Remove(item);
         }
      }

      internal void RemoveFromSets(IEnumerable<TKey> keys, TItem item) =>
         keys.ForEach(key => @this.RemoveFromSet(key, item));

      internal void AddToSet(TKey key, TItem item)
      {
         if(!@this.TryGetValue(key, out var set))
         {
            set = [];
            @this[key] = set;
         }

         set.Add(item);
      }

      internal void AddToSets(IEnumerable<TKey> keys, TItem item) =>
         keys.ForEach(key => @this.AddToSet(key, item));
   }
}
