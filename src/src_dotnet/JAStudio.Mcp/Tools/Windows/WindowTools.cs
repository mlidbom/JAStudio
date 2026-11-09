using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JAStudio.Core;
using JAStudio.Mcp.UI;
using JAStudio.UI.Menus;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Windows;

/// <summary>
/// The tools that show Claude JAStudio's windows: they open them in the headless JAStudio's <see cref="HeadlessUI"/>, on the real corpus, and<br/>
/// render them. Only a JAStudio with a headless UI offers them: in the live JAStudio, a window opened would open on Magnus's screen.
/// </summary>
[McpServerToolType]
public class WindowTools
{
   /// <summary>
   /// The windows <see cref="RenderWindow"/> renders. Not <see cref="MainMenuWindows.TextToSpeechVoices"/>: opening it lists the voices of the<br/>
   /// text-to-speech services online, with Magnus's keys, and opens an audio player.
   /// </summary>
   static readonly MainMenuWindow[] WindowsRendered = [MainMenuWindows.Options, MainMenuWindows.ReadingsMappings, MainMenuWindows.ImportProfiles];

   readonly HeadlessUI _headlessUI;
   readonly TemporaryServiceCollection _services;

   public WindowTools(HeadlessUI headlessUI, TemporaryServiceCollection services)
   {
      _headlessUI = headlessUI;
      _services = services;
   }

   [McpServerTool(Name = "render_window", ReadOnly = true, Idempotent = true)]
   [Description("""
                Renders one of the windows of their own that the workspace's main menu opens: opens it in the headless JAStudio as the menu
                does, with the corpus and the add-on's configuration, draws it as on a screen, closes it, and gives its image. The windows:
                "Options", "Readings mappings" and "Import profiles".
                """)]
   public IEnumerable<ContentBlock> RenderWindow([Description("The window's name, as the main menu names it, such as \"Options\".")] string window)
   {
      var windowToRender = WindowsRendered.SingleOrDefault(it => string.Equals(it.Name, window, StringComparison.OrdinalIgnoreCase))
                        ?? throw new ArgumentException($"There is no window named '{window}' to render. The windows are: {string.Join(", ", WindowsRendered.Select(it => $"\"{it.Name}\""))}.", nameof(window));

      var image = _headlessUI.Render(() => windowToRender.Open(_services));
      return [new TextContentBlock { Text = $"{windowToRender.Name}, {image.Width}×{image.Height} pixels" }, ImageContentBlock.FromBytes(image.Png, "image/png")];
   }
}
