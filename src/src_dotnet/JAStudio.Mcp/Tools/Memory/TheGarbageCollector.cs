using System;
using System.Runtime;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>The .NET garbage collector of this process.</summary>
public static class TheGarbageCollector
{
   /// <summary>The mode the garbage collector of this process runs in.</summary>
   public static GarbageCollectorMode Mode => GCSettings.IsServerGC ? GarbageCollectorMode.Server : GarbageCollectorMode.Workstation;

   /// <summary>
   /// Runs the most thorough collection there is: blocking, compacting, the large object heap included, of every generation, and aggressive,<br/>
   /// so the garbage collector gives the operating system back all the memory it can.
   /// </summary>
   public static void CollectAllGarbage()
   {
      GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
      GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
   }
}
