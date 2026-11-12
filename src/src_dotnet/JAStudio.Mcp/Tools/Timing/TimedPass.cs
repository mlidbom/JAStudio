using System;
using System.Diagnostics;
using JAStudio.Mcp.Tools.Memory;

namespace JAStudio.Mcp.Tools.Timing;

/// <summary>
/// How long one pass of parsing over every sentence took, how much processor time it and the machine's other processes used, how much of the<br/>
/// time the garbage collector held the threads still, how much memory the pass allocated, and the most memory the process used meanwhile.<br/>
/// While the garbage collector runs a blocking collection, every thread that allocates waits for it, so its pauses are time no thread parses.
/// </summary>
/// <param name="Seconds">How long the pass took.</param>
/// <param name="ProcessorTime">
/// The processor time the pass used, and the machine's other processes meanwhile: whether other work took cores from the pass.
/// </param>
/// <param name="SecondsPausedByTheGarbageCollector">How long, of <paramref name="Seconds"/>, the garbage collector paused the process's threads.</param>
/// <param name="GarbageCollections">How many collections the garbage collector ran during the pass, of every generation.</param>
/// <param name="AllocatedMegabytes">
/// How much memory the process allocated during the pass, nearly all of it the parsing's. Unlike the times, it's the same from run to run,<br/>
/// whatever else runs on the machine, so it shows a change to how much parsing allocates where the times can't.
/// </param>
/// <param name="PeakMemoryUse">The most memory the process used during the pass.</param>
public record TimedPass(
   double Seconds,
   ProcessorTimeUsed ProcessorTime,
   double SecondsPausedByTheGarbageCollector,
   int GarbageCollections,
   double AllocatedMegabytes,
   PeakMemoryUse PeakMemoryUse)
{
   /// <summary>Runs <paramref name="pass"/>, and times it and samples the memory meanwhile.</summary>
   public static TimedPass Of(Action pass)
   {
      var garbageCollectionsBefore = GC.CollectionCount(0);
      var pausedByTheGarbageCollectorBefore = GC.GetTotalPauseDuration();
      var allocatedBytesBefore = GC.GetTotalAllocatedBytes(precise: true);
      var processorTimeUsedSinceTheStart = ProcessorTimeUsed.MeasuringFromNow();
      var stopwatch = Stopwatch.StartNew();
      var peakMemoryUse = PeakMemoryUse.During(pass);
      return new TimedPass(stopwatch.Elapsed.TotalSeconds,
                           processorTimeUsedSinceTheStart(),
                           (GC.GetTotalPauseDuration() - pausedByTheGarbageCollectorBefore).TotalSeconds,
                           GC.CollectionCount(0) - garbageCollectionsBefore,
                           Math.Round((GC.GetTotalAllocatedBytes(precise: true) - allocatedBytesBefore) / (1024.0 * 1024.0), 1),
                           peakMemoryUse);
   }
}
