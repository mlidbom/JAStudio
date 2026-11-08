namespace Mp3LoudnessNormalization.Measurement;

partial class EqualLoudnessFilter
{
   /// <summary>
   /// The last few samples a filter took in, or put out, which its next output depends on. Zeros until that many samples have passed,<br/>
   /// as in ReplayGain's reference implementation, where a recording starts from silence.
   /// </summary>
   sealed class SampleHistory(int length)
   {
      readonly double[] _samples = new double[length];
      int _latestIndex;

      /// <summary>Remembers <paramref name="sample"/> as the latest, forgetting the oldest.</summary>
      public void Add(double sample)
      {
         _latestIndex = (_latestIndex + 1) % _samples.Length;
         _samples[_latestIndex] = sample;
      }

      /// <summary>The sample <paramref name="count"/> samples back: 1 is the latest added, and the history's length the oldest it remembers.</summary>
      public double SamplesBack(int count) => _samples[(_latestIndex - (count - 1) + _samples.Length) % _samples.Length];
   }
}
