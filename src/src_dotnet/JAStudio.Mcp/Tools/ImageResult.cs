using JAStudio.Core.UI;
using ModelContextProtocol.Protocol;

namespace JAStudio.Mcp.Tools;

/// <summary>What a tool that renders something answers with: a line saying what the image shows and how large it is, and the image, which Claude sees.</summary>
static class ImageResult
{
   public static ContentBlock[] Of(string whatItShows, PngImage image) =>
      [new TextContentBlock { Text = $"{whatItShows}, {image.Width}×{image.Height} pixels" }, ImageContentBlock.FromBytes(image.Png, "image/png")];
}
