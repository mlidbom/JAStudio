using System;
using System.Collections.Generic;
using System.IO;

namespace JAStudio.Audio;

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

   const int SamplesReadAtATime = 64 * 1024;

   /// <summary>The samples, from -1 to 1, a frame at a time: one sample per channel per frame.</summary>
   readonly float[] _samples;

   /// <summary>How many samples a second of the audio holds, counting every channel's.</summary>
   readonly int _samplesPerSecond;

   DecodedAudio(float[] samples, int samplesPerSecond)
   {
      _samples = samples;
      _samplesPerSecond = samplesPerSecond;
   }

   /// <summary>The kinds of audio file SoundFlow decodes, and so plays: those miniaudio, the native library under it, decodes itself.</summary>
   static readonly string[] DecodableFileExtensions = [".mp3", ".wav", ".flac"];

   /// <summary>
   /// Whether the audio file named <paramref name="fileName"/> is of a kind SoundFlow decodes, and so plays: an MP3, WAV or FLAC file.<br/>
   /// Others, such as the Ogg Vorbis audio of a few imported sentences, would need a codec SoundFlow brings in a separate package.
   /// </summary>
   public static bool CanDecode(string fileName) => DecodableFileExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

   /// <summary>
   /// Decodes <paramref name="audioFile"/>, the bytes of a file <see cref="CanDecode"/> says it can, with SoundFlow (<see cref="SharedAudioEngine"/>),<br/>
   /// to the last sample the decoder gives: the length it says the audio has is an estimate, too high for some MP3s.
   /// </summary>
   public static DecodedAudio Of(byte[] audioFile)
   {
      using var decoder = new SoundFlow.Providers.StreamDataProvider(SharedAudioEngine.Instance, new MemoryStream(audioFile));
      var channels = decoder.FormatInfo?.ChannelCount ?? throw new InvalidDataException("SoundFlow found no audio format in the file.");
      var samples = new List<float>();
      var buffer = new float[SamplesReadAtATime];
      while(decoder.ReadBytes(buffer) is var read and > 0) samples.AddRange(buffer.AsSpan(0, read));
      return new DecodedAudio([..samples], decoder.SampleRate * channels);
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
