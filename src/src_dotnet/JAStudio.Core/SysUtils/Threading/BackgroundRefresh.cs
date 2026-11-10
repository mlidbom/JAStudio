using System;
using Compze.Threading;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.SysUtils.Threading;

/// <summary>
/// Work that brings something up to date, run in the background whenever asked, one run at a time: asked while a run is under way, it runs once more<br/>
/// when that run is over, however often it was asked meanwhile. So the last run always starts after the last ask, and asks that come in a burst cost<br/>
/// two runs, not one each. Such as reading the study-notes from Anki again after Anki changed notes.
/// </summary>
public sealed class BackgroundRefresh
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly BackgroundTaskManager _backgroundTasks;
   readonly Action _refresh;
   bool _aRunIsUnderWay;
   bool _askedDuringTheRun;

   /// <param name="backgroundTasks">Runs the refresh on a background thread, and surfaces what it throws.</param>
   /// <param name="refresh">The work that brings something up to date.</param>
   public BackgroundRefresh(BackgroundTaskManager backgroundTasks, Action refresh)
   {
      _backgroundTasks = backgroundTasks;
      _refresh = refresh;
   }

   /// <summary>Runs the refresh in the background: now, or, when a run is under way, once more when that run is over.</summary>
   public void Run()
   {
      if(_monitor.Locked(StartARunUnlessOneIsUnderWay)) _backgroundTasks.Run(RunAsLongAsAskedAgain);
   }

   /// <summary>Whether a run starts now: when one is under way, the ask is noted for it instead.</summary>
   bool StartARunUnlessOneIsUnderWay()
   {
      if(_aRunIsUnderWay)
      {
         _askedDuringTheRun = true;
         return false;
      }

      _aRunIsUnderWay = true;
      return true;
   }

   void RunAsLongAsAskedAgain()
   {
      var stillRunning = true;
      try
      {
         while(stillRunning)
         {
            _refresh();
            stillRunning = _monitor.Locked(GoOnIfAskedDuringTheRun);
         }
      }
      finally
      {
         if(stillRunning) _monitor.Locked(() => _aRunIsUnderWay = _askedDuringTheRun = false); // the refresh threw: the next ask starts a run again
      }
   }

   bool GoOnIfAskedDuringTheRun()
   {
      _aRunIsUnderWay = _askedDuringTheRun;
      _askedDuringTheRun = false;
      return _aRunIsUnderWay;
   }
}
