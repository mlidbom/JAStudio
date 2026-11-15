namespace JAStudio.Core.Studying;

/// <summary>
/// The own windows of the SRS, the spaced repetition system, such as Anki's reviewer, browser and previewer, while JAStudio runs inside Anki:<br/>
/// JAStudio refreshes them after it changes notes, shows its passing messages in them, and opens notes in them. JAStudio running without a<br/>
/// spaced repetition system has none (<see cref="NoSRSWindows"/>), and offers the commands that open notes in them only while there is one<br/>
/// (<see cref="SRSAvailability"/>).
/// </summary>
/// <remarks>review-naming:studying: ISRSWindows was IAnkiWindows (Claude, 2026-11-15).</remarks>
public interface ISRSWindows
{
   /// <summary>Refreshes what they show, so that the notes JAStudio changed show as they are now.</summary>
   void Refresh();

   /// <summary>Shows <paramref name="message"/> for a few seconds, then hides it.</summary>
   void ShowAPassingMessage(string message);

   /// <summary>Opens Anki's browser on the notes that <paramref name="ankiSearch"/>, written in Anki's search syntax, finds.</summary>
   void OpenTheBrowserOn(string ankiSearch);

   /// <summary>Opens Anki's browser on the notes that <paramref name="ankiSearch"/>, written in Anki's search syntax, finds, and its previewer on the first.</summary>
   void OpenThePreviewerOn(string ankiSearch);
}
