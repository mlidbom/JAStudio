using System;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsWindows(AnkisHttpServer server) : IAnkisWindows
   {
      readonly AnkisHttpServer _server = server;

      public void OpenTheBrowserOn(string search) => _server.Call("browser_execute_lookup", ("query", search));

      public void OpenTheBrowserAndThePreviewerOn(string search) => _server.Call("browser_execute_lookup_and_show_previewer", ("query", search));

      public void ShowATooltip(string message, TimeSpan shownFor) => _server.Call("ui_show_tooltip", ("message", message), ("period_ms", (int)shownFor.TotalMilliseconds));

      public void Refresh() => _server.Call("ui_refresh");
   }
}
