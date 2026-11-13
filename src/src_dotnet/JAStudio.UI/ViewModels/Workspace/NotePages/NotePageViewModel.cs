using System.Collections.Generic;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

namespace JAStudio.UI.ViewModels.Workspace.NotePages;

/// <summary>
/// A note's page in a workspace tab, built in Avalonia: a sentence's, for now (<see cref="SentencePageViewModel"/>). The other note types' pages are<br/>
/// still the cards' pages, which the tab shows in its web view. See <c>dev_docs/jastudio-workspace.md</c>, "Technology".
/// </summary>
/// <remarks>
/// A page shows its note as it was when the page was made: the tab makes it anew when the note or a setting changes, as the web view reloads a card's<br/>
/// page.
/// </remarks>
abstract class NotePageViewModel
{
   readonly INotePageHost _host;

   protected NotePageViewModel(INotePageHost host) => _host = host;

   /// <summary>Follows <paramref name="link"/>, showing its note in the tab, as clicking it does.</summary>
   public void Follow(NoteLinkViewModel link) => _host.ShowNote(link.LinkedNote);

   /// <summary>Opens the note of <paramref name="link"/> in a new tab in the background, as middle-clicking it does.</summary>
   public void OpenInABackgroundTab(NoteLinkViewModel link) => _host.OpenNoteInBackgroundTab(link.LinkedNote);

   /// <summary>
   /// The right-click menu for the page's note, for a right-click on no element of the page (<see cref="INotePageElement"/>), such as on its question:<br/>
   /// the same one Anki shows for it, for the <paramref name="selection"/> right-clicked, the text selected there or else the whole text, and the<br/>
   /// <paramref name="clipboard"/>'s text.
   /// </summary>
   public List<SpecMenuItem> NoteContextMenuSpec(string selection, string clipboard) => _host.NoteContextMenuSpec(selection, clipboard);

   /// <summary>
   /// The right-click menu of <paramref name="element"/>, an element of the page, such as a sentence's breakdown entry: its own items, then its note's<br/>
   /// menu and the page's note's, and what can be done with <paramref name="selection"/>, the text selected on it, if any.
   /// </summary>
   public abstract List<SpecMenuItem> MenuOf(INotePageElement element, string selection);

   /// <summary>Runs the note view shortcut bound to <paramref name="keyGesture"/>, if any, as the key was pressed on the page.</summary>
   public void KeyPressed(string keyGesture) => _host.KeyPressed(keyGesture);

   /// <summary>Plays the note's own audio, if it has any, as the page is shown after the note was opened or a link to it followed, as Anki plays a card's.</summary>
   public abstract void PlayTheNotesAudio();
}
