namespace Mp3LoudnessNormalization.Frames;

/// <summary>
/// Which MPEG standard an MP3 frame follows, which decides its sample rates, how many samples it holds and how its side information is laid out.<br/>
/// MPEG-2 and MPEG-2.5 add the lower sample rates, such as the 24 kHz speech is often encoded at, with frames half as long.
/// </summary>
enum MpegVersion
{
   /// <summary>MPEG-1: 32, 44.1 and 48 kHz, 1152 samples per frame, in two granules.</summary>
   Mpeg1,

   /// <summary>MPEG-2: 16, 22.05 and 24 kHz, 576 samples per frame, in one granule.</summary>
   Mpeg2,

   /// <summary>MPEG-2.5, an unofficial extension: 8, 11.025 and 12 kHz, laid out as MPEG-2.</summary>
   Mpeg25
}
