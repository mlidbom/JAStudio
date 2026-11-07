using System;
using System.Text;

namespace JAStudio.TextToSpeech.Audio.Mp3;

/// <summary>
/// One frame of an MP3 file: a header, then side information, then compressed audio, of 1152 or 576 samples per channel.<br/>
/// Each granule of each channel has a global gain in the side information, which scales all its samples:<br/>
/// one step multiplies them by 2^¼, about 1.5 dB, so changing it changes the volume without decoding or re-encoding anything.
/// </summary>
/// <remarks>
/// The frame works on the bytes of the file it's in, at <see cref="Start"/>: changing its gains changes those bytes.
/// </remarks>
sealed class Mp3Frame
{
   const int HighestGlobalGain = 255;

   readonly byte[] _fileBytes;

   public Mp3Frame(byte[] fileBytes, int start, Mp3FrameHeader header)
   {
      _fileBytes = fileBytes;
      Start = start;
      Header = header;
   }

   /// <summary>Where the frame starts in its file.</summary>
   public int Start { get; }

   public Mp3FrameHeader Header { get; }

   /// <summary>Where the next frame starts, if one does.</summary>
   public int End => Start + Header.FrameLength;

   /// <summary>
   /// Whether this is a VBR header frame, which encoders such as LAME put first: it holds no audio, but the file's length and,<br/>
   /// for LAME, how many silent samples the encoder added at both ends. It says so with "Xing" or "Info" right after the side information.
   /// </summary>
   public bool IsVbrHeader
   {
      get
      {
         var markerStart = Start + 4 + Header.SideInformationLength;
         if(markerStart + 4 > _fileBytes.Length) return false;

         var marker = Encoding.ASCII.GetString(_fileBytes, markerStart, 4);
         return marker is "Xing" or "Info";
      }
   }

   /// <summary>
   /// Changes every global gain in the frame by <paramref name="steps"/>, as mp3gain does: a gain stops at 0 and 255 rather than wrapping around,<br/>
   /// and a gain of 0, which encoders give silent granules, stays 0, so that nothing left behind in a silent granule becomes audible.<br/>
   /// The CRC, if the frame has one, is computed again, since the gains are in the part it covers.
   /// </summary>
   public void ChangeGlobalGainsBy(int steps)
   {
      for(var granule = 0; granule < Header.Granules; granule++)
         for(var channel = 0; channel < Header.Channels; channel++)
         {
            var gainBit = GlobalGainBit(granule, channel);
            var gain = ReadByteAtBit(gainBit);
            if(gain != 0) WriteByteAtBit(gainBit, (byte)Math.Clamp(gain + steps, 0, HighestGlobalGain));
         }
      if(Header.IsProtectedByCrc) WriteTheCrc();
   }

   /// <summary>
   /// Where, counted in bits from the frame's start, the global gain of <paramref name="granule"/> of <paramref name="channel"/> is.<br/>
   /// The side information starts with where the audio data begins and a few bits per channel, then has one block per granule per channel,<br/>
   /// each with the length of its audio data (12 bits) and its count of big values (9 bits) before its global gain (8 bits).
   /// </summary>
   int GlobalGainBit(int granule, int channel)
   {
      var bitsBeforeTheFirstBlock = (Header.Version, Header.IsMono) switch
      {
         (MpegVersion.Mpeg1, true) => 18,
         (MpegVersion.Mpeg1, false) => 20,
         (_, true) => 9,
         (_, false) => 10
      };
      var bitsPerBlock = Header.Version == MpegVersion.Mpeg1 ? 59 : 63;
      const int bitsBeforeTheGlobalGain = 12 + 9;
      var block = granule * Header.Channels + channel;
      return Header.SideInformationStart * 8 + bitsBeforeTheFirstBlock + block * bitsPerBlock + bitsBeforeTheGlobalGain;
   }

   int ReadByteAtBit(int bit)
   {
      var index = Start + bit / 8;
      var shift = bit % 8;
      return shift == 0 ? _fileBytes[index] : (byte)((_fileBytes[index] << shift) | (_fileBytes[index + 1] >> (8 - shift)));
   }

   void WriteByteAtBit(int bit, byte value)
   {
      var index = Start + bit / 8;
      var shift = bit % 8;
      if(shift == 0)
      {
         _fileBytes[index] = value;
         return;
      }
      _fileBytes[index] = (byte)((_fileBytes[index] & (0xFF << (8 - shift))) | (value >> shift));
      _fileBytes[index + 1] = (byte)((_fileBytes[index + 1] & (0xFF >> shift)) | (value << (8 - shift)));
   }

   /// <summary>
   /// Writes the CRC-16 of the header's last two bytes and the side information after the header, as encoders do and mp3gain does:<br/>
   /// polynomial 0x8005, starting from 0xFFFF, big-endian.
   /// </summary>
   void WriteTheCrc()
   {
      var crc = 0xFFFF;
      Add(_fileBytes[Start + 2]);
      Add(_fileBytes[Start + 3]);
      for(var index = 0; index < Header.SideInformationLength; index++) Add(_fileBytes[Start + 6 + index]);
      _fileBytes[Start + 4] = (byte)(crc >> 8);
      _fileBytes[Start + 5] = (byte)crc;
      return;

      void Add(byte value)
      {
         crc ^= value << 8;
         for(var bit = 0; bit < 8; bit++) crc = (crc & 0x8000) != 0 ? ((crc << 1) ^ 0x8005) & 0xFFFF : (crc << 1) & 0xFFFF;
      }
   }
}
