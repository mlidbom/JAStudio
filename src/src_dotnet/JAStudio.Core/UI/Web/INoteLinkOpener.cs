using JAStudio.Core.Note;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// Opens the note behind a clicked note link on a card page, either in JAStudio's workspace or in Anki's previewer.
/// </summary>
/// <remarks>
/// A card page shown inside Anki's reviewer can't open windows itself, so its link clicks reach the card server (<c>JAStudio.Web.CardServer</c>) as<br/>
/// requests, and the card server hands them to this.
/// </remarks>
public interface INoteLinkOpener
{
   /// <summary>Shows the note in JAStudio's workspace window and brings that window to the front.</summary>
   void OpenInWorkspace(NoteId noteId);

   /// <summary>Shows the note in Anki's previewer, if the note has an Anki note.</summary>
   void OpenInAnkiPreviewer(NoteId noteId);
}
