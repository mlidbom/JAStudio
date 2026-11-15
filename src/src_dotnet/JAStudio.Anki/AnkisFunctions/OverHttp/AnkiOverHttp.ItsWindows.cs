using System;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsWindows(AnkiOverHttp anki) : IAnkisWindows
   {
      readonly AnkiOverHttp _anki = anki;

      public void OpenTheBrowserOn(string search) => _anki.Call("browser_execute_lookup", ("query", search));

      public void OpenTheBrowserAndThePreviewerOn(string search) => _anki.Call("browser_execute_lookup_and_show_previewer", ("query", search));

      public void ShowATooltip(string message, TimeSpan shownFor) => _anki.Call("ui_show_tooltip", ("message", message), ("period_ms", (int)shownFor.TotalMilliseconds));

      public void Refresh() => _anki.Call("ui_refresh");
   }
}
