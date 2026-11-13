using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Compze.Internals.SystemCE.DiagnosticsCE;

namespace JAStudio.Mcp.Discovery;

/// <summary>
/// What a running JAStudio announces about itself among the <see cref="RunningJAStudios"/>, for Claude's MCP gateway to find it and connect to its<br/>
/// MCP server: what hosts it, where its MCP server listens, what it works on, and the process it runs in.
/// </summary>
/// <remarks>
/// Every build of JAStudio running on this computer, from any source checkout and branch, reads and writes these, and so does every session's<br/>
/// gateway. So their JSON only ever gains properties, and what a newer build announced that an older one doesn't know, the older one keeps as it<br/>
/// was (<see cref="WhatANewerBuildAnnouncedToo"/>).
/// </remarks>
public sealed class RunningJAStudio
{
   /// <param name="host">What hosts the JAStudio.</param>
   /// <param name="mcpServer">The address the JAStudio's MCP server listens at.</param>
   /// <param name="corpusCheckout">The corpus checkout the JAStudio works on, with links followed.</param>
   /// <param name="addOnFolder">The add-on folder the JAStudio was started on, with links followed: a source checkout's <c>src</c> folder, whose code it runs.</param>
   /// <param name="processId">The id of the process the JAStudio runs in.</param>
   /// <param name="processStartTime">When that process started, in UTC: it tells the process apart from a later one given the same, recycled, id.</param>
   [JsonConstructor]
   public RunningJAStudio(JAStudioHost host, Uri mcpServer, DirectoryInfo corpusCheckout, DirectoryInfo addOnFolder, int processId, DateTime processStartTime)
   {
      Host = host;
      McpServer = mcpServer;
      CorpusCheckout = corpusCheckout;
      AddOnFolder = addOnFolder;
      ProcessId = processId;
      ProcessStartTime = processStartTime;
   }

   /// <summary>A JAStudio running in this process, hosted by <paramref name="host"/>, whose MCP server listens at <paramref name="mcpServer"/>.</summary>
   /// <param name="host">What hosts the JAStudio.</param>
   /// <param name="mcpServer">The address the JAStudio's MCP server listens at.</param>
   /// <param name="corpusCheckout">The corpus checkout the JAStudio works on, with links followed.</param>
   /// <param name="addOnFolder">The add-on folder the JAStudio was started on, with links followed.</param>
   public static RunningJAStudio InThisProcess(JAStudioHost host, Uri mcpServer, DirectoryInfo corpusCheckout, DirectoryInfo addOnFolder)
   {
      var thisProcess = ProcessIdentity.OfCurrentProcess;
      return new RunningJAStudio(host, mcpServer, corpusCheckout, addOnFolder, thisProcess.ProcessId, thisProcess.StartTime);
   }

   public JAStudioHost Host { get; }

   /// <summary>The address the JAStudio's MCP server listens at.</summary>
   public Uri McpServer { get; }

   /// <summary>The corpus checkout the JAStudio works on, with links followed.</summary>
   public DirectoryInfo CorpusCheckout { get; }

   /// <summary>The add-on folder the JAStudio was started on, with links followed: a source checkout's <c>src</c> folder, whose code the JAStudio runs.</summary>
   public DirectoryInfo AddOnFolder { get; }

   /// <summary>The id of the process the JAStudio runs in.</summary>
   public int ProcessId { get; }

   /// <summary>When the process the JAStudio runs in started, in UTC.</summary>
   public DateTime ProcessStartTime { get; }

   /// <summary>
   /// What a newer build of JAStudio announced that this build doesn't know, kept as it was, so that this build, announcing itself, doesn't strip it<br/>
   /// from the newer build's announcement.
   /// </summary>
   [JsonExtensionData]
   public Dictionary<string, JsonElement>? WhatANewerBuildAnnouncedToo { get; init; }

   /// <summary>Which JAStudio this is, the same through its restarts.</summary>
   [JsonIgnore]
   public JAStudioIdentity Identity => new(Host, CorpusCheckout);

   /// <summary>Whether the JAStudio's process still runs: one that crashed never withdrew its announcement.</summary>
   [JsonIgnore]
   public bool StillRuns => Process.IsCurrentlyRunning;

   ProcessIdentity Process => new(ProcessId, ProcessStartTime);

   /// <summary>
   /// Whether <paramref name="other"/> announces the same run of the same JAStudio as this: the same JAStudio, in the same process, with its MCP<br/>
   /// server at the same address. A JAStudio restarted is another run of it.
   /// </summary>
   public bool IsTheSameRunAs(RunningJAStudio other) => Identity == other.Identity && Process == other.Process && McpServer == other.McpServer;

   /// <summary>The run of the JAStudio as reports describe it: which JAStudio it is, what it was started on, its process, and its MCP server.</summary>
   public override string ToString() => $"{Identity}, started on the add-on folder {AddOnFolder.FullName}, in process {ProcessId}, its MCP server at {McpServer}";
}
