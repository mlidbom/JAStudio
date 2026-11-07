using System;
using System.IO;

namespace JAStudio.TextToSpeech.Audio;

/// <summary>
/// An audio file decoded to its samples, for telling whether two attempts at speaking a text sound the same,<br/>
/// which comparing their files can't tell: Azure's standard voices send slightly different bytes for the same request every time.
/// </summary>
/// <remarks>
/// Two decodings sound the same when they're about as long as each other, and the sample-by-sample difference between them is small next to<br/>
/// the audio itself. Measured on 2026-11-07, with the difference relative to the audio's strength: two of Azure's renditions of one text with a<br/>
/// standard voice differed by 0.025, and identical audio by 0. Two renditions with an HD voice differed by 1.5, and two different voices by 1.2.<br/>
/// So "the same" is below <see cref="LargestRelativeDifferenceThatSoundsTheSame"/>, with a wide margin on both sides.
/// </remarks>
public sealed class DecodedAudio
{
   /// <summary>See the remarks of <see cref="DecodedAudio"/>.</summary>
   const double LargestRelativeDifferenceThatSoundsTheSame = 0.2;

   /// <summary>How much longer one decoding may be than the other and still sound the same: 1%, about 20 ms for a short sentence.</summary>
   const double LargestRelativeDifferenceInLengthThatSoundsTheSame = 0.01;

   readonly float[] _samples;
   readonly int _samplesPerSecond;

   DecodedAudio(float[] samples, int samplesPerSecond)
   {
      _samples = samples;
      _samplesPerSecond = samplesPerSecond;
   }

   /// <summary>Decodes <paramref name="audioFile"/>, the bytes of an MP3, WAV or FLAC file, with SoundFlow (<see cref="SharedAudioEngine"/>).</summary>
   public static DecodedAudio Of(byte[] audioFile)
   {
      using var decoder = new SoundFlow.Providers.StreamDataProvider(SharedAudioEngine.Instance, new MemoryStream(audioFile));
      var channels = decoder.FormatInfo?.ChannelCount ?? throw new InvalidDataException("SoundFlow found no audio format in the file.");
      var samples = new float[decoder.Length];
      var read = 0;
      while(read < samples.Length && decoder.ReadBytes(samples.AsSpan(read)) is var readNow and > 0)
         read += readNow;
      return new DecodedAudio(samples[..read], decoder.SampleRate * channels);
   }

   /// <summary>How long the audio lasts.</summary>
   public TimeSpan Duration => TimeSpan.FromSeconds((double)_samples.Length / _samplesPerSecond);

   /// <summary>Whether this and <paramref name="other"/> sound the same. See the remarks of <see cref="DecodedAudio"/>.</summary>
   public bool SoundsTheSameAs(DecodedAudio other) =>
      _samplesPerSecond == other._samplesPerSecond
   && Math.Abs(_samples.Length - other._samples.Length) <= LargestRelativeDifferenceInLengthThatSoundsTheSame * _samples.Length
   && RelativeDifferenceFrom(other) < LargestRelativeDifferenceThatSoundsTheSame;

   /// <summary>The strength of the sample-by-sample difference from <paramref name="other"/>, over the samples both have, relative to this audio's strength.</summary>
   double RelativeDifferenceFrom(DecodedAudio other)
   {
      double differenceEnergy = 0, energy = 0;
      for(var index = 0; index < Math.Min(_samples.Length, other._samples.Length); index++)
      {
         var difference = _samples[index] - other._samples[index];
         differenceEnergy += difference * difference;
         energy += _samples[index] * (double)_samples[index];
      }
      return Math.Sqrt(differenceEnergy / energy);
   }
}
