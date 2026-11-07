using System;
using System.IO;
using System.Text;

namespace JAStudio.TextToSpeech.Specifications;

/// <summary>
/// WAV files made in code, for specifying how audio is compared and played without keeping recordings in the repository:<br/>
/// a tone of a pitch and a length, mono, 16-bit, at 24 kHz, the sample rate of the tts-providers' speech, optionally with a little noise.
/// </summary>
public static class GeneratedWav
{
   const int SamplesPerSecond = 24_000;

   /// <param name="hertz">The pitch.</param>
   /// <param name="seconds">How long it lasts.</param>
   /// <param name="noise">How strong a random noise is added, relative to the tone: 0.01 is 1% of its strength.</param>
   /// <param name="noiseSeed">Which random noise: two tones with the same noise and seed are identical.</param>
   public static byte[] Tone(double hertz, double seconds, double noise = 0, int noiseSeed = 0)
   {
      var random = new Random(noiseSeed);
      var sampleCount = (int)(seconds * SamplesPerSecond);
      using var wav = new MemoryStream();
      using var writer = new BinaryWriter(wav, Encoding.ASCII);
      writer.Write("RIFF"u8.ToArray());
      writer.Write(36 + sampleCount * 2);
      writer.Write("WAVEfmt "u8.ToArray());
      writer.Write(16);
      writer.Write((short)1);
      writer.Write((short)1);
      writer.Write(SamplesPerSecond);
      writer.Write(SamplesPerSecond * 2);
      writer.Write((short)2);
      writer.Write((short)16);
      writer.Write("data"u8.ToArray());
      writer.Write(sampleCount * 2);
      for(var index = 0; index < sampleCount; index++)
      {
         var sample = Math.Sin(2 * Math.PI * hertz * index / SamplesPerSecond) + noise * (random.NextDouble() * 2 - 1);
         writer.Write((short)(Math.Clamp(sample * 0.5, -1, 1) * short.MaxValue));
      }
      writer.Flush();
      return wav.ToArray();
   }
}
