using System;
using System.Collections.Generic;
using System.IO;
using Compze.Threading;
using JAStudio.Core.Reservoir;
using JAStudio.Core.Reservoir.Feedback;

namespace JAStudio.Mcp.Tools.Reservoir;

/// <summary>
/// The reservoirs the MCP server's tools have opened, each with its sentences' texts loaded in memory, which takes a few tens of seconds for<br/>
/// millions of sentences: done at a reservoir's first use, so that every later use answers in seconds. They stay open for as long as the server runs.
/// </summary>
public sealed class OpenReservoirs : IDisposable
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly Dictionary<string, OpenReservoir> _byFullPath = [];

   /// <summary>The reservoir in <paramref name="file"/>, opened, and its texts loaded, now if it isn't yet.</summary>
   public OpenReservoir In(FileInfo file) => _monitor.Locked(() =>
   {
      if(_byFullPath.TryGetValue(file.FullName, out var open)) return open;

      var reservoir = ReservoirFile.OpenOrCreate(file);
      return _byFullPath[file.FullName] = new OpenReservoir(reservoir, ReservoirSentenceTexts.LoadedFrom(reservoir));
   });

   public void Dispose() => _monitor.Locked(() =>
   {
      foreach(var open in _byFullPath.Values) open.File.Dispose();
      _byFullPath.Clear();
   });
}
