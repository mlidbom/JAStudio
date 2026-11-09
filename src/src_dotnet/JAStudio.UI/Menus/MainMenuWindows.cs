using JAStudio.UI.Views;
using JAStudio.UI.Views.Importing;
using JAStudio.UI.Views.TextToSpeech;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Menus;

/// <summary>
/// The windows of their own that the workspace's main menu (<see cref="WorkspaceMainMenu"/>) opens, each by the name the menu gives it, opened<br/>
/// as the menu opens it. JAStudio's MCP server opens them the same way, to render them.
/// </summary>
public static class MainMenuWindows
{
   public static readonly MainMenuWindow Options = new("Options", services => new OptionsDialog(services).ShowWhereItWasLast(services));
   public static readonly MainMenuWindow ReadingsMappings = new("Readings mappings", services => new ReadingsMappingsDialog(services).ShowWhereItWasLast(services));
   public static readonly MainMenuWindow TextToSpeechVoices = new("Text-to-speech voices", ChosenVoicesDialog.Open);
   public static readonly MainMenuWindow ImportProfiles = new("Import profiles", ImportProfilesWindow.Open);
}
