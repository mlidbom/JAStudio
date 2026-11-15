using System;

namespace JAStudio.Anki.AnkisFunctions.ThroughPythonnet;

public sealed partial class AnkiThroughPythonnet
{
   sealed class ItsWindows(AnkiThroughPythonnet anki) : IAnkisWindows
   {
      readonly AnkiThroughPythonnet _anki = anki;

      public void OpenTheBrowserOn(string search) => _anki.Backend.Use(it => it.browser_execute_lookup(search));

      public void OpenTheBrowserAndThePreviewerOn(string search) => _anki.Backend.Use(it => it.browser_execute_lookup_and_show_previewer(search));

      public void ShowATooltip(string message, TimeSpan shownFor) => _anki.Backend.Use(it => it.ui_show_tooltip(message, (int)shownFor.TotalMilliseconds));

      public void Refresh() => _anki.Backend.Use(it => it.ui_refresh());
   }
}
