using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;

namespace JAStudio.UI.ViewModels.Workspace.NotePages;

/// <summary>What a note's page in Avalonia (<see cref="NotePageViewModel"/>) needs from the workspace tab it's shown in (<see cref="NoteTabViewModel"/>).</summary>
interface INotePageHost
{
   /// <summary>Shows <paramref name="note"/> in the tab, after the note shown, as following a link on the page does.</summary>
   void ShowNote(NoteId note);

   /// <summary>Opens <paramref name="note"/> in a new tab, without showing it, as middle-clicking a link on the page does.</summary>
   void OpenNoteInBackgroundTab(NoteId note);

   /// <summary>The right-click menu for the page's note, for the <paramref name="selection"/> right-clicked: the same one Anki shows for it.</summary>
   List<SpecMenuItem> NoteContextMenuSpec(string selection, string clipboard);

   /// <summary>Runs the note view shortcut bound to <paramref name="keyGesture"/>, if any, on the page's note.</summary>
   void KeyPressed(string keyGesture);
}
