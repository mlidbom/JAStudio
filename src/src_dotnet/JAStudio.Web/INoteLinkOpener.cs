using JAStudio.Core.Note;

namespace JAStudio.Web;

/// <summary>
/// Opens the note behind a clicked note link, either in JAStudio's workspace or in Anki's previewer.
/// </summary>
/// <remarks>
/// A card page shown inside Anki's reviewer can't open windows itself, so its link clicks reach the
/// <see cref="CardServer"/> as requests, and the <see cref="CardServer"/> hands them to this.<br/>
/// It is implemented by the application hosting the <see cref="CardServer"/>, which owns both windows.
/// </remarks>
public interface INoteLinkOpener
{
   /// <summary>Shows the note in JAStudio's workspace window and brings that window to the front.</summary>
   void OpenInWorkspace(NoteId noteId);

   /// <summary>Shows the note in Anki's previewer, if the note has an Anki note.</summary>
   void OpenInAnkiPreviewer(NoteId noteId);
}
