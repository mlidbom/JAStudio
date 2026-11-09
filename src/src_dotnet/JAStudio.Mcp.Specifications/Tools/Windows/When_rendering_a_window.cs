using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.UI;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Windows;

/// <summary>The server of a JAStudio with a headless UI, as the headless JAStudio's is, rendering the windows the workspace's main menu opens.</summary>
public class When_rendering_a_window : McpServerSpecification
{
   /// <summary>The bytes every PNG file starts with.</summary>
   static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

   protected When_rendering_a_window() : base(HeadlessUI.OfThisProcess()) {}

   protected CallToolResult RenderWindow(string window) => CallTool("render_window", new Dictionary<string, object?> { ["window"] = window });

   public class named_options : When_rendering_a_window
   {
      readonly CallToolResult _result;

      public named_options() => _result = RenderWindow("options");

      ImageContentBlock TheImage => _result.Content.OfType<ImageContentBlock>().Single();

      /// <summary>A PNG's width, in its header chunk, which follows the signature, the chunk's length and its type.</summary>
      int WidthOfTheImage => BinaryPrimitives.ReadInt32BigEndian(TheImage.DecodedData.Span[16..20]);

      [XF] public void the_tool_call_succeeds() => _result.IsError.Must().NotBe(true);
      [XF] public void the_image_is_a_png() => TheImage.MimeType.Must().Be("image/png");
      [XF] public void the_images_data_is_a_png_file() => TheImage.DecodedData.Span[..8].ToArray().Must().SequenceEqual(PngSignature);
      [XF] public void the_image_is_wider_than_a_hundred_pixels() => WidthOfTheImage.Must().BeGreaterThan(100);
      [XF] public void the_text_names_the_window_as_the_main_menu_does() => _result.Content.OfType<TextContentBlock>().Single().Text.Must().StartWith("Options, ");
   }

   public class named_import_profiles : When_rendering_a_window
   {
      readonly CallToolResult _result;

      public named_import_profiles() => _result = RenderWindow("Import profiles");

      [XF] public void the_tool_call_succeeds() => _result.IsError.Must().NotBe(true);
      [XF] public void it_gives_an_image() => _result.Content.OfType<ImageContentBlock>().Count().Must().Be(1);
   }

   public class named_as_no_window_is : When_rendering_a_window
   {
      readonly CallToolResult _result;

      public named_as_no_window_is() => _result = RenderWindow("Text-to-speech voices");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_windows_there_are() => TextOf(_result).Must().Contain("The windows are: \"Options\", \"Readings mappings\", \"Import profiles\"");
   }
}
