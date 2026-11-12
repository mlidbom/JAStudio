using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>
/// A count of every object on this process's managed heap: by type, how many objects and bytes, and for the strings, by value, how many<br/>
/// copies. Made from a <see cref="HeapSnapshot"/>, as <c>dotnet-dump</c>'s <c>dumpheap -stat</c> and <c>dumpheap -strings</c> count a dump's.
/// </summary>
sealed class HeapCensus
{
   /// <summary>The longest string compared by its value: a longer one is counted with the strings, but its copies aren't looked for.</summary>
   const int LongestStringCompared = 4096;

   readonly Dictionary<string, ObjectTally> _types = new(StringComparer.Ordinal);
   readonly Dictionary<string, ObjectTally> _stringsByValue = new(StringComparer.Ordinal);
   readonly ObjectTally _everything = new();
   readonly ObjectTally _strings = new();
   long _objectsThatCouldNotBeRead;

   HeapCensus() {}

   /// <summary>
   /// What this process's managed heap holds after collecting all garbage, so the live objects, as <see cref="MemoryTools.DescribeWhatTheHeapHolds"/><br/>
   /// reports it.
   /// </summary>
   public static WhatTheHeapHolds DescribedForThisProcessAfterCollectingAllGarbage(int maxTypesListed, string? typesNamed, int maxRepeatedStringsListed) =>
      HeapSnapshot.ReadAfterCollectingAllGarbage(snapshot => Of(snapshot).Described(maxTypesListed, typesNamed, maxRepeatedStringsListed));

   static HeapCensus Of(HeapSnapshot snapshot)
   {
      var census = new HeapCensus();
      foreach(var found in snapshot.Objects())
      {
         if(found.CanBeRead) census.Count(found);
         else census._objectsThatCouldNotBeRead++;
      }
      return census;
   }

   void Count(HeapSnapshot.FoundObject found)
   {
      var bytes = (long)found.Object.Size;
      _everything.Add(bytes);
      TallyOf(_types, found.TypeName).Add(bytes);
      if(!found.Object.Type!.IsString) return;

      _strings.Add(bytes);
      if(found.Object.AsString(LongestStringCompared + 1) is { Length: <= LongestStringCompared } value)
         TallyOf(_stringsByValue, value).Add(bytes);
   }

   WhatTheHeapHolds Described(int maxTypesListed, string? typesNamed, int maxRepeatedStringsListed) =>
      new(Megabytes.Of(_everything.Bytes),
          _everything.Objects,
          _objectsThatCouldNotBeRead,
          [.. _types.Where(it => typesNamed == null || it.Key.Contains(typesNamed, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(it => it.Value.Bytes)
                    .Take(maxTypesListed)
                    .Select(it => new HeapType(it.Key, it.Value.Objects, Megabytes.Of(it.Value.Bytes), Percent(it.Value.Bytes)))],
          new StringsOnTheHeap(_strings.Objects,
                               Megabytes.Of(_strings.Bytes),
                               _stringsByValue.Count,
                               Megabytes.Of(_stringsByValue.Values.Sum(it => it.BytesOfTheCopies)),
                               [.. _stringsByValue.Where(it => it.Value.Objects > 1)
                                                  .OrderByDescending(it => it.Value.BytesOfTheCopies)
                                                  .Take(maxRepeatedStringsListed)
                                                  .Select(it => new RepeatedString(RepeatedString.Shown(it.Key), it.Value.Objects, Megabytes.Of(it.Value.BytesOfTheCopies)))]));

   double Percent(long bytes) => Math.Round(100.0 * bytes / _everything.Bytes, 2);

   static ObjectTally TallyOf(Dictionary<string, ObjectTally> tallies, string key)
   {
      ref var tally = ref CollectionsMarshal.GetValueRefOrAddDefault(tallies, key, out _);
      return tally ??= new ObjectTally();
   }

   /// <summary>How many objects of one kind the heap holds, and how many bytes they take.</summary>
   sealed class ObjectTally
   {
      public long Objects { get; private set; }
      public long Bytes { get; private set; }

      /// <summary>For strings of one value: the bytes all of them but one take, which sharing one string would save.</summary>
      public long BytesOfTheCopies => Bytes - Bytes / Objects;

      public void Add(long bytes)
      {
         Objects++;
         Bytes += bytes;
      }
   }
}
