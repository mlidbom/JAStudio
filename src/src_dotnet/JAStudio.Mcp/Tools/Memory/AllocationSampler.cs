using System;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Linq;
using Compze.Internals.SystemCE;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>
/// Samples what the process allocates, by type, from when it's made until <see cref="StopSampling"/>: the runtime reports an allocation about<br/>
/// every 100 KB each heap allocates, with the type of the object it was allocating then and the bytes allocated since its last report. Summed<br/>
/// by type, the reports show which types take most of what's allocated, which a profiler's times don't.
/// </summary>
/// <remarks>
/// A sample, not a count: the share of a type allocated often is right, and a type allocated rarely may be missed or counted for more than it<br/>
/// took. It samples every thread of the process, not only the code under study.
/// </remarks>
/// <remarks>
/// It listens to the runtime's own events in the process, through an <see cref="EventListener"/>: the garbage collector's <c>GCAllocationTick</c>.
/// </remarks>
sealed class AllocationSampler : EventListener
{
   /// <summary>The runtime's own event source, which reports the garbage collector's events.</summary>
   const string TheRuntimesEventSource = "Microsoft-Windows-DotNETRuntime";

   /// <summary>The runtime's keyword for the garbage collector's events, among them the allocation reports.</summary>
   const EventKeywords TheGarbageCollectorsEvents = (EventKeywords)0x1;

   readonly ConcurrentDictionary<string, long> _bytesByType = new();

   /// <summary>Stops sampling, and sums up what was sampled, the <paramref name="maxTypesListed"/> types that took the most listed first.</summary>
   public SampledAllocations StopSampling(int maxTypesListed)
   {
      Dispose();
      var sampledBytes = _bytesByType.Values.Sum();
      return new SampledAllocations(Megabytes(sampledBytes),
                                    [.. _bytesByType.OrderByDescending(it => it.Value)
                                                    .Take(maxTypesListed)
                                                    .Select(it => new AllocatedType(it.Key, Megabytes(it.Value), Math.Round(100.0 * it.Value / sampledBytes, 1)))]);
   }

   protected override void OnEventSourceCreated(EventSource eventSource)
   {
      if(eventSource.Name == TheRuntimesEventSource) EnableEvents(eventSource, EventLevel.Verbose, TheGarbageCollectorsEvents);
   }

   protected override void OnEventWritten(EventWrittenEventArgs runtimeEvent)
   {
      if(runtimeEvent.EventName?.StartsWithOrdinal("GCAllocationTick") != true) return;

      var type = (string?)PayloadValue("TypeName") ?? "(no type named)";
      var bytesSinceTheLastReport = Convert.ToInt64(PayloadValue("AllocationAmount64"));
      _bytesByType.AddOrUpdate(type, bytesSinceTheLastReport, (_, bytes) => bytes + bytesSinceTheLastReport);

      object? PayloadValue(string name) => runtimeEvent.Payload![runtimeEvent.PayloadNames!.IndexOf(name)];
   }

   static double Megabytes(long bytes) => Math.Round(bytes / 1_000_000.0, 1);
}
