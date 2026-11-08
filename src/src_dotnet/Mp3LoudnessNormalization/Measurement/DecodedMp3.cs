using System;
using System.Collections.Generic;
using System.IO;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Providers;

namespace Mp3LoudnessNormalization.Measurement;

/// <summary>
/// MP3 frames decoded to their samples, by miniaudio through SoundFlow, a cross-platform .NET audio library, for measuring how loud they sound<br/>
/// (<see cref="MeasureReplayGain"/>). Given the frames mp3gain's own decoder reads (<see cref="Mp3File"/>), they're the samples mp3gain measures.
/// </summary>
/// <remarks>
/// It decodes with a SoundFlow engine of its own, made on first use and kept as long as the process: it never plays, so it holds no device.
/// </remarks>
sealed class DecodedMp3
{
   const int SamplesReadAtATime = 64 * 1024;
   static readonly Lazy<MiniAudioEngine> Engine = new(() => new MiniAudioEngine());

   /// <summary>The samples, from -1 to 1, a frame at a time: one sample per channel per frame.</summary>
   readonly float[] _samples;

   readonly int _sampleRate;
   readonly int _channelCount;

   DecodedMp3(float[] samples, int sampleRate, int channelCount)
   {
      _samples = samples;
      _sampleRate = sampleRate;
      _channelCount = channelCount;
   }

   /// <summary>Decodes <paramref name="mp3Frames"/>, MP3 frames without tags, to the last sample the decoder gives.</summary>
   public static DecodedMp3 Of(byte[] mp3Frames)
   {
      using var decoder = new StreamDataProvider(Engine.Value, new MemoryStream(mp3Frames));
      var channelCount = decoder.FormatInfo?.ChannelCount ?? throw new InvalidDataException("SoundFlow found no audio format in the MP3 frames.");
      var samples = new List<float>();
      var buffer = new float[SamplesReadAtATime];
      while(decoder.ReadBytes(buffer) is var read and > 0) samples.AddRange(buffer.AsSpan(0, read));
      return new DecodedMp3([..samples], decoder.SampleRate, channelCount);
   }

   /// <summary>
   /// The gain that brings these samples to ReplayGain 1's reference loudness, measured as mp3gain measures samples (<see cref="ReplayGainMeasurement"/>).<br/>
   /// Only for mono and stereo audio at least 50 ms long.
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
            throw new InvalidDataException($"MP3 holds mono and stereo audio, not audio with {_channelCount} channels.");
      }
      return measurement.Result();
   }
}
