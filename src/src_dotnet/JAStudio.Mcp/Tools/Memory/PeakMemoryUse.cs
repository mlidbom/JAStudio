using System;
using System.Threading;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>The most memory the process used while some work ran, in megabytes, as sampled every <see cref="TimeBetweenSamples"/>.</summary>
/// <remarks>
/// Not more often: on Windows, reading a process's private memory reads the information of every process on the machine, so sampling every 25 ms<br/>
/// took about a quarter of a core from the work it measured (dotTrace, 2026-11-12).
/// </remarks>
/// <param name="PrivateMegabytes">The highest private memory sampled: the memory committed for the process alone, as the operating system counts it.</param>
/// <param name="CommittedByTheGarbageCollectorMegabytes">The most memory sampled that the garbage collector had committed for its heap.</param>
public record PeakMemoryUse(long PrivateMegabytes, long CommittedByTheGarbageCollectorMegabytes)
{
   /// <summary>How often the memory is sampled.</summary>
   public static readonly TimeSpan TimeBetweenSamples = TimeSpan.FromMilliseconds(100);

   /// <summary>Runs <paramref name="work"/>, sampling the memory on a thread of its own meanwhile, and returns the peak the samples saw.</summary>
   public static PeakMemoryUse During(Action work)
   {
      var peak = Sample();
      var workIsDone = false;
      var sampler = new Thread(() =>
      {
         while(!Volatile.Read(ref workIsDone))
         {
            Thread.Sleep(TimeBetweenSamples);
            peak = peak.HigherOfEachAnd(Sample());
         }
      }) { IsBackground = true, Name = "Sampling the peak memory use" };

      sampler.Start();
      try
      {
         work();
      }
      finally
      {
         Volatile.Write(ref workIsDone, true);
         sampler.Join();
      }

      return peak.HigherOfEachAnd(Sample());
   }

   static PeakMemoryUse Sample()
   {
      var memoryUse = MemoryUse.Now();
      return new PeakMemoryUse(memoryUse.PrivateMegabytes, memoryUse.CommittedByTheGarbageCollectorMegabytes);
   }

   PeakMemoryUse HigherOfEachAnd(PeakMemoryUse other) =>
      new(Math.Max(PrivateMegabytes, other.PrivateMegabytes), Math.Max(CommittedByTheGarbageCollectorMegabytes, other.CommittedByTheGarbageCollectorMegabytes));
}
