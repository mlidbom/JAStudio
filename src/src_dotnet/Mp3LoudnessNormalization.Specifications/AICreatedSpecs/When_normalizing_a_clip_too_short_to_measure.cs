using Compze.Must;
using Compze.xUnitBDD;

// ReSharper disable InconsistentNaming

namespace Mp3LoudnessNormalization.Specifications.AICreatedSpecs;

/// <summary>
/// Clips too short to measure, as subs2srs cuts for very short lines, made of the first frames of a LAME fixture, whose first frame is its Info<br/>
/// header frame: <see cref="Mp3File.MeasureReplayGainOrNone"/> gives none, and normalizing leaves the clip as it is, as mp3gain leaves a file it<br/>
/// can't measure.
/// </summary>
public class When_normalizing_a_clip_too_short_to_measure : IAIGeneratedSpec
{
   const string TheFixture = "lame-32khz-stereo";

   static byte[] TheFirstFrames(int frameCount) => Mp3Clips.TheFirstFramesOf(Mp3Fixtures.Original(TheFixture), frameCount);

   /// <summary>Two frames: the Info header frame, and one frame of audio, the last, which mp3gain doesn't measure. The decoder refuses nothing to decode.</summary>
   public class with_no_frame_mp3gain_measures : When_normalizing_a_clip_too_short_to_measure
   {
      [XF] public void it_cant_be_measured() => (new Mp3File(TheFirstFrames(2)).MeasureReplayGainOrNone() is null).Must().BeTrue();
      [XF] public void normalizing_leaves_it_as_it_is() => new Mp3File(TheFirstFrames(2)).NormalizedToTheReplayGainReference().Bytes().Must().SequenceEqual(TheFirstFrames(2));
   }

   /// <summary>Three frames: one of audio is measured, 36 ms at 32 kHz, shorter than one of ReplayGain's windows, 50 ms.</summary>
   public class with_less_audio_than_one_window : When_normalizing_a_clip_too_short_to_measure
   {
      [XF] public void it_cant_be_measured() => (new Mp3File(TheFirstFrames(3)).MeasureReplayGainOrNone() is null).Must().BeTrue();
      [XF] public void normalizing_leaves_it_as_it_is() => new Mp3File(TheFirstFrames(3)).NormalizedToTheReplayGainReference().Bytes().Must().SequenceEqual(TheFirstFrames(3));
   }
}
