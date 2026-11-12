using System;

namespace JAStudio.Mcp.Tools.Analysis.Timing;

/// <summary>
/// How much processor time a timed pass used, and how much the machine's other processes used meanwhile.<br/>
/// Other development work often runs on the machine, such as builds and tests. When it does, it takes cores from the pass, and the pass's<br/>
/// time says less about the code: <see cref="OtherProcessesSeconds"/> tells whether that happened.
/// </summary>
/// <param name="ThisProcessSeconds">
/// The processor time this process used during the pass, summed over its threads, the garbage collector's included. Divided by the pass's<br/>
/// time, how many cores it kept busy. Other work inflates it about as much as the pass's time: a thread that shares a physical core with<br/>
/// another process's runs slower, and its processor time counts on (2026-11-12: 15 seconds for a single-threaded pass that takes 6 alone).
/// </param>
/// <param name="OtherProcessesSeconds">
/// The processor time the machine's other processes used during the pass, summed over every core. Near zero for an undisturbed pass.<br/>
/// Null where it isn't read: see <see cref="MachineProcessorTime"/>.
/// </param>
public record ProcessorTimeUsed(double ThisProcessSeconds, double? OtherProcessesSeconds)
{
   /// <summary>Starts measuring: the returned function, called when the pass is done, returns the processor time used since.</summary>
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
