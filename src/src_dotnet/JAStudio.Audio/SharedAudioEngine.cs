using System;
using SoundFlow.Backends.MiniAudio;

namespace JAStudio.Audio;

/// <summary>
/// JAStudio's SoundFlow engine, which <see cref="SoundFlowAudioPlayer"/> plays with and <see cref="DecodedAudio"/> decodes with.<br/>
/// SoundFlow is a cross-platform .NET audio library, built on miniaudio, a native library it brings for each platform.<br/>
/// <c>Mp3LoudnessNormalization</c> decodes with an engine of its own.
/// </summary>
/// <remarks>It's made on first use, and lives as long as the process: it holds no device until something plays.</remarks>
static class SharedAudioEngine
{
   static readonly Lazy<MiniAudioEngine> TheEngine = new(() => new MiniAudioEngine());

   public static MiniAudioEngine Instance => TheEngine.Value;
}
