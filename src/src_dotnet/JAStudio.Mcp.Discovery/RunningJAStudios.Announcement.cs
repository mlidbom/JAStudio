using System;

namespace JAStudio.Mcp.Discovery;

public sealed partial class RunningJAStudios
{
   /// <summary>A running JAStudio's announcement of itself, which it withdraws by disposing it, as its MCP server stops.</summary>
   public sealed class Announcement : IDisposable
   {
      readonly RunningJAStudios _runningJAStudios;
      readonly RunningJAStudio _jastudio;

      internal Announcement(RunningJAStudios runningJAStudios, RunningJAStudio jastudio)
      {
         _runningJAStudios = runningJAStudios;
         _jastudio = jastudio;
      }

      /// <summary>Withdraws the announcement.</summary>
      public void Dispose() => _runningJAStudios.Withdraw(_jastudio);
   }
}
