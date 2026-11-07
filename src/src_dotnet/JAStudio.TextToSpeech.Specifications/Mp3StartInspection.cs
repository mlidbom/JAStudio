using System.Text;

namespace JAStudio.TextToSpeech.Specifications;

/// <summary>
/// Tells whether an MP3 file starts with a frame whose audio data is missing: the first audio frame says its data begins in a frame before it,<br/>
/// which isn't there, as in clips cut from a longer MP3 by ffmpeg, which overwrites that frame with a VBR header frame.<br/>
/// mp3gain's decoder and miniaudio treat that broken start differently, so on such files JAStudio's measurement may differ from mp3gain's.
/// </summary>
/// <remarks>
/// It reads only as much of the frames as it needs: the first frame's header, skipping it when it's a VBR header frame,<br/>
/// and the next frame's main_data_begin, the side information's first 9 bits in MPEG-1 and 8 in MPEG-2 and 2.5, which is how many bytes back its data begins.
/// </remarks>
static class Mp3StartInspection
{
   static readonly int[] Mpeg1KilobitsPerSecond = [0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320];
   static readonly int[] Mpeg2KilobitsPerSecond = [0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160];
   static readonly int[] Mpeg1SampleRates = [44100, 48000, 32000];

   public static bool StartsWithAFrameWhoseAudioDataIsMissing(byte[] mp3)
   {
      var position = FirstSyncWordFrom(mp3, AfterAnId3V2Tag(mp3));
      if(position < 0) return false;
      if(IsVbrHeaderFrame(mp3, position)) position = FirstSyncWordFrom(mp3, position + FrameLength(mp3, position));
      if(position < 0 || position + 6 >= mp3.Length) return false;

      var sideInformation = position + ((mp3[position + 1] & 1) == 0 ? 6 : 4);
      var mainDataBegin = IsMpeg1(mp3, position) ? (mp3[sideInformation] << 1) | (mp3[sideInformation + 1] >> 7) : mp3[sideInformation];
      return mainDataBegin > 0;
   }

   static int AfterAnId3V2Tag(byte[] mp3) =>
      mp3.Length > 10 && mp3[0] == 'I' && mp3[1] == 'D' && mp3[2] == '3' ? 10 + ((mp3[6] & 0x7F) << 21 | (mp3[7] & 0x7F) << 14 | (mp3[8] & 0x7F) << 7 | (mp3[9] & 0x7F)) : 0;

   static int FirstSyncWordFrom(byte[] mp3, int position)
   {
      for(; position + 4 <= mp3.Length; position++)
         if(mp3[position] == 0xFF && (mp3[position + 1] & 0xE0) == 0xE0) return position;
      return -1;
   }

   static bool IsMpeg1(byte[] mp3, int position) => ((mp3[position + 1] >> 3) & 3) == 3;

   static bool IsVbrHeaderFrame(byte[] mp3, int position)
   {
      var isMono = mp3[position + 3] >> 6 == 3;
      var sideInformationLength = (IsMpeg1(mp3, position), isMono) switch
      {
         (true, true) => 17,
         (true, false) => 32,
         (false, true) => 9,
         (false, false) => 17
      };
      var markerStart = position + 4 + sideInformationLength;
      return markerStart + 4 <= mp3.Length && Encoding.ASCII.GetString(mp3, markerStart, 4) is "Xing" or "Info";
   }

   static int FrameLength(byte[] mp3, int position)
   {
      var versionBits = (mp3[position + 1] >> 3) & 3;
      var howManyTimesLowerThanMpeg1s = versionBits switch
      {
         3 => 1,
         2 => 2,
         _ => 4
      };
      var sampleRate = Mpeg1SampleRates[(mp3[position + 2] >> 2) & 3] / howManyTimesLowerThanMpeg1s;
      var bitsPerSecond = (versionBits == 3 ? Mpeg1KilobitsPerSecond : Mpeg2KilobitsPerSecond)[mp3[position + 2] >> 4] * 1000;
      return (versionBits == 3 ? 144 : 72) * bitsPerSecond / sampleRate + ((mp3[position + 2] >> 1) & 1);
   }
}
