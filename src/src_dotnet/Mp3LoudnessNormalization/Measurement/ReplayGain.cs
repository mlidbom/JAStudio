using System;
using System.Globalization;

namespace Mp3LoudnessNormalization.Measurement;

/// <summary>
/// How many decibels a recording's volume should change by to sound as loud as ReplayGain 1's reference, 89 dB SPL: negative for a recording<br/>
/// that sounds louder. It's what mp3gain calls the recommended track gain, measured by a <see cref="ReplayGainMeasurement"/>.
/// </summary>
/// <param name="Decibels">The change in decibels.</param>
public readonly record struct ReplayGain(double Decibels)
{
   /// <summary>
   /// How many decibels one step of an MP3 frame's gain changes its volume by: 5·log10(2), about 1.505, since a step multiplies the samples by 2^¼.
   /// </summary>
   public const double DecibelsPerMp3GainStep = 1.5051499783199060;

   /// <summary>
   /// The gain in steps of an MP3 frame's gain, the nearest whole number of steps to <see cref="Decibels"/>, rounded as mp3gain rounds it:<br/>
   /// halfway rounds up.
   /// </summary>
   public int Mp3GainSteps => (int)Math.Floor(Decibels / DecibelsPerMp3GainStep + 0.5);

   public override string ToString() => $"{Decibels.ToString("+0.00;-0.00", CultureInfo.InvariantCulture)} dB";
}
