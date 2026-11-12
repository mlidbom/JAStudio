using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JAStudio.Mcp.Tools.Analysis.Timing;

/// <summary>
/// The processor time every process on the machine has used together, read from the operating system: this process's, and that of any other<br/>
/// work running on the machine, such as builds and tests.
/// </summary>
/// <remarks>
/// .NET has no way to read it that works on every platform, so it's read on Windows only, where JAStudio is developed. On other platforms it's<br/>
/// unknown, and nothing depends on it but what <see cref="ProcessorTimeUsed"/> reports.
/// </remarks>
static class MachineProcessorTime
{
   /// <summary>
   /// The processor time every process has used since the machine started, summed over every core. Null where it isn't read: on platforms<br/>
   /// other than Windows.
   /// </summary>
   public static TimeSpan? UsedSinceTheMachineStarted() => OperatingSystem.IsWindows() ? UsedSinceTheMachineStartedOnWindows() : null;

   static TimeSpan UsedSinceTheMachineStartedOnWindows()
   {
      if(!GetSystemTimes(out var idle, out var kernel, out var user)) throw new Win32Exception(Marshal.GetLastWin32Error());
      var kernelWithoutIdle = kernel - idle;
      return TimeSpan.FromTicks(kernelWithoutIdle + user);
   }

   /// <summary>
   /// Windows' processor time since it started, summed over every core, in the 100-nanosecond units of a <see cref="TimeSpan"/>'s ticks: idle,<br/>
   /// in the kernel (the idle time included), and in user mode.
   /// </summary>
   [DllImport("kernel32.dll", SetLastError = true)]
   [return: MarshalAs(UnmanagedType.Bool)]
   static extern bool GetSystemTimes(out long idleTime, out long kernelTime, out long userTime);
}
