using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Compze.Threading;
using Microsoft.Diagnostics.Runtime;

namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>
/// A count of every object on this process's managed heap: by type, how many objects and bytes, and for the strings, by value, how many<br/>
/// copies. Made by reading a snapshot of the process with ClrMD, the library <c>dotnet-dump</c>'s heap commands are built on, as<br/>
/// <c>dumpheap -stat</c> and <c>dumpheap -strings</c> count a dump's.
/// </summary>
/// <remarks>
/// The snapshot holds the dead objects too, those no collection has freed yet: collect all garbage first for a count of the live ones.<br/>
/// A snapshot is a copy of the process's memory that the operating system makes as the pages change, so the process goes on as it's read.
/// </remarks>
/// <remarks>
/// The snapshot can catch memory that reads as an object but isn't one, or isn't finished: at the end of a region of the heap where objects<br/>
/// are being made, ClrMD read "strings" of length -1, and one 4.8 GB long, and <see cref="ClrObject.IsValid"/> took them as valid<br/>
/// (2026-11-12). So an object counts only if it ends within its region and, for a string, its length isn't negative. The others are<br/>
/// counted as <see cref="WhatTheHeapHolds.ObjectsThatCouldNotBeRead"/>, and left out of the rest.
/// </remarks>
/// <remarks>
/// Some snapshots hold thousands of such objects, and then the counts are off by up to 1%: ClrMD's walk of a region skips objects, or reads<br/>
/// one twice. In the specifications' process, after 300 specifications had run, now and then a snapshot couldn't read 2,000 to 3,000<br/>
/// objects, at the ends of generation 2's regions, and counted 987 to 1002 of 1001 strings, though nothing else ran meanwhile. With background<br/>
/// garbage collection turned off, it never did. Neither a no-GC region around the snapshot, with no collection induced meanwhile, nor taking<br/>
/// the snapshot again, up to three times, helped: the next snapshot held as many (2026-11-12). On the headless JAStudio, the snapshot read<br/>
/// every object, and matched <c>dotnet-dump</c>'s count.
/// </remarks>
sealed class HeapCensus
{
   /// <summary>The longest string compared by its value: a longer one is counted with the strings, but its copies aren't looked for.</summary>
   const int LongestStringCompared = 4096;

   /// <summary>
   /// Makes a census wait for the one being taken and described: two at once would count each other's work, and five at once, each collecting<br/>
   /// garbage and taking snapshots meanwhile, hung the specifications. A census still alive when the next one collects garbage is counted by<br/>
   /// it, its 2.4 million strings with the rest (2026-11-12).
   /// </summary>
   static readonly IMonitor OneCensusAtATime = IMonitor.New(LockTimeout.Minutes(10));

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
      OneCensusAtATime.Locked(() => OfThisProcessAfterCollectingAllGarbage().Described(maxTypesListed, typesNamed, maxRepeatedStringsListed));

   static HeapCensus OfThisProcessAfterCollectingAllGarbage()
   {
      TheGarbageCollector.CollectAllGarbage();
      var census = new HeapCensus();
      using var snapshot = DataTarget.CreateSnapshotAndAttach(Environment.ProcessId);
      using var runtime = snapshot.ClrVersions.Single().CreateRuntime();
      foreach(var region in runtime.Heap.Segments)
      foreach(var heapObject in region.EnumerateObjects(carefully: true).Where(it => !it.IsFree))
      {
         if(IsAnObjectThatCanBeRead(heapObject, region, snapshot.DataReader)) census.Count(heapObject);
         else census._objectsThatCouldNotBeRead++;
      }
      return census;
   }

   static bool IsAnObjectThatCanBeRead(ClrObject heapObject, ClrSegment region, IDataReader memory) =>
      heapObject.IsValid
   && heapObject.Address + heapObject.Size <= region.ObjectRange.End
   && !(heapObject.Type!.IsString && TheLengthOfTheString(heapObject, memory) < 0);

   /// <summary>The length a string's object says it has: the <see cref="int"/> after its type's address, as the runtime lays strings out.</summary>
   static int TheLengthOfTheString(ClrObject heapObject, IDataReader memory) => memory.Read<int>(heapObject.Address + (ulong)memory.PointerSize);

   void Count(ClrObject heapObject)
   {
      var bytes = (long)heapObject.Size;
      var type = heapObject.Type!;
      _everything.Add(bytes);
      TallyOf(_types, type.Name ?? "(a type with no name)").Add(bytes);
      if(!type.IsString) return;

      _strings.Add(bytes);
      if(heapObject.AsString(LongestStringCompared + 1) is { Length: <= LongestStringCompared } value)
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
