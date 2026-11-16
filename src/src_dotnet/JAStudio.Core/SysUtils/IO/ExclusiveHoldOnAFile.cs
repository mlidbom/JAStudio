using System;
using System.IO;

namespace JAStudio.Core.SysUtils.IO;

/// <summary>
/// A hold on a file that one process at a time can have, for as long as it keeps it: the file held open, shared with no one. The system lets go of<br/>
/// it when the process ends, however it ends, so a crash leaves no hold behind. What it's used for is one JAStudio at a time having something, such<br/>
/// as a corpus checkout to work on.
/// </summary>
/// <remarks>
/// On Windows the system also deletes the file as it lets go of it. Elsewhere the file may stay behind, held by no one, which is no hold.
/// </remarks>
/// <remarks>
/// todo:compze: Compze.Threading.Interprocess has no exclusive hold a process keeps for as long as it runs, let go of from any thread, and by the<br/>
/// system when the process ends: its IMutex wraps a system mutex, which only the thread that took it may release, and the standalone JAStudio<br/>
/// stops on a thread of its own. Once Compze has such a hold, use it instead of this.
/// </remarks>
public sealed class ExclusiveHoldOnAFile : IDisposable
{
   /// <summary>
   /// The error code, as <see cref="Exception.HResult"/>, of the <see cref="IOException"/> the system gives opening a file another process holds open:<br/>
   /// ERROR_SHARING_VIOLATION on Windows; elsewhere EWOULDBLOCK, from the lock .NET takes on the file for <see cref="FileShare.None"/>, which is 11 on<br/>
   /// Linux and 35 on macOS.
   /// </summary>
   static readonly int TheFileIsOpenInAnotherProcess = OperatingSystem.IsWindows() ? unchecked((int)0x80070020)
                                                       : OperatingSystem.IsLinux() ? 11
                                                       : 35;

   readonly FileStream _heldOpen;

   ExclusiveHoldOnAFile(FileStream heldOpen) => _heldOpen = heldOpen;

   /// <summary>Takes the hold on the file at <paramref name="path"/>, which is made if it isn't there, or gives null when another process has it.</summary>
   public static ExclusiveHoldOnAFile? TakeUnlessAnotherProcessHasIt(string path)
   {
      try
      {
         return new ExclusiveHoldOnAFile(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 1, FileOptions.DeleteOnClose));
      }
      catch(IOException exception) when(exception.HResult == TheFileIsOpenInAnotherProcess)
      {
         return null;
      }
   }

   /// <summary>Lets go of the hold.</summary>
   public void Dispose() => _heldOpen.Dispose();
}
