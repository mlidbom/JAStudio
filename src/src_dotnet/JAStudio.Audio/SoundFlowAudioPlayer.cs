using System;
using System.IO;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace JAStudio.Audio;

/// <summary>
/// Plays audio files on the computer's default output with SoundFlow (<see cref="SharedAudioEngine"/>), on every platform JAStudio runs on.<br/>
/// Playing a file stops the one playing. It's used from the UI thread, one call at a time.
/// </summary>
/// <remarks>
/// Each file gets a playback device of its own, opened in the file's own format. Opening one took 9 to 27 ms when tried (2026-11-07).<br/>
/// A file that has finished playing keeps its device until the next file plays, or the player is disposed, since disposing a device<br/>
/// from SoundFlow's notification that playback ended would be from SoundFlow's own audio thread.
/// </remarks>
public sealed class SoundFlowAudioPlayer : IAudioPlayer, IDisposable
{
   FilePlaying? _playing;

   public void Play(byte[] audioFile)
   {
      _playing?.Dispose();
      _playing = new FilePlaying(audioFile);
   }

   public void Dispose() => _playing?.Dispose();

   /// <summary>A file playing, or played, on a device of its own, from its decoder through a player.</summary>
   sealed class FilePlaying : IDisposable
   {
      readonly StreamDataProvider _decoder;
      readonly AudioPlaybackDevice _device;
      readonly SoundPlayer _player;

      public FilePlaying(byte[] audioFile)
      {
         var engine = SharedAudioEngine.Instance;
         _decoder = new StreamDataProvider(engine, new MemoryStream(audioFile));
         var format = new AudioFormat
                      {
                         Format = SampleFormat.F32,
                         SampleRate = _decoder.SampleRate,
                         Channels = _decoder.FormatInfo?.ChannelCount ?? throw new InvalidDataException("SoundFlow found no audio format in the file.")
                      };
         _device = engine.InitializePlaybackDevice(null, format);
         _player = new SoundPlayer(engine, format, _decoder);
         _device.MasterMixer.AddComponent(_player);
         _device.Start();
         _player.Play();
      }

      public void Dispose()
      {
         _device.Stop();
         _player.Dispose();
         _device.Dispose();
         _decoder.Dispose();
      }
   }
}
