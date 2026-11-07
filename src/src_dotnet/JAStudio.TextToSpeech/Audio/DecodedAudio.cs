using System;
using System.IO;
using JAStudio.TextToSpeech.Audio.Loudness;

namespace JAStudio.TextToSpeech.Audio;

/// <summary>
/// An audio file decoded to its samples, for telling whether two attempts at speaking a text sound the same,<br/>
/// which comparing their files can't tell: Azure's standard voices send slightly different bytes for the same request every time,<br/>
/// and for measuring how loud it sounds (<see cref="MeasureReplayGain"/>).
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

   /// <summary>The samples, from -1 to 1, a frame at a time: one sample per channel per frame.</summary>
   readonly float[] _samples;

   readonly int _sampleRate;
   readonly int _channelCount;

   DecodedAudio(float[] samples, int sampleRate, int channelCount)
   {
      _samples = samples;
      _sampleRate = sampleRate;
      _channelCount = channelCount;
   }

   /// <summary>The kinds of audio file SoundFlow decodes, and so plays: those miniaudio, the native library under it, decodes itself.</summary>
   static readonly string[] DecodableFileExtensions = [".mp3", ".wav", ".flac"];

   /// <summary>
   /// Whether the audio file named <paramref name="fileName"/> is of a kind SoundFlow decodes, and so plays: an MP3, WAV or FLAC file.<br/>
   /// Others, such as the Ogg Vorbis audio of a few imported sentences, would need a codec SoundFlow brings in a separate package.
   /// </summary>
   public static bool CanDecode(string fileName) => DecodableFileExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

   /// <summary>Decodes <paramref name="audioFile"/>, the bytes of a file <see cref="CanDecode"/> says it can, with SoundFlow (<see cref="SharedAudioEngine"/>).</summary>
   public static DecodedAudio Of(byte[] audioFile)
   {
      using var decoder = new SoundFlow.Providers.StreamDataProvider(SharedAudioEngine.Instance, new MemoryStream(audioFile));
      var channels = decoder.FormatInfo?.ChannelCount ?? throw new InvalidDataException("SoundFlow found no audio format in the file.");
      var samples = new float[decoder.Length];
      var read = 0;
      while(read < samples.Length && decoder.ReadBytes(samples.AsSpan(read)) is var readNow and > 0)
         read += readNow;
      return new DecodedAudio(samples[..read], decoder.SampleRate, channels);
   }

   /// <summary>How long the audio lasts.</summary>
   public TimeSpan Duration => TimeSpan.FromSeconds((double)_samples.Length / SamplesPerSecond);

   int SamplesPerSecond => _sampleRate * _channelCount;

   /// <summary>
   /// The gain that brings these samples to ReplayGain 1's reference loudness, measured as mp3gain measures samples (<see cref="ReplayGainMeasurement"/>).<br/>
   /// Which samples of an MP3 file mp3gain measures is <see cref="Mp3.Mp3File.MeasureReplayGain"/>'s business.<br/>
   /// Only for mono and stereo audio at least 50 ms long, at a sample rate MP3 allows.
   /// </summary>
   public ReplayGain MeasureReplayGain()
   {
      var measurement = new ReplayGainMeasurement(_sampleRate);
      switch(_channelCount)
      {
         case 1:
            foreach(var sample in _samples) measurement.AddMono(sample);
            break;
         case 2:
            for(var frameStart = 0; frameStart < _samples.Length; frameStart += 2) measurement.AddStereo(_samples[frameStart], _samples[frameStart + 1]);
            break;
         default:
            throw new InvalidOperationException($"ReplayGain measures mono and stereo audio, not audio with {_channelCount} channels.");
      }
      return measurement.Result();
   }

   /// <summary>Whether this and <paramref name="other"/> sound the same. See the remarks of <see cref="DecodedAudio"/>.</summary>
   public bool SoundsTheSameAs(DecodedAudio other) =>
      SamplesPerSecond == other.SamplesPerSecond
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
