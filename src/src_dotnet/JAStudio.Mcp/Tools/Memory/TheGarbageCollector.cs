using System.Runtime;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>The .NET garbage collector of this process.</summary>
public static class TheGarbageCollector
{
   /// <summary>The mode the garbage collector of this process runs in.</summary>
   public static GarbageCollectorMode Mode => GCSettings.IsServerGC ? GarbageCollectorMode.Server : GarbageCollectorMode.Workstation;
}
