using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Storage.Media;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

public class MediaLayerFolders_IsInsideFreeOrACommercialRepository : IAIGeneratedSpec
{
   public class is_true_for : MediaLayerFolders_IsInsideFreeOrACommercialRepository
   {
      [XF] public void free() => MediaLayerFolders.IsInsideFreeOrACommercialRepository("free").Must().BeTrue();
      [XF] public void a_folder_inside_free() => MediaLayerFolders.IsInsideFreeOrACommercialRepository("free/TTS/vocab").Must().BeTrue();
      [XF] public void a_folder_inside_a_commercial_repository() => MediaLayerFolders.IsInsideFreeOrACommercialRepository("commercial-01/anime/immersion-kit/audio").Must().BeTrue();
      [XF] public void a_folder_written_with_backslashes_and_a_trailing_slash() => MediaLayerFolders.IsInsideFreeOrACommercialRepository(@"commercial-01\anime\").Must().BeTrue();
   }

   public class is_false_for : MediaLayerFolders_IsInsideFreeOrACommercialRepository
   {
      [XF] public void a_folder_outside_them() => MediaLayerFolders.IsInsideFreeOrACommercialRepository("vocab/TTS").Must().BeFalse();
      [XF] public void no_folder() => MediaLayerFolders.IsInsideFreeOrACommercialRepository("").Must().BeFalse();
      [XF] public void a_commercial_folder_without_a_number() => MediaLayerFolders.IsInsideFreeOrACommercialRepository("commercial/anime").Must().BeFalse();
      [XF] public void a_folder_that_climbs_out_of_one() => MediaLayerFolders.IsInsideFreeOrACommercialRepository("free/../vocab").Must().BeFalse();
      [XF] public void an_absolute_path() => MediaLayerFolders.IsInsideFreeOrACommercialRepository(@"C:\media\free").Must().BeFalse();
   }
}
