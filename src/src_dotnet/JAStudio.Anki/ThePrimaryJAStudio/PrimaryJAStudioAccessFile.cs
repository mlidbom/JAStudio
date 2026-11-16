using System;
using System.IO;
using System.Text.Json;
using JAStudio.Core.SysUtils.IO;

namespace JAStudio.Anki.ThePrimaryJAStudio;

/// <summary>
/// The primary JAStudio's access file: the file in which Anki's primary JAStudio, the one the Anki add-on sends what goes from Anki to JAStudio,<br/>
/// says how to reach it: its card server's port, and its process's id. The add-on reads it (Python's <c>primary_jastudio</c>) before each request.
/// </summary>
/// <remarks>
/// The first JAStudio to start of those that run is the primary one: a JAStudio becomes it as its card server starts (<see cref="TakeUnlessAnotherJAStudioIsIt"/>)<br/>
/// unless another that still runs is, and it stops being it as it stops. Which JAStudio is the primary one is the hold on a file beside the access<br/>
/// file (<see cref="ExclusiveHoldOnAFile"/>), which the system lets go of when the JAStudio's process ends, however it ends: so a JAStudio that ended<br/>
/// without stopping, as one stopped in the debugger does, is never taken for one that runs.
/// </remarks>
/// <remarks>
/// It mirrors the add-on's access file (<see cref="AnkisFunctions.OverHttp.AnkisHttpServerAccessFile"/>), and lies beside it. It holds no token:<br/>
/// the card server takes none, and its pages show any process on this computer what the requests show.
/// </remarks>
/// <remarks>review-naming:anki-http: the primary JAStudio's access file, and its file names, are Claude's (2026-11-15).</remarks>
public sealed class PrimaryJAStudioAccessFile(DirectoryInfo folder)
{
   /// <summary>The access file every JAStudio of this user writes, and the add-on reads: in the user's own folder for JAStudio's data.</summary>
   public static PrimaryJAStudioAccessFile OfThisUser => new(ThisUsersFolderForJAStudio.Folder);

   readonly DirectoryInfo _folder = folder;

   /// <summary>The access file itself. The add-on names the same file (Python's <c>the_primary_jastudios_access_file_of_this_user</c>).</summary>
   public FileInfo File => new(Path.Combine(_folder.FullName, "primary-jastudio-access.json"));

   /// <summary>The file whose hold makes a JAStudio the primary one.</summary>
   string TheHoldsFile => Path.Combine(_folder.FullName, "primary-jastudio.lock");

   /// <summary>
   /// Makes this JAStudio, whose card server listens on <paramref name="cardServersPort"/>, the primary JAStudio, and writes the access file,<br/>
   /// unless another JAStudio that still runs is the primary one: then it gives null.
   /// </summary>
   public BeingThePrimaryJAStudio? TakeUnlessAnotherJAStudioIsIt(int cardServersPort)
   {
      _folder.Create();
      if(ExclusiveHoldOnAFile.TakeUnlessAnotherProcessHasIt(TheHoldsFile) is not {} hold) return null;

      WriteWholeOrNotAtAll(new { port = cardServersPort, processId = Environment.ProcessId });
      return new BeingThePrimaryJAStudio(hold, File);
   }

   /// <summary>
   /// Writes the file to one beside it, which then replaces it, so that the add-on never reads half of it. A file left behind by a primary JAStudio<br/>
   /// that ended without stopping is replaced.
   /// </summary>
   void WriteWholeOrNotAtAll(object access)
   {
      var written = new FileInfo(File.FullName + ".new");
      System.IO.File.WriteAllBytes(written.FullName, JsonSerializer.SerializeToUtf8Bytes(access));
      written.MoveTo(File.FullName, overwrite: true);
   }
}
