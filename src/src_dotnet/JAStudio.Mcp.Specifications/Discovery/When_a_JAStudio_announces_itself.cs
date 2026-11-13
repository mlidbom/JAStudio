using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Discovery;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Discovery;

/// <summary>
/// A running JAStudio announces itself among the <see cref="RunningJAStudios"/>, for Claude's MCP gateway to find: here in a folder of the<br/>
/// specification's own, apart from the JAStudios really running.
/// </summary>
public class When_a_JAStudio_announces_itself : IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"JAStudio_running_jastudios_{Guid.NewGuid():N}"));
   readonly DirectoryInfo _registryFolder;
   readonly RunningJAStudios _runningJAStudios;
   readonly RunningJAStudio _theJAStudio;
   readonly RunningJAStudios.Announcement _itsAnnouncement;

   public When_a_JAStudio_announces_itself()
   {
      _registryFolder = _folder.CreateSubdirectory("registry");
      _runningJAStudios = RunningJAStudios.In(_registryFolder);
      _theJAStudio = AJAStudioInThisProcess(JAStudioHost.Standalone, "jas_database", mcpServerPort: 5000);
      _itsAnnouncement = _runningJAStudios.Announce(_theJAStudio);
   }

   RunningJAStudio AJAStudioInThisProcess(JAStudioHost host, string corpusCheckout, int mcpServerPort) =>
      RunningJAStudio.InThisProcess(host, new Uri($"http://127.0.0.1:{mcpServerPort}/"), CorpusCheckoutNamed(corpusCheckout), _folder);

   DirectoryInfo CorpusCheckoutNamed(string name) => new(Path.Combine(_folder.FullName, name));

   bool IsRunning(RunningJAStudio jastudio) => _runningJAStudios.All.Any(it => it.IsTheSameRunAs(jastudio));

   [XF] public void it_is_among_the_running_JAStudios() => _theJAStudio.Must().Satisfy(IsRunning);

   public class and_withdraws_its_announcement : When_a_JAStudio_announces_itself
   {
      public and_withdraws_its_announcement() => _itsAnnouncement.Dispose();

      [XF] public void it_is_not_among_the_running_JAStudios() => _theJAStudio.Must().Satisfy(it => !IsRunning(it));
   }

   public class and_another_run_of_the_same_JAStudio_announces_itself : When_a_JAStudio_announces_itself
   {
      readonly RunningJAStudio _theOtherRun;

      public and_another_run_of_the_same_JAStudio_announces_itself()
      {
         _theOtherRun = AJAStudioInThisProcess(JAStudioHost.Standalone, "jas_database", mcpServerPort: 5001);
         _runningJAStudios.Announce(_theOtherRun);
      }

      [XF] public void the_other_run_is_the_only_run_of_it_among_the_running_JAStudios() =>
         _runningJAStudios.All.Where(it => it.Identity == _theJAStudio.Identity).Must().Satisfy(it => it.Single().IsTheSameRunAs(_theOtherRun));
   }

   public class and_a_JAStudio_whose_process_no_longer_runs_is_announced : When_a_JAStudio_announces_itself
   {
      readonly RunningJAStudio _theJAStudioThatNoLongerRuns;

      public and_a_JAStudio_whose_process_no_longer_runs_is_announced()
      {
         var whenAnotherProcessWithThisProcessesIdStarted = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
         _theJAStudioThatNoLongerRuns = new RunningJAStudio(JAStudioHost.Headless, new Uri("http://127.0.0.1:5002/"), CorpusCheckoutNamed("another_corpus"), _folder, Environment.ProcessId, whenAnotherProcessWithThisProcessesIdStarted);
         _runningJAStudios.Announce(_theJAStudioThatNoLongerRuns);
      }

      [XF] public void it_is_not_among_the_running_JAStudios() => _theJAStudioThatNoLongerRuns.Must().Satisfy(it => !IsRunning(it));
   }

   public class and_a_newer_build_announces_itself_with_what_this_build_does_not_know : When_a_JAStudio_announces_itself
   {
      readonly RunningJAStudio _ofTheNewerBuild;

      public and_a_newer_build_announces_itself_with_what_this_build_does_not_know()
      {
         _ofTheNewerBuild = new RunningJAStudio(JAStudioHost.Headless, new Uri("http://127.0.0.1:5003/"), CorpusCheckoutNamed("newer_corpus"), _folder, Environment.ProcessId, _theJAStudio.ProcessStartTime)
                            {
                               WhatANewerBuildAnnouncedToo = new Dictionary<string, JsonElement> { ["sourceCommit"] = JsonDocument.Parse("\"5bdf24eb2\"").RootElement }
                            };
         _runningJAStudios.Announce(_ofTheNewerBuild);
      }

      public class and_then_another_JAStudio_announces_itself : and_a_newer_build_announces_itself_with_what_this_build_does_not_know
      {
         public and_then_another_JAStudio_announces_itself() => _runningJAStudios.Announce(AJAStudioInThisProcess(JAStudioHost.Anki, "another_corpus", mcpServerPort: 5004));

         [XF] public void what_the_newer_build_announced_too_is_kept() =>
            _runningJAStudios.All.Single(it => it.IsTheSameRunAs(_ofTheNewerBuild)).WhatANewerBuildAnnouncedToo?["sourceCommit"].GetString().Must().Be("5bdf24eb2");
      }
   }

   public class and_a_wait_for_a_change_to_the_running_JAStudios_starts : When_a_JAStudio_announces_itself
   {
      readonly Task _theWait;

      public and_a_wait_for_a_change_to_the_running_JAStudios_starts() =>
         _theWait = _runningJAStudios.AwaitAChangeFromAsync(_runningJAStudios.All, TimeSpan.FromMinutes(1), CancellationToken.None);

      public class and_another_JAStudio_announces_itself_in_another_process : and_a_wait_for_a_change_to_the_running_JAStudios_starts
      {
         readonly RunningJAStudios _asAnotherProcessOpensThem;

         public and_another_JAStudio_announces_itself_in_another_process()
         {
            _asAnotherProcessOpensThem = RunningJAStudios.In(_registryFolder);
            _asAnotherProcessOpensThem.Announce(AJAStudioInThisProcess(JAStudioHost.Headless, "another_corpus", mcpServerPort: 5005));
         }

         [XF] public void the_wait_ends_within_seconds() => _theWait.Must().Satisfy(it => it.Wait(TimeSpan.FromSeconds(10)));

         public override void Dispose()
         {
            _asAnotherProcessOpensThem.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
         }
      }
   }

   public virtual void Dispose()
   {
      _runningJAStudios.Dispose();
      _folder.Delete(recursive: true);
      GC.SuppressFinalize(this);
   }
}
