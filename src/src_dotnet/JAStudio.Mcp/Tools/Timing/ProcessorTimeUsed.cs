using System;

namespace JAStudio.Mcp.Tools.Timing;

/// <summary>
/// How much processor time some timed work used, such as a pass of parsing or a full rebuild, and how much the machine's other processes<br/>
/// used meanwhile. Other development work often runs on the machine, such as builds and tests. When it does, it takes cores from the work,<br/>
/// and the work's time says less about the code: <see cref="OtherProcessesSeconds"/> tells whether that happened.
/// </summary>
/// <param name="ThisProcessSeconds">
/// The processor time this process used during the work, summed over its threads, the garbage collector's included. Divided by the work's<br/>
/// time, how many cores it kept busy. Other work inflates it about as much as the pass's time: a thread that shares a physical core with<br/>
/// another process's runs slower, and its processor time counts on (2026-11-12: 15 seconds for a single-threaded pass that takes 6 alone).
/// </param>
/// <param name="OtherProcessesSeconds">
/// The processor time the machine's other processes used during the work, summed over every core. Near zero when nothing disturbed it.<br/>
/// Null where it isn't read: see <see cref="MachineProcessorTime"/>.
/// </param>
public record ProcessorTimeUsed(double ThisProcessSeconds, double? OtherProcessesSeconds)
{
   /// <summary>Starts measuring: the returned function, called when the work is done, returns the processor time used since.</summary>
   public static Func<ProcessorTimeUsed> MeasuringFromNow()
   {
      var thisProcessBefore = Environment.CpuUsage.TotalTime;
      var machineBefore = MachineProcessorTime.UsedSinceTheMachineStarted();
      return () =>
      {
         var thisProcess = Environment.CpuUsage.TotalTime - thisProcessBefore;
         var machine = MachineProcessorTime.UsedSinceTheMachineStarted() - machineBefore;
         var otherProcesses = machine - thisProcess;
         return new ProcessorTimeUsed(Math.Round(thisProcess.TotalSeconds, 2), otherProcesses is {} other ? Math.Round(other.TotalSeconds, 2) : null);
      };
   }
}
