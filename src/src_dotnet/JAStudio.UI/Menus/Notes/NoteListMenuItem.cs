using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Menus.Notes;

/// <summary>Menu items that list notes in the workspace's navigation pane, as the note menus' "Open" items do.</summary>
static class NoteListMenuItem
{
   /// <summary>
   /// A menu item that, when clicked, lists the notes <paramref name="notes"/> finds then in the navigation pane's "List" tab,<br/>
   /// under <paramref name="listTitle"/>, such as "Sentences with 走る".
   /// </summary>
   public static SpecMenuItem Showing(string menuItemTitle, string listTitle, Func<IEnumerable<JPNote>> notes) =>
      SpecMenuItem.Command(menuItemTitle, () => WorkspaceWindow.ShowNoteList(new NoteList(listTitle, [..notes()])));
}
