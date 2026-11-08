using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mp3LoudnessNormalization.Frames;
using Mp3LoudnessNormalization.Measurement;

namespace Mp3LoudnessNormalization;

/// <summary>
/// An MP3 file, read the way mp3gain reads it: as the audio frames between its tags, for measuring how loud it sounds, as ReplayGain 1 measures it,<br/>
/// and for bringing it to ReplayGain's reference loudness without decoding or re-encoding it, by changing every frame's gains:<br/>
/// <see cref="NormalizedToTheReplayGainReference"/> does what <c>mp3gain /r /c</c> does, byte for byte.
/// </summary>
/// <remarks>
/// The frames are looked for between the tags, as mp3gain looks for them: after an ID3v2 tag at the start, and before an APE tag and an ID3v1 tag at the end<br/>
/// (<see cref="FramesBetweenTheTags"/>). Anything between frames is skipped, such as a tag in the middle of two MP3 files joined together,<br/>
/// and so are frames whose MPEG version or sample rate differ from the first frame's.<br/>
/// VBR header frames hold no audio, and mp3gain doesn't change their gains (<see cref="FramesMp3GainChanges"/>). Its decoder leaves out<br/>
/// the file's first frame when it's one, but decodes one further on, such as the second file's, as a frame of silence (<see cref="FramesMp3GainsDecoderReads"/>).
/// </remarks>
/// <remarks>
/// mp3gain measures the frames its decoder reads but the last, whose samples the decoder never puts out, so that's what <see cref="MeasureReplayGain"/> decodes.<br/>
/// On a two-second clip, one frame more can change which window is the loudest 5%, and the measurement by over 2 dB.<br/>
/// Compared with mp3gain 1.4.6 on 2026-11-07, on 7,565 files: 7,500 from JAStudio's media layer, and 65 TTS files from Azure and Google. Every file came out<br/>
/// byte for byte as mp3gain made it, but for ffmpeg-encoded clips whose first frame refers to audio data in a frame before it, which isn't there:<br/>
/// the decoders treat that broken start differently, so 121 of the 1,472 such files got a gain one step apart. mp3gain 1.4.6 also measures a file<br/>
/// whose frames have a CRC differently from the same audio without one, which this doesn't. See <c>dev_docs/mp3-loudness-normalization.md</c>.
/// </remarks>
public sealed class Mp3File
{
   const int Id3V2HeaderLength = 10;
   const int Id3V1Length = 128;
   const int ApeTagFooterLength = 32;
   const uint ApeTagHasAHeaderFlag = 1u << 31;

   readonly byte[] _bytes;

   /// <summary>The MP3 file whose bytes are <paramref name="bytes"/>, which it keeps a copy of.</summary>
   public Mp3File(byte[] bytes) => _bytes = [..bytes];

   /// <summary>A copy of the file's bytes.</summary>
   public byte[] Bytes() => [.._bytes];

   /// <summary>
   /// The gain that brings the file to ReplayGain 1's reference loudness, measured as mp3gain measures it: on the frames its decoder reads, but the last.
   /// </summary>
   public ReplayGain MeasureReplayGain() => DecodedMp3.Of(TheFramesMp3GainMeasures()).MeasureReplayGain();

   /// <summary>
   /// A copy of the file with the gains of every frame mp3gain changes changed by <paramref name="mp3GainSteps"/> (<see cref="Mp3Frame.ChangeGlobalGainsBy"/>).
   /// </summary>
   public Mp3File WithGainChangedBy(int mp3GainSteps)
   {
      var changed = new Mp3File(_bytes);
      foreach(var frame in changed.FramesMp3GainChanges()) frame.ChangeGlobalGainsBy(mp3GainSteps);
      return changed;
   }

   /// <summary>
   /// A copy of the file brought to ReplayGain 1's reference loudness, as <c>mp3gain /r /c</c> brings it: by the <see cref="ReplayGain.Mp3GainSteps"/><br/>
   /// <see cref="MeasureReplayGain"/> gives, even when that makes it clip. mp3gain's tags, which record what it changed, aren't written.
   /// </summary>
   public Mp3File NormalizedToTheReplayGainReference() => WithGainChangedBy(MeasureReplayGain().Mp3GainSteps);

