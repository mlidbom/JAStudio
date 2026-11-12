namespace JAStudio.Mcp.Tools.Memory;

/// <summary>
/// The .NET garbage collector's two modes, chosen per process when the runtime starts, by <c>System.GC.Server</c> in its <c>runtimeconfig.json</c>.
/// </summary>
public enum GarbageCollectorMode
{
   /// <summary>One heap, with small allocation budgets: little memory, and frequent collections when many threads allocate, each of which pauses them all.</summary>
   Workstation,

   /// <summary>
   /// Heaps that each have a collection thread of their own, and larger allocation budgets: fewer collections when many threads allocate, more memory.<br/>
   /// Since .NET 9, DATAS adapts the number of heaps and the budgets to the load and to the size of the long-lived data, unless it's turned off.
   /// </summary>
   Server
}
