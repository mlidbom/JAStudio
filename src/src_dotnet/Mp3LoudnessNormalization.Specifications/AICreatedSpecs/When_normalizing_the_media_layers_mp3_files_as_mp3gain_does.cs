using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Mp3LoudnessNormalization.Specifications.AICreatedSpecs;

/// <summary>
/// Up to 300 MP3 files from each folder of each repository of JAStudio's media layer, normalized by <see cref="Mp3File"/> and by mp3gain,<br/>
/// compared byte for byte (<see cref="Mp3GainComparison"/>).<br/>
/// Explicit, so run on purpose only: it needs mp3gain.exe, named by the <c>MP3GAIN</c> environment variable, and the media layer, and takes minutes. Run it with<br/>
/// <c>dotnet test src/src_dotnet/Mp3LoudnessNormalization.Specifications --filter FullyQualifiedName~media_layers -- xUnit.Explicit=only</c>.
/// </summary>
/// <remarks>
/// The known exception is files starting with a frame whose audio data is missing (<see cref="Mp3StartInspection"/>): mp3gain's decoder and miniaudio<br/>
/// treat that broken start differently, so about 8% of them get a gain one step apart. Every other file must come out the same.
/// </remarks>
public class When_normalizing_the_media_layers_mp3_files_as_mp3gain_does : IAIGeneratedSpec
{
   [XF(Explicit = true)]
   public void every_file_comes_out_byte_for_byte_as_mp3gain_makes_it_but_files_starting_with_a_frame_whose_audio_data_is_missing()
   {
      var comparison = Mp3GainComparison.OfTheMediaLayer(filesPerFolder: 300);
      var excepted = comparison.FilesNormalizedDifferently.Count(it => it.StartsWithAFrameWhoseAudioDataIsMissing);
      TestContext.Current.TestOutputHelper?.WriteLine($"{comparison.Files.Count} files compared, {excepted} of the "
                                                    + $"{comparison.Files.Count(it => it.StartsWithAFrameWhoseAudioDataIsMissing)} starting with missing audio data came out differently.");
      comparison.FilesNormalizedDifferently.Where(it => !it.StartsWithAFrameWhoseAudioDataIsMissing).Must().BeEmpty();
   }
}
