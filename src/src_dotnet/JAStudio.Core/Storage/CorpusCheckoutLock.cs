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
/// for as long as it runs (<see cref="ExclusiveHoldOnAFile"/>), so a crash leaves no lock behind.
/// </remarks>
public sealed class CorpusCheckoutLock : IDisposable
{
   readonly ExclusiveHoldOnAFile _hold;

   CorpusCheckoutLock(ExclusiveHoldOnAFile hold) => _hold = hold;

   /// <summary>Holds the corpus checkout in <paramref name="corpusCheckoutFolder"/>, or refuses when another JAStudio holds it.</summary>
   public static CorpusCheckoutLock On(string corpusCheckoutFolder)
   {
      var lockFile = LockFileOf(corpusCheckoutFolder);
      return new CorpusCheckoutLock(ExclusiveHoldOnAFile.TakeUnlessAnotherProcessHasIt(lockFile)
                                 ?? throw new InvalidOperationException($"Another JAStudio works on the corpus checkout {corpusCheckoutFolder}, which it holds ({lockFile}): JAStudio inside Anki, " +
                                                                        "or a standalone or headless JAStudio. Two JAStudios would overwrite each other's edits, so close the other one first."));
   }

   /// <summary>
   /// The file beside <paramref name="corpusCheckoutFolder"/> that a JAStudio holds open while it works on it: beside the folder itself, with links<br/>
   /// followed, so that every JAStudio on a corpus checkout holds the same lock file, whichever path leads it there.
   /// </summary>
   static string LockFileOf(string corpusCheckoutFolder) => new DirectoryInfo(corpusCheckoutFolder).TheFolderItself().FullName + ".lock";

   /// <summary>Lets go of the corpus checkout, as the JAStudio stops.</summary>
   public void Dispose() => _hold.Dispose();
}
