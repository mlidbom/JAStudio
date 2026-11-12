using System;
using System.Diagnostics;
using JAStudio.Mcp.Tools.Memory;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>
/// How long one pass of parsing over every sentence took, how much of that time the garbage collector held the threads still, and the most<br/>
/// memory the process used meanwhile. While the garbage collector runs a blocking collection, every thread that allocates waits for it, so its<br/>
/// pauses are time no thread parses.
/// </summary>
/// <param name="Seconds">How long the pass took.</param>
/// <param name="SecondsPausedByTheGarbageCollector">How long, of <paramref name="Seconds"/>, the garbage collector paused the process's threads.</param>
/// <param name="GarbageCollections">How many collections the garbage collector ran during the pass, of every generation.</param>
/// <param name="PeakMemoryUse">The most memory the process used during the pass.</param>
public record TimedPass(double Seconds, double SecondsPausedByTheGarbageCollector, int GarbageCollections, PeakMemoryUse PeakMemoryUse)
{
   /// <summary>Runs <paramref name="pass"/>, and times it and samples the memory meanwhile.</summary>
   public static TimedPass Of(Action pass)
   {
      var garbageCollectionsBefore = GC.CollectionCount(0);
      var pausedByTheGarbageCollectorBefore = GC.GetTotalPauseDuration();
      var stopwatch = Stopwatch.StartNew();
      var peakMemoryUse = PeakMemoryUse.During(pass);
      return new TimedPass(stopwatch.Elapsed.TotalSeconds,
                           (GC.GetTotalPauseDuration() - pausedByTheGarbageCollectorBefore).TotalSeconds,
                           GC.CollectionCount(0) - garbageCollectionsBefore,
                           peakMemoryUse);
   }
}
