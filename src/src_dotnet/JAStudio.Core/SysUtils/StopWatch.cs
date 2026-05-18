using System;
using Compze.Internals.Logging;

namespace JAStudio.Core.SysUtils;

class StopWatch
{
   readonly System.Diagnostics.Stopwatch _stopwatch;

   public StopWatch() => _stopwatch = System.Diagnostics.Stopwatch.StartNew();

   public TimeSpan Elapsed() => _stopwatch.Elapsed;

   public double ElapsedSeconds() => _stopwatch.Elapsed.TotalSeconds;

   public string ElapsedFormatted() => _stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");

   /// <summary>
   /// Returns a disposable that logs a warning if the execution takes longer than the threshold.
   /// </summary>
   public static IDisposable LogWarningIfSlowerThan(double warnIfSlowerThanSeconds, string message = "") => new TimingScope(warnIfSlowerThanSeconds, message);

   class TimingScope : IDisposable
   {
      readonly StopWatch _watch;
      readonly double _threshold;
      readonly string _message;

      public TimingScope(double threshold, string message)
      {
         _watch = new StopWatch();
         _threshold = threshold;
         _message = message;
      }

      public void Dispose()
      {
         var elapsed = _watch.ElapsedSeconds();
         if(elapsed > _threshold)
         {
            this.Log().Warning($"############## Execution time:{_watch.ElapsedFormatted()} for {_message} ##############");
         } else if(elapsed * 2 > _threshold)
         {
            this.Log().Info($"############## Execution time:{_watch.ElapsedFormatted()} for {_message} ##############");
         }
      }
   }
}
