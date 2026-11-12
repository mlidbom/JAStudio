using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>What <see cref="MemoryTools.DescribeMemory"/> reports: how the garbage collector is configured, and how much memory the process uses.</summary>
/// <param name="GarbageCollectorMode">The mode the garbage collector runs in.</param>
/// <param name="GarbageCollectorConfiguration">Every setting of the garbage collector, by the runtime's names for them, such as its heap count.</param>
/// <param name="MemoryUse">The memory the process uses, as the call found it.</param>
/// <param name="MemoryUseAfterCollectingGarbage">
/// The memory the process uses after the most thorough collection there is, when one was asked for: what of <paramref name="MemoryUse"/> the process<br/>
/// keeps when it has to give back all it can.
/// </param>
public record MemoryDescription(
   GarbageCollectorMode GarbageCollectorMode,
   IReadOnlyDictionary<string, object> GarbageCollectorConfiguration,
   MemoryUse MemoryUse,
   MemoryUse? MemoryUseAfterCollectingGarbage);
