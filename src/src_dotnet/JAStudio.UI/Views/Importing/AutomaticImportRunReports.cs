using System;
using Avalonia.Threading;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Importing;

namespace JAStudio.UI.Views.Importing;

/// <summary>
/// Shows what the <see cref="AutomaticImportRuns"/> did. A run its problems stopped opens its profile's import run window, which checks again and shows<br/>
/// the report, as Check does. A run that imported says so in a passing message in the spaced repetition system's own windows: in Anki, its tooltip.
/// </summary>
public static class AutomaticImportRunReports
{
   /// <summary>Starts showing what the automatic import runs do, for as long as JAStudio runs. Called once, when JAStudio starts in Anki.</summary>
   public static void StartShowing(TemporaryServiceCollection services)
   {
      var automaticImportRuns = services.Resolve<AutomaticImportRuns>();
      automaticImportRuns.StoppedByProblems.Subscribe(profile => Dispatcher.UIThread.Post(() => ImportRunWindow.CheckInItsWindow(profile, services)));
      automaticImportRuns.Executed.Subscribe(run => services.SRSWindows.ShowAPassingMessage(
                                                $"The import profile {run.Profile.Name} imported {run.ImportedSentences.Count} sentences into JA Studio.", shownFor: TimeSpan.FromSeconds(5)));
   }
}
