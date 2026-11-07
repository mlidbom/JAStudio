using System;
using SoundFlow.Backends.MiniAudio;

namespace JAStudio.TextToSpeech.Audio;

/// <summary>
/// The one SoundFlow engine of the process, which <see cref="SoundFlowAudioPlayer"/> plays with and <see cref="DecodedAudio"/> decodes with.<br/>
/// SoundFlow is a cross-platform .NET audio library, built on miniaudio, a native library it brings for each platform.
/// </summary>
/// <remarks>It's made on first use, and lives as long as the process: it holds no device until something plays.</remarks>
static class SharedAudioEngine
{
   static readonly Lazy<MiniAudioEngine> TheEngine = new(() => new MiniAudioEngine());

   public static MiniAudioEngine Instance => TheEngine.Value;
}
