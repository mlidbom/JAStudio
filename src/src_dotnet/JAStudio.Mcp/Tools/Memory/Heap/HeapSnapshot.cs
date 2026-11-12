using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;
using Microsoft.Diagnostics.Runtime;

namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>
/// A snapshot of this process's managed heap, read with ClrMD, the library <c>dotnet-dump</c>'s heap commands are built on: every object<br/>
/// on the heap, with its type, its size and the objects it references.
/// </summary>
/// <remarks>
/// The snapshot holds the dead objects too, those no collection has freed yet, so it's taken after collecting all garbage: its objects<br/>
/// are the live ones. A snapshot is a copy of the process's memory that the operating system makes as the pages change, so the process<br/>
/// goes on as it's read.
/// </remarks>
/// <remarks>
/// The snapshot can catch memory that reads as an object but isn't one, or isn't finished: at the end of a region of the heap where objects<br/>
/// are being made, ClrMD read "strings" of length -1, and one 4.8 GB long, and <see cref="ClrObject.IsValid"/> took them as valid<br/>
/// (2026-11-12). So an object can be read only if it ends within its region and, for a string, its length isn't negative.<br/>
/// Some snapshots hold thousands of objects that can't be read, and then counts made from them are off by up to 1%: ClrMD's walk of a<br/>
/// region skips objects, or reads one twice. In the specifications' process, after 300 specifications had run, now and then a snapshot<br/>
/// couldn't read 2,000 to 3,000 objects, at the ends of generation 2's regions, and counted 987 to 1002 of 1001 strings, though nothing else<br/>
/// ran meanwhile. With background garbage collection turned off, it never did. Neither a no-GC region around the snapshot, with no<br/>
/// collection induced meanwhile, nor taking the snapshot again, up to three times, helped: the next snapshot held as many (2026-11-12).<br/>
/// On the headless JAStudio, the snapshot read every object, and matched <c>dotnet-dump</c>'s count.
/// </remarks>
sealed partial class HeapSnapshot : IDisposable
{
   /// <summary>
   /// Makes the reading of a snapshot wait for the one being read: two at once would count each other's work, and five at once, each<br/>
   /// collecting garbage and taking snapshots meanwhile, hung the specifications. What a reading still holds when the next one collects<br/>
   /// garbage is counted by it: a census's 2.4 million strings with the rest (2026-11-12). So the reading describes what it found before<br/>
   /// it lets the next one start.
   /// </summary>
   static readonly IMonitor OneAtATime = IMonitor.New(LockTimeout.Minutes(10));

   readonly DataTarget _snapshot;
   readonly ClrRuntime _runtime;

   HeapSnapshot()
   {
      _snapshot = DataTarget.CreateSnapshotAndAttach(Environment.ProcessId);
      _runtime = _snapshot.ClrVersions.Single().CreateRuntime();
   }

   /// <summary>
   /// What <paramref name="read"/> reads from a snapshot of this process's heap, taken after collecting all garbage, once every snapshot<br/>
   /// taken before has been read: such as a census of the heap, described.
   /// </summary>
   public static TDescription ReadAfterCollectingAllGarbage<TDescription>(Func<HeapSnapshot, TDescription> read) =>
      OneAtATime.Locked(() =>
      {
         TheGarbageCollector.CollectAllGarbage();
         using var snapshot = new HeapSnapshot();
         return read(snapshot);
      });

   /// <summary>Every object the walk of the heap finds, region by region, each with whether it can be read.</summary>
   public IEnumerable<FoundObject> Objects() =>
      from region in _runtime.Heap.Segments
      from heapObject in region.EnumerateObjects(carefully: true)
      where !heapObject.IsFree
      select new FoundObject(heapObject, IsAnObjectThatCanBeRead(heapObject, region));

   bool IsAnObjectThatCanBeRead(ClrObject heapObject, ClrSegment region) =>
      heapObject.IsValid
   && heapObject.Address + heapObject.Size <= region.ObjectRange.End
   && !(heapObject.Type!.IsString && TheLengthOfTheString(heapObject) < 0);

   /// <summary>The length a string's object says it has: the <see cref="int"/> after its type's address, as the runtime lays strings out.</summary>
   int TheLengthOfTheString(ClrObject heapObject) => _snapshot.DataReader.Read<int>(heapObject.Address + (ulong)_snapshot.DataReader.PointerSize);

   public void Dispose()
   {
      _runtime.Dispose();
      _snapshot.Dispose();
   }
}
