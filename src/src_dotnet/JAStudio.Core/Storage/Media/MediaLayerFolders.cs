using System.Text.RegularExpressions;

namespace JAStudio.Core.Storage.Media;

/// <summary>
/// The folders of the media layer that files can be stored in, by copyright regime: <c>free/</c>, in the corpus repository itself,<br/>
/// and the commercial repositories <c>commercial-01/</c>, <c>commercial-02/</c>, …, submodules of the corpus repository, so that a takedown<br/>
/// of one commercial source means deleting files there. See <c>dev_docs/media-storage-and-import.md</c>.
/// </summary>
public static partial class MediaLayerFolders
{
   /// <summary>
   /// Whether <paramref name="folder"/>, relative to the media layer's root, is <c>free/</c> or a commercial repository, or a folder inside one.<br/>
   /// Folders outside them, such as the stray <c>vocab/TTS</c>, say nothing about the copyright of the files in them.
   /// </summary>
   public static bool IsInsideFreeOrACommercialRepository(string folder) => FreeOrACommercialRepository().IsMatch(folder.Replace('\\', '/').TrimEnd('/'));

   [GeneratedRegex(@"^(free|commercial-\d+)(/(?!\.\.?/|\.\.?$)[^/]+)*$")]
   private static partial Regex FreeOrACommercialRepository();
}
