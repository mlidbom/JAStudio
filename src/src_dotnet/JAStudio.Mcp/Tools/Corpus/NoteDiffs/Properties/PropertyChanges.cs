using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.Properties;

/// <summary>
/// How two versions of a note differ, property by property, as the corpus stores them: each property whose value changed, and each list that<br/>
/// items were added to or removed from. Objects are compared property by property, at any depth; lists by their items.
/// </summary>
public class PropertyChanges
{
   readonly List<ValueChange> _valuesChanged = [];
   readonly List<ListChange> _listsChanged = [];
   readonly IReadOnlySet<string> _propertiesComparedElsewhere;

   PropertyChanges(IReadOnlySet<string> propertiesComparedElsewhere) => _propertiesComparedElsewhere = propertiesComparedElsewhere;

   public IReadOnlyList<ValueChange> ValuesChanged => _valuesChanged;
   public IReadOnlyList<ListChange> ListsChanged => _listsChanged;

   /// <summary>
   /// Compares <paramref name="before"/> with <paramref name="after"/>, two versions of a note as the corpus stores it, but for the properties<br/>
   /// <paramref name="propertiesComparedElsewhere"/>, by their paths, which the caller compares in a way of its own.
   /// </summary>
   public static PropertyChanges Between(JsonObject before, JsonObject after, IReadOnlySet<string> propertiesComparedElsewhere)
   {
      var changes = new PropertyChanges(propertiesComparedElsewhere);
      changes.Compare("", before, after);
      return changes;
   }

   void Compare(string property, JsonNode? before, JsonNode? after)
   {
      if(_propertiesComparedElsewhere.Contains(property) || JsonNode.DeepEquals(before, after)) return;

      if(before is JsonObject objectBefore && after is JsonObject objectAfter)
      {
         foreach(var name in objectBefore.Select(it => it.Key).Union(objectAfter.Select(it => it.Key)))
            Compare(PathOf(name), objectBefore[name], objectAfter[name]);
      } else if(before is JsonArray listBefore && after is JsonArray listAfter && ItemsAddedOrRemoved(listBefore, listAfter) is {} listChange)
      {
         _listsChanged.Add(listChange);
      } else
      {
         _valuesChanged.Add(new ValueChange(property, before, after));
      }

      return;

      string PathOf(string name) => property.Length == 0 ? name : $"{property}.{name}";

      ListChange? ItemsAddedOrRemoved(JsonArray listBefore, JsonArray listAfter)
      {
         var added = ItemsWithoutAnEqualItemIn(listAfter, listBefore);
         var removed = ItemsWithoutAnEqualItemIn(listBefore, listAfter);
         return added.Count == 0 && removed.Count == 0 ? null : new ListChange(property, added, removed);
      }
   }

   /// <summary>The items of <paramref name="items"/> that no item of <paramref name="otherItems"/> equals, each item of <paramref name="otherItems"/> equaling at most one, so that a list with an item twice differs from one with it once.</summary>
   static List<JsonNode?> ItemsWithoutAnEqualItemIn(JsonArray items, JsonArray otherItems)
   {
      var otherItemsNotYetEqualed = otherItems.ToList();
      var itemsWithoutAnEqual = new List<JsonNode?>();
      foreach(var item in items)
      {
         var indexOfAnEqualItem = otherItemsNotYetEqualed.FindIndex(it => JsonNode.DeepEquals(it, item));
         if(indexOfAnEqualItem < 0) itemsWithoutAnEqual.Add(item);
         else otherItemsNotYetEqualed.RemoveAt(indexOfAnEqualItem);
      }

      return itemsWithoutAnEqual;
   }
}