   /// <summary>The frames mp3gain measures, the ones its decoder reads but the last, as an MP3 file without tags.</summary>
   byte[] TheFramesMp3GainMeasures()
   {
      using var frames = new MemoryStream();
      foreach(var frame in FramesMp3GainsDecoderReads().SkipLast(1)) frames.Write(_bytes, frame.Start, frame.Header.FrameLength);
      return frames.ToArray();
   }

   /// <summary>The frames mp3gain changes the gains of: the audio frames, but no VBR header frame. See the remarks of <see cref="Mp3File"/>.</summary>
   IEnumerable<Mp3Frame> FramesMp3GainChanges() => FramesBetweenTheTags().Where(frame => !frame.IsVbrHeader);

   /// <summary>The frames mp3gain's decoder reads: the audio frames, but the first when it's a VBR header frame. See the remarks of <see cref="Mp3File"/>.</summary>
   IEnumerable<Mp3Frame> FramesMp3GainsDecoderReads() => FramesBetweenTheTags().SkipWhile((frame, index) => index == 0 && frame.IsVbrHeader);

   /// <summary>The frames between the tags, VBR header frames included, found as mp3gain finds them. See the remarks of <see cref="Mp3File"/>.</summary>
   IEnumerable<Mp3Frame> FramesBetweenTheTags()
   {
      var audioEnd = AudioEnd();
      Mp3FrameHeader? first = null;
      var position = AudioStart();
      while(position + 4 <= audioEnd)
      {
         if(FrameAt(position, audioEnd) is not {} frame || first is not null && !frame.Header.BelongsWith(first))
         {
            position++;
            continue;
         }

         first ??= frame.Header;
         yield return frame;
         position = frame.End;
      }
      if(first is null) throw new InvalidDataException("The file holds no MP3 frames.");
   }

   /// <summary>The frame at <paramref name="position"/>, if a valid header is there and the frame ends by <paramref name="audioEnd"/>.</summary>
   Mp3Frame? FrameAt(int position, int audioEnd) =>
      Mp3FrameHeader.TryRead(_bytes, position) is {} header && position + header.FrameLength <= audioEnd ? new Mp3Frame(_bytes, position, header) : null;

   /// <summary>Where the audio starts: after an ID3v2 tag, whose header gives its length in four 7-bit bytes, or at the file's start.</summary>
   int AudioStart() =>
      _bytes.Length >= Id3V2HeaderLength && _bytes.AsSpan(0, 3).SequenceEqual("ID3"u8)
         ? Id3V2HeaderLength + ((_bytes[6] & 0x7F) << 21 | (_bytes[7] & 0x7F) << 14 | (_bytes[8] & 0x7F) << 7 | (_bytes[9] & 0x7F))
         : 0;

   /// <summary>Where the audio ends: before an ID3v1 tag, the last 128 bytes when they start with "TAG", and before an APE tag ending there.</summary>
   int AudioEnd()
   {
      var end = _bytes.Length;
      if(end >= Id3V1Length && _bytes.AsSpan(end - Id3V1Length, 3).SequenceEqual("TAG"u8)) end -= Id3V1Length;
      if(ApeTagStartingBefore(end) is {} apeTagStart) end = apeTagStart;
      return end;
   }

   /// <summary>Where an APE tag ending at <paramref name="end"/> starts, read from its 32-byte footer, or null when no APE tag ends there.</summary>
   int? ApeTagStartingBefore(int end)
   {
      var footer = end - ApeTagFooterLength;
      if(footer < 0 || !_bytes.AsSpan(footer, 8).SequenceEqual("APETAGEX"u8)) return null;

      var lengthWithTheFooter = (int)BinaryPrimitives.ReadUInt32LittleEndian(_bytes.AsSpan(footer + 12, 4));
      var flags = BinaryPrimitives.ReadUInt32LittleEndian(_bytes.AsSpan(footer + 20, 4));
      var start = end - lengthWithTheFooter - ((flags & ApeTagHasAHeaderFlag) != 0 ? ApeTagFooterLength : 0);
      return start >= 0 ? start : null;
   }

   /// <summary>Not the bytes, which would only fill a log.</summary>
   public override string ToString() => $"MP3 file of {_bytes.Length} bytes";
}
