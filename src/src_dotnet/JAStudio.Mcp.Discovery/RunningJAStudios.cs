using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compze.InterprocessObject;
using Compze.Threading;

namespace JAStudio.Mcp.Discovery;

/// <summary>
/// The JAStudios running on this computer for this user, each as it announced itself (<see cref="RunningJAStudio"/>): inside Anki, standalone or<br/>
/// headless, built from any source checkout. Claude's MCP gateway lists them, connects to their MCP servers, and follows them as they stop and start<br/>
/// again, so that Claude can work in several at once, such as to compare how two branches parse the same sentences.
/// </summary>
/// <remarks>
/// The announcements are an object shared between processes (Compze's <see cref="IInterprocessObject{T}"/>): a memory-mapped file, which every<br/>
/// process that opens it sees each change to at once, and whose every change wakes the processes waiting for one (<see cref="AwaitAChangeFromAsync"/>).
/// <br/><br/>
/// A JAStudio that crashed never withdrew its announcement, and the file outlives every process. So an announcement whose process no longer runs is<br/>
/// left out of <see cref="All"/>, and removed from the file by the next announcement.
/// </remarks>
public sealed partial class RunningJAStudios : IDisposable
{
   /// <summary>The name of the shared object, and so of its files in the folder it's in.</summary>
   const string Name = "running-jastudios";

   /// <summary>The most bytes the announcements may take serialized: one takes about 400.</summary>
   const int MaxBytes = 64 * 1024;

   readonly IInterprocessObject<List<RunningJAStudio>> _announcements;

   RunningJAStudios(DirectoryInfo folder) =>
      _announcements = IInterprocessObject.NewLocal(Name, new AnnouncementsAsJson(), createDefault: () => [], CorruptionAction.ReplaceContentWithDefaultAndThrow, MaxBytes, folder);

   /// <summary>The JAStudios running for this user, announced in JAStudio's folder of the local application data: <c>%LOCALAPPDATA%\JAStudio</c> on Windows.</summary>
   public static RunningJAStudios OfThisUser() =>
      new(Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio")));

   /// <summary>The JAStudios announced in <paramref name="folder"/>, an existing folder: specifications announce theirs apart from the JAStudios really running.</summary>
   public static RunningJAStudios In(DirectoryInfo folder) => new(folder);

   /// <summary>Every running JAStudio, in the order they announced themselves.</summary>
   public IReadOnlyList<RunningJAStudio> All => _announcements.Read(TheOnesStillRunning);

   /// <summary>
   /// Announces <paramref name="jastudio"/>, once its MCP server listens, until the returned announcement is withdrawn, as the server stops. It takes<br/>
   /// the place of an announcement of the same JAStudio (<see cref="RunningJAStudio.Identity"/>) that a run of it that crashed left, and removes the<br/>
   /// others whose process no longer runs.
   /// </summary>
   public Announcement Announce(RunningJAStudio jastudio)
   {
      _announcements.Update(announcements =>
      {
         announcements.RemoveAll(it => !it.StillRuns || it.Identity == jastudio.Identity);
         announcements.Add(jastudio);
      });
      return new Announcement(this, jastudio);
   }

   void Withdraw(RunningJAStudio jastudio) => _announcements.Update(announcements => announcements.RemoveAll(it => it.IsTheSameRunAs(jastudio)));

   /// <summary>
   /// Waits, holding no thread, until the running JAStudios are others than <paramref name="seen"/>, as a JAStudio announces itself or withdraws its<br/>
   /// announcement, until <paramref name="timeout"/> has passed, or until <paramref name="cancellationToken"/> is cancelled, which it then throws for.
   /// </summary>
   /// <remarks>A JAStudio that crashes wakes no one: it just stops being listed. Waiting with a timeout and looking again finds that out too.</remarks>
   public Task AwaitAChangeFromAsync(IReadOnlyList<RunningJAStudio> seen, TimeSpan timeout, CancellationToken cancellationToken) =>
      _announcements.TryAwaitAsync(announcements => !AreTheSameRuns(TheOnesStillRunning(announcements), seen), cancellationToken, new WaitTimeout(timeout));

   static List<RunningJAStudio> TheOnesStillRunning(List<RunningJAStudio> announcements) => [.. announcements.Where(it => it.StillRuns)];

   static bool AreTheSameRuns(List<RunningJAStudio> these, IReadOnlyList<RunningJAStudio> those) =>
      these.Count == those.Count && these.Zip(those).All(it => it.First.IsTheSameRunAs(it.Second));

   public void Dispose() => _announcements.Dispose();
}
