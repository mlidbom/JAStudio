using System;
using System.Collections.Generic;

namespace JAStudio.Core.Studying;

/// <summary>
/// The own windows of the SRS, the spaced repetition system, such as Anki's reviewer, browser and previewer, while JAStudio runs inside Anki:<br/>
/// JAStudio has them draw their cards again once its collection has loaded, shows its passing messages in them, and opens notes in them. JAStudio running without a<br/>
/// spaced repetition system has none (<see cref="NoSRSWindows"/>), and offers the commands that open notes in them only while there is one<br/>
/// (<see cref="SRSAvailability"/>).
/// </summary>
/// <remarks>
/// review-naming:studying: ISRSWindows was IAnkiWindows (Claude, 2026-11-15). The same day its browser and previewer went from opening on a<br/>
/// search in Anki's search syntax to opening on study-notes by id, which left the syntax to the Anki integration.
/// </remarks>
public interface ISRSWindows
{
   /// <summary>
   /// Has them draw what they show again. JAStudio asks for it once its collection has loaded: a card drawn while it was loading says that<br/>
   /// JAStudio is still loading, and only the spaced repetition system can replace that with the card.
   /// </summary>
   /// <remarks>
   /// It isn't for showing a change to a note: a card's page is JAStudio's own, and reloads itself when its note changes<br/>
   /// (<see cref="JAStudio.Core.UI.Web.ChangesThatOutdateNotePages"/>). What JAStudio changes in the spaced repetition system itself, the system's<br/>
   /// windows follow as they follow any of its own operations.
   /// </remarks>
   void Refresh();

   /// <summary>Shows <paramref name="message"/> for a few seconds, then hides it.</summary>
   void ShowAPassingMessage(string message) => ShowAPassingMessage(message, shownFor: TimeSpan.FromSeconds(3));

   /// <summary>Shows <paramref name="message"/> for <paramref name="shownFor"/>, then hides it: for a message that takes longer to read than a few seconds.</summary>
   void ShowAPassingMessage(string message, TimeSpan shownFor);

   /// <summary>
   /// Opens the spaced repetition system's browser, in Anki its browser, on the study-notes with the ids <paramref name="studyNoteIds"/>, of which<br/>
   /// there is at least one, and on no others.
   /// </summary>
   void OpenTheBrowserOn(IReadOnlyCollection<long> studyNoteIds);

   /// <summary>Opens the spaced repetition system's previewer on the study-note with the id <paramref name="studyNoteId"/>: in Anki, its browser on it, and the previewer.</summary>
   void OpenThePreviewerOn(long studyNoteId);
}
