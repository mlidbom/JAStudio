namespace Mp3LoudnessNormalization.Specifications;

/// <summary>Clips of a few frames cut from an MP3 fixture, as subs2srs cuts very short lines out of an episode's audio.</summary>
static class Mp3Clips
{
   /// <summary>The first <paramref name="frameCount"/> frames of <paramref name="mp3"/>, alone: no tags, nothing after them.</summary>
   public static byte[] TheFirstFramesOf(byte[] mp3, int frameCount)
   {
      var start = Mp3StartInspection.FirstSyncWordFrom(mp3, Mp3StartInspection.AfterAnId3V2Tag(mp3));
      var end = start;
      for(var frame = 0; frame < frameCount; frame++) end += Mp3StartInspection.FrameLength(mp3, end);
      return mp3[start..end];
   }
}
