using System;

namespace JAStudio.Core.Studying;

/// <summary>
/// The windows of the SRS, the spaced repetition system, of a JAStudio running without one, such as the headless JAStudio and JAStudio started<br/>
/// on its own: there are none. Nothing shows notes that would need refreshing, and the commands that would open a note in them aren't offered.
/// </summary>
/// <remarks>review-naming:studying: NoSRSWindows was NoAnkiWindows (Claude, 2026-11-15).</remarks>
public sealed class NoSRSWindows : ISRSWindows
{
   public void Refresh() {}

   /// <summary>Shows nothing: there is no window to show it in.</summary>
   public void ShowAPassingMessage(string message) {}

   public void OpenTheBrowserOn(string ankiSearch) => throw NotOffered();
   public void OpenThePreviewerOn(string ankiSearch) => throw NotOffered();

   static InvalidOperationException NotOffered() =>
      new($"JAStudio runs without a spaced repetition system, so the commands that open its windows aren't offered ({nameof(SRSAvailability)}).");
}
