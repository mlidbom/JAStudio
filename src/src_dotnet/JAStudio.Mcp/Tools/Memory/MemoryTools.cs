using System;
using System.ComponentModel;
using System.Runtime;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>The tools about the memory of the JAStudio process, for telling what a change, such as to the garbage collector's mode, costs in memory.</summary>
[McpServerToolType]
public class MemoryTools
{
   [McpServerTool(Name = "describe_memory", Idempotent = false)]
   [Description("""
                Describes the memory of the JAStudio process: the garbage collector's mode, workstation or server, and every setting of it; how
                much the managed heap holds and how much the garbage collector has committed; and the process's working set and private memory.
                With alsoAfterCollectingGarbage, it then runs the most thorough collection there is, a blocking, compacting, aggressive one of
                every generation that gives the operating system back all the memory it can, and describes the memory again: what the process
                keeps when it must give back all it can, as against what it held only because nothing made it collect.
                """)]
   public static MemoryDescription DescribeMemory(
      [Description("True to also collect all garbage and describe the memory after it.")] bool alsoAfterCollectingGarbage = false)
   {
      var memoryUse = MemoryUse.Now();
      return new MemoryDescription(TheGarbageCollector.Mode,
                                   GC.GetConfigurationVariables(),
                                   memoryUse,
                                   alsoAfterCollectingGarbage ? MemoryUseAfterCollectingAllGarbage() : null);
   }

   static MemoryUse MemoryUseAfterCollectingAllGarbage()
   {
      GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
      GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
      return MemoryUse.Now();
   }
}
