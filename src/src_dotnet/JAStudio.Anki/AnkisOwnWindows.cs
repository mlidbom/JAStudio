using JAStudio.Core.Anki;

namespace JAStudio.Anki;

/// <summary>The Anki windows of JAStudio running inside Anki: Anki's own, which JAStudio drives through Python (<see cref="AnkiFacade"/>).</summary>
public sealed class AnkisOwnWindows : IAnkiWindows
{
   public void Refresh() => AnkiFacade.UIUtils.Refresh();

   /// <summary>Shows <paramref name="message"/> in Anki's tooltip.</summary>
   public void ShowAPassingMessage(string message) => AnkiFacade.UIUtils.ShowTooltip(message);

   public void OpenTheBrowserOn(string ankiSearch) => AnkiFacade.Browser.ExecuteLookup(ankiSearch);

   public void OpenThePreviewerOn(string ankiSearch) => AnkiFacade.Browser.ExecuteLookupAndShowPreviewer(ankiSearch);
}
