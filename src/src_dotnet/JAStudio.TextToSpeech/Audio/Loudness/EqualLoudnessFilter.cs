using System;
using System.Collections.Generic;

namespace JAStudio.TextToSpeech.Audio.Loudness;

/// <summary>
/// ReplayGain 1's equal-loudness filter for one channel of a recording: it weights the frequencies roughly as human hearing does,<br/>
/// so that the loudness measured afterwards is the loudness heard (<see cref="ReplayGainMeasurement"/>).
/// </summary>
/// <remarks>
/// Two filters in a row: a 10th-order Yule-Walker filter shaped like the inverse of an equal-loudness curve,<br/>
/// and a 2nd-order Butterworth high-pass filter at 150 Hz. Each sample rate has its own coefficients.
/// </remarks>
/// <remarks>
/// The arithmetic is that of ReplayGain's reference implementation, <c>gain_analysis.c</c>, which mp3gain uses: double precision, the terms summed<br/>
/// in the same order, and the Yule-Walker filter adding 1e-10 to every output, to keep silence from producing denormal numbers, which are slow.<br/>
/// The order only matters for matching mp3gain's measurement to the last bit.
/// </remarks>
sealed partial class EqualLoudnessFilter
{
   const double DenormalPrevention = 1e-10;

   readonly Coefficients _coefficients;
   readonly SampleHistory _yuleWalkerInputs = new(Coefficients.YuleWalkerOrder);
   readonly SampleHistory _yuleWalkerOutputs = new(Coefficients.YuleWalkerOrder);
   readonly SampleHistory _butterworthInputs = new(Coefficients.ButterworthOrder);
   readonly SampleHistory _butterworthOutputs = new(Coefficients.ButterworthOrder);

   /// <summary>A filter for a channel sampled <paramref name="sampleRate"/> times a second, which must be one of the rates MP3 allows.</summary>
   public EqualLoudnessFilter(int sampleRate) =>
      _coefficients = CoefficientsBySampleRate.GetValueOrDefault(sampleRate)
                   ?? throw new ArgumentException($"ReplayGain has no equal-loudness filter for {sampleRate} Hz, which isn't a sample rate MP3 allows.", nameof(sampleRate));

   /// <summary>Filters the channel's next sample, and returns the filtered sample.</summary>
   public double Filter(double sample) => Butterworth(YuleWalker(sample));

   double YuleWalker(double sample)
   {
      var output = DenormalPrevention + sample * _coefficients.YuleWalkerFeedforward[0];
      for(var samplesBack = 1; samplesBack <= Coefficients.YuleWalkerOrder; samplesBack++)
      {
         output -= _yuleWalkerOutputs.SamplesBack(samplesBack) * _coefficients.YuleWalkerFeedback[samplesBack];
         output += _yuleWalkerInputs.SamplesBack(samplesBack) * _coefficients.YuleWalkerFeedforward[samplesBack];
      }
      _yuleWalkerInputs.Add(sample);
      _yuleWalkerOutputs.Add(output);
      return output;
   }

   double Butterworth(double sample)
   {
      var output = sample * _coefficients.ButterworthFeedforward[0];
      for(var samplesBack = 1; samplesBack <= Coefficients.ButterworthOrder; samplesBack++)
      {
         output -= _butterworthOutputs.SamplesBack(samplesBack) * _coefficients.ButterworthFeedback[samplesBack];
         output += _butterworthInputs.SamplesBack(samplesBack) * _coefficients.ButterworthFeedforward[samplesBack];
      }
      _butterworthInputs.Add(sample);
      _butterworthOutputs.Add(output);
      return output;
   }
}
