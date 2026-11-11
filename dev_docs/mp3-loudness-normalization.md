# Mp3LoudnessNormalization

A library that brings MP3 files to ReplayGain's reference loudness the way `mp3gain /r /c` does, byte for byte, without
decoding or re-encoding them. It knows nothing of JAStudio: JAStudio uses it to normalize the speech its voices give it
(see [text-to-speech.md](text-to-speech.md#normalizing)), and it could move to a repository or NuGet package of its own.

Projects: `src/src_dotnet/Mp3LoudnessNormalization`, and `Mp3LoudnessNormalization.Specifications`.

## What it does

`Mp3File` is the whole public surface, with the `ReplayGain` it measures:

1. **Measure** how loud the file sounds, with ReplayGain 1's algorithm (`Measurement`, `ReplayGainMeasurement`):
   an equal-loudness filter shaped like human hearing (`EqualLoudnessFilter`), the loudness of each 50 ms window, and
   the loudness only 5% of the windows exceed (`LoudnessHistogram`). The result is the gain to the reference, 89 dB
   (`ReplayGain`).
2. **Change** every frame's gains by that gain, rounded to whole steps of 1.5 dB, a step being what one unit of an MP3
   frame's global gain changes the volume by (`Frames`, `Mp3Frame`). Nothing is decoded or re-encoded, so nothing is
   lost. Clipping is ignored, as `-c` does. mp3gain's tags, which record what it changed, aren't written.

## Why ReplayGain 1, not EBU R128

EBU R128 (ITU BS.1770) is the standard today, used by broadcasters, streaming services and ReplayGain 2, and generally
considered a little more accurate, though no listening study comparing the two directly turned up (searched
2026-11-07). ReplayGain 1 is what mp3gain uses, and ~30,000 of JAStudio's files were normalized by mp3gain, so new files
should match them. Switching later is possible, and lossless, since it's only gains.

## Where it comes from

No .NET library does this (searched 2026-11-07). The code follows ReplayGain's published algorithm and mp3gain's frame
handling, with mp3rgain, an MIT-licensed Rust port verified against mp3gain, as the reference
(<https://github.com/M-Igashi/mp3rgain>). The filter coefficients are generated from its copy of ReplayGain's tables
(`EqualLoudnessFilter.CoefficientsBySampleRate.cs`).

## What mp3gain measures

Its decoder (mpglib, from 2004) and the one used here (miniaudio, through SoundFlow) give different samples unless given
exactly the frames mpglib reads, which took finding out (`Mp3File`):

- the frames between the tags, after an ID3v2 tag and before an APE tag and an ID3v1 tag, skipping anything between
  frames, such as a tag in the middle of two files joined together, and frames of another MPEG version or sample rate;
- not a VBR header frame at the start, whose LAME data would make miniaudio trim the encoder's padding. One further on,
  such as a joined file's second, mpglib decodes as a frame of silence;
- not the last frame, which mpglib never puts out. On a two-second clip one frame decides which window is the loudest 5%,
  so it moved the result by up to 2.5 dB.

mp3gain changes the gains of the same frames, but no VBR header frame. The library decodes with a SoundFlow engine of
its own (`DecodedMp3`), and reads until the decoder gives no more samples.

## Verified

Against mp3gain 1.4.6, the version Magnus's normalizing script uses:

- **12 fixtures,** with mp3gain's results beside them (`Mp3LoudnessNormalization.Specifications/Mp3Fixtures`): 8
  encoded by `dev_scripts/generate-mp3-fixtures.cs` with `lame.exe`, and 4 TTS files from Azure and Google. They cover
  MPEG-1, -2 and -2.5, mono and stereo, CBR and VBR, a CRC, ID3 and APE tags, silent granules, and two files joined by a
  tag. Specified in `When_normalizing_mp3_files_to_the_replaygain_reference`, in the regular suite.
- **JAStudio's media layer,** up to 300 files per folder, by an explicit specification, run on purpose:
  `MP3GAIN=<path to mp3gain.exe> dotnet test src/src_dotnet/Mp3LoudnessNormalization.Specifications --filter FullyQualifiedName~media_layers -- xUnit.Explicit=only`
  (`When_normalizing_the_media_layers_mp3_files_as_mp3gain_does`). It takes under a minute.
- **On 2026-11-07, 7,565 files,** 7,500 from the media layer and 65 TTS files: every one came out byte for byte as mp3gain
  made it, but for the exceptions below.

## Known exceptions

- **A file starting with a frame whose audio data is missing,** as in clips ffmpeg cut, whose first frame's data was in
  a frame ffmpeg overwrote with its VBR header. mpglib drops only that frame, miniaudio drops every frame lacking data,
  so the windows shift. 121 of the 1,472 such files in the media layer, all anime clips but two, got a gain one step
  apart. TTS files don't start so. Making miniaudio drop frames as mpglib does was tried and failed.
- **A file whose frames have a CRC:** mp3gain 1.4.6 measures it differently from the same audio without one, which looks
  like a bug in its decoder, and the library doesn't copy it. Its frames' gains and CRCs are changed exactly as mp3gain
  changes them. None turned up in the media layer.
- **A clip too short to measure** (2026-11-11): one whose frames, but the last, hold less than one of ReplayGain's
  windows, 50 ms, or one miniaudio refuses to decode, as it refuses a stream of no frames or very few. subs2srs cuts
  such clips for very short lines: 9 of about 30,000 MP3s in five decks of the reservoir. `Mp3File.MeasureReplayGainOrNone`
  gives none for one, and `NormalizedToTheReplayGainReference` leaves it as it is, as mp3gain leaves a file it can't
  measure. Until then both threw. mp3gain's own decoder may well measure some of the clips miniaudio refuses, which
  then stay unnormalized here.
