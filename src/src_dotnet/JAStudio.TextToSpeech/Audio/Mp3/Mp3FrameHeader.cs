namespace JAStudio.TextToSpeech.Audio.Mp3;

/// <summary>
/// The four bytes that start every MP3 frame: a sync word, and how the frame is encoded, which says how long it is and where its side information,<br/>
/// holding its gains, lies. Only MPEG Layer III headers, the MP3 ones, are read: <see cref="TryRead"/> refuses the others.
/// </summary>
sealed class Mp3FrameHeader
{
   const int SamplesPerGranule = 576;
   static readonly int[] Mpeg1KilobitsPerSecond = [0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320];
   static readonly int[] Mpeg2KilobitsPerSecond = [0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160];
   static readonly int[] Mpeg1SampleRates = [44100, 48000, 32000];

   Mp3FrameHeader(MpegVersion version, bool isProtectedByCrc, int sampleRate, bool isMono, int frameLength)
   {
      Version = version;
      IsProtectedByCrc = isProtectedByCrc;
      SampleRate = sampleRate;
      IsMono = isMono;
      FrameLength = frameLength;
   }

   /// <summary>The header at <paramref name="position"/> of <paramref name="bytes"/>, or null when there's no valid MP3 frame header there.</summary>
   public static Mp3FrameHeader? TryRead(byte[] bytes, int position)
   {
      if(position + 4 > bytes.Length || !StartsWithASyncWord(bytes, position)) return null;

      MpegVersion? version = ((bytes[position + 1] >> 3) & 0b11) switch
      {
         0b11 => MpegVersion.Mpeg1,
         0b10 => MpegVersion.Mpeg2,
         0b00 => MpegVersion.Mpeg25,
         _ => null
      };
      var isLayer3 = ((bytes[position + 1] >> 1) & 0b11) == 0b01;
      var bitrateIndex = bytes[position + 2] >> 4;
      var sampleRateIndex = (bytes[position + 2] >> 2) & 0b11;
      if(version is not {} mpegVersion || !isLayer3 || bitrateIndex is 0 or 15 || sampleRateIndex == 3) return null;

      var sampleRate = Mpeg1SampleRates[sampleRateIndex] / HowManyTimesLowerThanMpeg1sItsSampleRatesAre(mpegVersion);
      var bitsPerSecond = (mpegVersion == MpegVersion.Mpeg1 ? Mpeg1KilobitsPerSecond : Mpeg2KilobitsPerSecond)[bitrateIndex] * 1000;
      var granules = mpegVersion == MpegVersion.Mpeg1 ? 2 : 1;
      var padding = (bytes[position + 2] >> 1) & 1;
      var frameLength = granules * SamplesPerGranule * bitsPerSecond / 8 / sampleRate + padding;
      return new Mp3FrameHeader(mpegVersion,
                                isProtectedByCrc: (bytes[position + 1] & 1) == 0,
                                sampleRate,
                                isMono: bytes[position + 3] >> 6 == 0b11,
                                frameLength);
   }

   /// <summary>MPEG-2 halves MPEG-1's sample rates, and MPEG-2.5 quarters them.</summary>
   static int HowManyTimesLowerThanMpeg1sItsSampleRatesAre(MpegVersion version) => version switch
   {
      MpegVersion.Mpeg1 => 1,
      MpegVersion.Mpeg2 => 2,
      _ => 4
   };

   /// <summary>Whether the 11 set bits every frame header starts with are at <paramref name="position"/> of <paramref name="bytes"/>.</summary>
   public static bool StartsWithASyncWord(byte[] bytes, int position) =>
      position + 2 <= bytes.Length && bytes[position] == 0xFF && (bytes[position + 1] & 0xE0) == 0xE0;

   public MpegVersion Version { get; }

   /// <summary>Whether a CRC checksum follows the header, covering the header's last two bytes and the side information.</summary>
   public bool IsProtectedByCrc { get; }

   public int SampleRate { get; }

   public bool IsMono { get; }

   /// <summary>How many bytes the frame takes, header included.</summary>
   public int FrameLength { get; }

   /// <summary>How many granules, of 576 samples per channel, the frame holds: each has its own gain per channel.</summary>
   public int Granules => Version == MpegVersion.Mpeg1 ? 2 : 1;

   public int Channels => IsMono ? 1 : 2;

   /// <summary>How many bytes of side information follow the header and CRC: how the frame's audio data is laid out, gains included.</summary>
   public int SideInformationLength => (Version, IsMono) switch
   {
      (MpegVersion.Mpeg1, true) => 17,
      (MpegVersion.Mpeg1, false) => 32,
      (_, true) => 9,
      (_, false) => 17
   };

   /// <summary>Where the side information starts, counted from the frame's start: after the header, and the CRC if there is one.</summary>
   public int SideInformationStart => IsProtectedByCrc ? 6 : 4;

   /// <summary>Whether a frame with this header belongs to the same stream as one with <paramref name="first"/>: the same MPEG version and sample rate.</summary>
   public bool BelongsWith(Mp3FrameHeader first) => Version == first.Version && SampleRate == first.SampleRate;
}
