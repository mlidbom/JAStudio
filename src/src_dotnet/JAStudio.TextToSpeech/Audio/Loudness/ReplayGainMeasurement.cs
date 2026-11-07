namespace JAStudio.TextToSpeech.Audio.Loudness;

/// <summary>
/// ReplayGain 1's measurement of how loud a recording sounds, done as mp3gain does it, fed one sample frame at a time:<br/>
/// the result is the <see cref="ReplayGain"/> that brings it to ReplayGain's reference loudness.
/// </summary>
/// <remarks>
/// Each channel goes through an <see cref="EqualLoudnessFilter"/>. The filtered samples are cut into windows of 50 ms, and each window's mean square,<br/>
/// averaged over the channels, is counted in a <see cref="LoudnessHistogram"/>, which gives the recording's loudness as that of its loud parts.<br/>
/// Samples after the last full window are left out, as in ReplayGain's reference implementation.
/// </remarks>
/// <remarks>
/// The samples come in as decoders give them, from -1 to 1, and are scaled to the 16-bit range the algorithm was calibrated on, as mp3gain scales<br/>
/// decoded MP3: by 32768 when mono, but by 32767 when stereo. A mono recording counts as two identical channels.
/// </remarks>
sealed class ReplayGainMeasurement
{
   /// <summary>
   /// What ReplayGain's measurement gives for its reference: pink noise at -14 dB full scale, played back at 89 dB SPL.<br/>
   /// A recording measured louder than this needs a negative gain. See <see href="https://replaygain.hydrogenaud.io/calibration.html"/>.
   /// </summary>
   const double ReferenceLoudness = 64.82;

   const double MonoSampleScale = 32768;
   const double StereoSampleScale = 32767;
   const int MillisecondsPerWindow = 50;

   readonly EqualLoudnessFilter _leftFilter;
   readonly EqualLoudnessFilter _rightFilter;
   readonly LoudnessHistogram _histogram = new();
   readonly int _framesPerWindow;
   double _leftSumOfSquares;
   double _rightSumOfSquares;
   int _framesInTheWindow;

   /// <summary>A measurement of a recording sampled <paramref name="sampleRate"/> times a second, which must be one of the rates MP3 allows.</summary>
   public ReplayGainMeasurement(int sampleRate)
   {
      _leftFilter = new EqualLoudnessFilter(sampleRate);
      _rightFilter = new EqualLoudnessFilter(sampleRate);
      _framesPerWindow = (sampleRate * MillisecondsPerWindow + 999) / 1000;
   }

   /// <summary>Measures the next sample of a mono recording.</summary>
   public void AddMono(float sample)
   {
      var filtered = _leftFilter.Filter(sample * MonoSampleScale);
      Add(filtered, filtered);
   }

   /// <summary>Measures the next pair of samples of a stereo recording.</summary>
   public void AddStereo(float left, float right) => Add(_leftFilter.Filter(left * StereoSampleScale), _rightFilter.Filter(right * StereoSampleScale));

   /// <summary>The gain that brings the recording measured so far to ReplayGain's reference loudness. Only when it's at least one window long.</summary>
   public ReplayGain Result() => new(ReferenceLoudness - _histogram.LoudnessOnlyFivePercentOfTheWindowsExceed());

   void Add(double filteredLeft, double filteredRight)
   {
      _leftSumOfSquares += filteredLeft * filteredLeft;
      _rightSumOfSquares += filteredRight * filteredRight;
      _framesInTheWindow++;
      if(_framesInTheWindow == _framesPerWindow) CountTheWindow();
   }

   void CountTheWindow()
   {
      _histogram.CountWindow((_leftSumOfSquares + _rightSumOfSquares) / _framesInTheWindow * 0.5);
      _leftSumOfSquares = 0;
      _rightSumOfSquares = 0;
      _framesInTheWindow = 0;
   }
}
