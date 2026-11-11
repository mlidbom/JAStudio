using System;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using Mp3LoudnessNormalization;
using Xunit;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace Mp3LoudnessNormalization.Specifications.AICreatedSpecs;

/// <summary>
/// Bringing MP3 files to ReplayGain's reference loudness as <c>mp3gain /r /c</c> does (<see cref="Mp3File"/>),<br/>
/// compared with what mp3gain 1.4.6 made of the same files (<see cref="Mp3Fixtures"/>). They cover MPEG-1, -2 and -2.5, mono and stereo, CBR and VBR,<br/>
/// a CRC, ID3 and APE tags, silent granules, two files joined with a tag between them, and TTS audio from Azure and Google.
/// </summary>
/// <remarks>
/// mp3gain 1.4.6 measures a file whose frames have a CRC differently from the same audio without one, which <see cref="Mp3File"/> doesn't copy.<br/>
/// So the fixture with a CRC, <c>lame-crc-32khz-stereo</c>, is measured against its twin without one, <c>lame-32khz-stereo</c>.
/// </remarks>
public class When_normalizing_mp3_files_to_the_replaygain_reference : IAIGeneratedSpec
{
   const string TheFixtureWithACrc = "lame-crc-32khz-stereo";
   const string ItsTwinWithoutACrc = "lame-32khz-stereo";

   public static TheoryData<string> TheFixtures => [..Mp3Fixtures.Names()];

   public static TheoryData<string> TheFixturesWithoutACrc => [..Mp3Fixtures.Names().Where(it => it != TheFixtureWithACrc)];

   static double ReplayGainMeasured(string fixture) => Math.Round((new Mp3File(Mp3Fixtures.Original(fixture)).MeasureReplayGainOrNone()?.Decibels ?? double.NaN), 2);

   [Theory, MemberData(nameof(TheFixturesWithoutACrc))]
   public void the_replaygain_measured_is_the_one_mp3gain_measured(string fixture) => ReplayGainMeasured(fixture).Must().Be(Mp3Fixtures.ReplayGainMp3GainMeasured(fixture));

   [XF] public void a_file_whose_frames_have_a_crc_is_measured_as_the_same_audio_without_one() =>
      ReplayGainMeasured(TheFixtureWithACrc).Must().Be(ReplayGainMeasured(ItsTwinWithoutACrc));

   [Theory, MemberData(nameof(TheFixtures))]
   public void the_normalized_file_is_byte_for_byte_the_one_mp3gain_made(string fixture) =>
      new Mp3File(Mp3Fixtures.Original(fixture)).NormalizedToTheReplayGainReference().Bytes().Must().SequenceEqual(Mp3Fixtures.NormalizedByMp3Gain(fixture));

   [XF] public void changing_the_gain_by_no_steps_changes_nothing() =>
      new Mp3File(Mp3Fixtures.Original("lame-crc-32khz-stereo")).WithGainChangedBy(0).Bytes().Must().SequenceEqual(Mp3Fixtures.Original("lame-crc-32khz-stereo"));

   [XF] public void a_file_holding_no_mp3_frames_is_refused() =>
      Invoking(() => new Mp3File([1, 2, 3, 4, 5, 6]).MeasureReplayGainOrNone()).Must().Throw<InvalidDataException>();
}
