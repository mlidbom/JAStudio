using System;
using System.IO;
using JAStudio.Core.SysUtils.IO;

namespace JAStudio.Core.Storage;

/// <summary>
/// A running JAStudio's hold on the corpus checkout it works on, which keeps every other JAStudio off it: two JAStudios saving notes to one corpus<br/>
/// checkout, such as JAStudio inside Anki and the standalone JAStudio on the same source checkout, would each overwrite the other's edits.
/// </summary>
/// <remarks>
/// The hold is a file beside the corpus checkout, named after it, such as <c>jas_database.lock</c> beside <c>jas_database</c>, which the JAStudio holds<br/>
/// open, shared with no one, for as long as it runs. The system closes and deletes it when the JAStudio's process ends, however it ends, so a crash<br/>
/// leaves no lock behind.
/// </remarks>
public sealed class CorpusCheckoutLock : IDisposable
{
   /// <summary>The error Windows gives opening a file another process holds open, ERROR_SHARING_VIOLATION, as an HRESULT.</summary>
   const int TheFileIsOpenInAnotherProcess = unchecked((int)0x80070020);

   readonly FileStream _heldOpen;

   CorpusCheckoutLock(FileStream heldOpen) => _heldOpen = heldOpen;

   /// <summary>Holds the corpus checkout in <paramref name="corpusCheckoutFolder"/>, or refuses when another JAStudio holds it.</summary>
   /// <remarks>On Windows, the refusal says which corpus checkout another JAStudio holds; elsewhere it's the system's own error, which names the lock file.</remarks>
   public static CorpusCheckoutLock On(string corpusCheckoutFolder)
   {
      var lockFile = LockFileOf(corpusCheckoutFolder);
      //todo:compze: Compze.Threading.Interprocess has no exclusive hold a process keeps for as long as it runs, let go of from any thread, and by the
      //   system when the process ends: its IMutex wraps a system mutex, which only the thread that took it may release, and the standalone JAStudio
      //   stops on a thread of its own. Once Compze has such a hold, use it here instead of this file.
      try
      {
         return new CorpusCheckoutLock(new FileStream(lockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize: 1, FileOptions.DeleteOnClose));
      }
      catch(IOException exception) when(exception.HResult == TheFileIsOpenInAnotherProcess)
      {
         throw new InvalidOperationException($"Another JAStudio works on the corpus checkout {corpusCheckoutFolder}, which it holds ({lockFile}): JAStudio inside Anki, " +
                                             "or a standalone or headless JAStudio. Two JAStudios would overwrite each other's edits, so close the other one first.", exception);
      }
   }

   /// <summary>
   /// The file beside <paramref name="corpusCheckoutFolder"/> that a JAStudio holds open while it works on it: beside the folder itself, with links<br/>
   /// followed, so that every JAStudio on a corpus checkout holds the same lock file, whichever path leads it there.
   /// </summary>
   static string LockFileOf(string corpusCheckoutFolder) => new DirectoryInfo(corpusCheckoutFolder).TheFolderItself().FullName + ".lock";

   /// <summary>Lets go of the corpus checkout, as the JAStudio stops.</summary>
   public void Dispose() => _heldOpen.Dispose();
}
