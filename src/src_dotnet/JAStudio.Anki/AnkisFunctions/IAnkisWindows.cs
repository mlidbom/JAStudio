using System;

namespace JAStudio.Anki.AnkisFunctions;

/// <summary>
/// Anki's own windows, its reviewer, browser and previewer: one group of the functions JAStudio calls in Anki (see <see cref="IAnkisNotes"/> for<br/>
/// the groups). Each call returns at once: Anki does the work on its own UI thread afterwards.
/// </summary>
public interface IAnkisWindows
{
   /// <summary>
   /// Opens Anki's browser, if it isn't open, and runs <paramref name="search"/> in it, a search in Anki's search syntax. The add-on's<br/>
   /// <c>browser_execute_lookup</c>.
   /// </summary>
   void OpenTheBrowserOn(string search);

   /// <summary>As <see cref="OpenTheBrowserOn"/>, and opens Anki's previewer on what the search finds. The add-on's <c>browser_execute_lookup_and_show_previewer</c>.</summary>
   void OpenTheBrowserAndThePreviewerOn(string search);

   /// <summary>Shows <paramref name="message"/> in Anki's tooltip for <paramref name="shownFor"/>. The add-on's <c>ui_show_tooltip</c>.</summary>
   void ShowATooltip(string message, TimeSpan shownFor);

   /// <summary>Has Anki's reviewer, previewer and browser draw what they show again, so that it shows the notes as they are now. The add-on's <c>ui_refresh</c>.</summary>
   void Refresh();
}
