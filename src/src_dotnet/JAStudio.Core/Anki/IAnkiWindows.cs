namespace JAStudio.Core.Anki;

/// <summary>
/// Anki's own windows, such as its reviewer, browser and previewer, while JAStudio runs inside Anki: JAStudio refreshes them after it changes notes,<br/>
/// shows its passing messages in them, and opens notes in them. JAStudio running without Anki has none (<see cref="NoAnkiWindows"/>), and offers<br/>
/// the commands that open notes in them only while there's an Anki (<see cref="AnkiAvailability"/>).
/// </summary>
public interface IAnkiWindows
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
