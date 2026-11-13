using System;

namespace JAStudio.Core.Anki;

/// <summary>
/// The Anki windows of a JAStudio running without Anki, such as the headless JAStudio and JAStudio started on its own: there are none. Nothing<br/>
/// shows notes that would need refreshing, and the commands that would open a note in Anki aren't offered.
/// </summary>
public sealed class NoAnkiWindows : IAnkiWindows
{
   public void Refresh() {}

   /// <summary>Shows nothing: there is no Anki window to show it in.</summary>
   public void ShowAPassingMessage(string message) {}

   public void OpenTheBrowserOn(string ankiSearch) => throw NotOffered();
   public void OpenThePreviewerOn(string ankiSearch) => throw NotOffered();

   static InvalidOperationException NotOffered() => new($"JAStudio runs without Anki, so the commands that open Anki's windows aren't offered ({nameof(AnkiAvailability)}).");
}
