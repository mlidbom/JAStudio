using System;

namespace JAStudio.TextToSpeech.Audio.Loudness;

/// <summary>
/// How many 50 ms windows of a recording reached each loudness, in hundredths of a decibel, for ReplayGain 1's measurement (<see cref="ReplayGainMeasurement"/>).<br/>
/// The recording's loudness is the loudness that only 5% of its windows exceed: its loud parts, which are what a listener judges it by.
/// </summary>
/// <remarks>
/// A window's loudness is its mean square in decibels, after the <see cref="EqualLoudnessFilter"/>. As in ReplayGain's reference implementation,<br/>
/// <c>gain_analysis.c</c>, every window is counted, silent ones in the lowest step and impossibly loud ones in the highest, since leaving them out<br/>
/// would change which window is the 95th percentile.
/// </remarks>
sealed class LoudnessHistogram
{
   const int StepsPerDecibel = 100;
   const int HighestDecibels = 120;
   const double ShareOfWindowsLouderThanTheRecording = 1.0 - 0.95;

   readonly int[] _windowsPerStep = new int[StepsPerDecibel * HighestDecibels];

   /// <summary>Whether any window has been counted, which a recording shorter than one window has none of.</summary>
   public bool HasWindows => WindowCount > 0;

   int WindowCount { get; set; }

   /// <summary>Counts a window whose filtered samples' mean square, on the 16-bit scale, is <paramref name="meanSquare"/>.</summary>
   public void CountWindow(double meanSquare)
   {
      var step = (int)(StepsPerDecibel * 10 * Math.Log10(meanSquare + 1e-37));
      _windowsPerStep[Math.Clamp(step, 0, _windowsPerStep.Length - 1)]++;
      WindowCount++;
   }

   /// <summary>
   /// The loudness, in decibels on ReplayGain's internal scale, that only 5% of the windows exceed. Only when it <see cref="HasWindows"/>.
   /// </summary>
   /// <remarks>
   /// How many windows may exceed it is rounded up from <see cref="ShareOfWindowsLouderThanTheRecording"/> of them, which in double precision<br/>
   /// is a hair above 0.05, so that 100 windows allow 6, not 5. That's the reference implementation's arithmetic, and kept for matching mp3gain.
   /// </remarks>
   public double LoudnessOnlyFivePercentOfTheWindowsExceed()
   {
      if(!HasWindows) throw new InvalidOperationException("No window was counted: the recording is shorter than one window.");

      var windowsLeftToPass = (int)Math.Ceiling(WindowCount * ShareOfWindowsLouderThanTheRecording);
      var step = _windowsPerStep.Length - 1;
      for(; step > 0; step--)
      {
         windowsLeftToPass -= _windowsPerStep[step];
         if(windowsLeftToPass <= 0) break;
      }
      return (double)step / StepsPerDecibel;
   }
}
