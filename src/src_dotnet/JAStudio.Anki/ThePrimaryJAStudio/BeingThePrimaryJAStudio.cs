using System;
using System.IO;
using JAStudio.Core.SysUtils.IO;

namespace JAStudio.Anki.ThePrimaryJAStudio;

/// <summary>
/// This JAStudio's being Anki's primary JAStudio, for as long as it lasts: the one the Anki add-on sends what goes from Anki to JAStudio, which<br/>
/// finds it by its access file (<see cref="PrimaryJAStudioAccessFile"/>). Disposing it, as the JAStudio stops, removes the access file and lets<br/>
/// another JAStudio become the primary one.
/// </summary>
/// <remarks>
/// A JAStudio that still runs doesn't become the primary one when this one stops: only one that starts after does.
/// </remarks>
public sealed class BeingThePrimaryJAStudio : IDisposable
{
   readonly ExclusiveHoldOnAFile _hold;
   readonly FileInfo _accessFile;

   internal BeingThePrimaryJAStudio(ExclusiveHoldOnAFile hold, FileInfo accessFile)
   {
      _hold = hold;
      _accessFile = accessFile;
   }

   /// <summary>Removes the access file, and then lets go of being the primary JAStudio, so that no JAStudio that becomes it next has its file removed.</summary>
   public void Dispose()
   {
      _accessFile.Delete();
      _hold.Dispose();
   }
}
