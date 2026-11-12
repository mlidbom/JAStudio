using System;
using System.Diagnostics;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>How much memory this process uses, in megabytes, as the garbage collector and the operating system count it.</summary>
/// <param name="AllocatedOnTheManagedHeapMegabytes">
/// What the garbage collector counts as allocated on its heap: the live objects, and the garbage it hasn't collected yet.
/// </param>
/// <param name="CommittedByTheGarbageCollectorMegabytes">
/// The memory the garbage collector had committed for its heap at the end of its last collection: the allocated objects, and room it keeps for<br/>
/// allocations to come.
/// </param>
/// <param name="WorkingSetMegabytes">The process's memory that is in physical memory, as the operating system counts it, shared pages included.</param>
/// <param name="PrivateMegabytes">The memory committed for the process alone, as the operating system counts it.</param>
public record MemoryUse(double AllocatedOnTheManagedHeapMegabytes, double CommittedByTheGarbageCollectorMegabytes, double WorkingSetMegabytes, double PrivateMegabytes)
{
   /// <summary>The memory this process uses at this moment.</summary>
   public static MemoryUse Now()
   {
      using var process = Process.GetCurrentProcess();
      return new MemoryUse(Megabytes.Of(GC.GetTotalMemory(forceFullCollection: false)),
                           Megabytes.Of(GC.GetGCMemoryInfo().TotalCommittedBytes),
                           Megabytes.Of(process.WorkingSet64),
                           Megabytes.Of(process.PrivateMemorySize64));
   }
}
