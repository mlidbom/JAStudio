using System;
using System.IO;

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

   /// <summary>The file beside <paramref name="corpusCheckoutFolder"/> that a JAStudio holds open while it works on it.</summary>
   static string LockFileOf(string corpusCheckoutFolder) => TheFolderItself(corpusCheckoutFolder) + ".lock";

   /// <summary>
   /// The folder <paramref name="folder"/> is, when it's a link to another, such as a junction: so that every JAStudio on a corpus checkout holds the<br/>
   /// same lock file, whichever path leads it there. A link to a folder above it, such as Anki's link to the add-on folder, leads to the same file anyway.
   /// </summary>
   static string TheFolderItself(string folder)
   {
      var folderAsGiven = new DirectoryInfo(Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder)));
      return folderAsGiven.ResolveLinkTarget(returnFinalTarget: true)?.FullName ?? folderAsGiven.FullName;
   }

   /// <summary>Lets go of the corpus checkout, as the JAStudio stops.</summary>
   public void Dispose() => _heldOpen.Dispose();
}
