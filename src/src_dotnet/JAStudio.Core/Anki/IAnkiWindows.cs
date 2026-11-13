namespace JAStudio.Core.Anki;

/// <summary>
/// Anki's own windows, such as its reviewer, browser and previewer, while JAStudio runs inside Anki: JAStudio refreshes them after it changes notes,<br/>
/// shows its passing messages in them, and opens notes in them. JAStudio running without Anki has none (<see cref="NoAnkiWindows"/>).
/// </summary>
public interface IAnkiWindows
{
   /// <summary>Whether there are Anki windows, as there are while JAStudio runs inside Anki. The commands that open notes in them are offered only then.</summary>
   bool Exist { get; }

   /// <summary>Refreshes what they show, so that the notes JAStudio changed show as they are now.</summary>
   void Refresh();

   /// <summary>Shows <paramref name="message"/> for a few seconds, then hides it.</summary>
   void ShowAPassingMessage(string message);

   /// <summary>Opens Anki's browser on the notes that <paramref name="ankiSearch"/>, written in Anki's search syntax, finds.</summary>
   void OpenTheBrowserOn(string ankiSearch);

   /// <summary>Opens Anki's browser on the notes that <paramref name="ankiSearch"/>, written in Anki's search syntax, finds, and its previewer on the first.</summary>
   void OpenThePreviewerOn(string ankiSearch);
}
