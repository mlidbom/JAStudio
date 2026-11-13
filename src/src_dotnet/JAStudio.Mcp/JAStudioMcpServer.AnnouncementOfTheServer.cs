using System;
using System.IO;
using JAStudio.Core;
using JAStudio.Core.SysUtils.IO;
using JAStudio.Mcp.Discovery;

namespace JAStudio.Mcp;

public sealed partial class JAStudioMcpServer
{
   /// <summary>The server's announcement among the <see cref="RunningJAStudios"/> of this user, which it withdraws as it stops.</summary>
   sealed class AnnouncementOfTheServer : IDisposable
   {
      readonly RunningJAStudios _runningJAStudios;
      readonly RunningJAStudios.Announcement _announcement;

      AnnouncementOfTheServer(RunningJAStudios runningJAStudios, RunningJAStudios.Announcement announcement)
      {
         _runningJAStudios = runningJAStudios;
         _announcement = announcement;
      }

      /// <summary>
      /// Announces the server listening at <paramref name="url"/> as the MCP server of a JAStudio in this process, hosted by <paramref name="host"/>,<br/>
      /// working on the corpus checkout and the add-on folder of <paramref name="paths"/>, as the folders themselves, whatever links lead the JAStudio<br/>
      /// to them: Anki's does through a link to the add-on folder.
      /// </summary>
      public static AnnouncementOfTheServer Make(JAStudioHost host, Uri url, IEnvironmentPaths paths)
      {
         var runningJAStudios = RunningJAStudios.OfThisUser();
         var jastudio = RunningJAStudio.InThisProcess(host,
                                                      url,
                                                      corpusCheckout: new DirectoryInfo(paths.DatabaseDir).TheFolderItself(),
                                                      addOnFolder: new DirectoryInfo(paths.AddonRootDir).TheFolderItself());
         return new AnnouncementOfTheServer(runningJAStudios, runningJAStudios.Announce(jastudio));
      }

      /// <summary>Withdraws the announcement.</summary>
      public void Dispose()
      {
         _announcement.Dispose();
         _runningJAStudios.Dispose();
      }
   }
}
