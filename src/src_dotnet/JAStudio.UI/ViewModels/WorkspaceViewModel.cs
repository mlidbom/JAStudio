using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.UI.Web;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Shortcuts;

namespace JAStudio.UI.ViewModels;

/// <summary>
/// What a workspace shows: the note on its page, the window title, and whether its history can go back or forward.<br/>
/// See <c>dev_docs/jastudio-workspace.md</c>.
/// </summary>
/// <remarks>
/// The view, <see cref="Views.WorkspaceWindow"/>, owns the web view that shows the note's page.<br/>
/// It reports here what the page shows and which keys are pressed on it, and reloads the page when <see cref="PageOutdated"/> says so.
/// </remarks>
partial class WorkspaceViewModel : ObservableObject
{
   const string AppTitle = "JAStudio";

   readonly TemporaryServiceCollection _services;
   readonly NoteViewShortcuts _noteViewShortcuts;

   /// <summary>The note on the page, as last reported by the page. Null for a page that shows no note.</summary>
   NoteId? _shownNoteId;

   [ObservableProperty] string _title = AppTitle;
   [ObservableProperty] bool _canGoBack;
   [ObservableProperty] bool _canGoForward;

   public WorkspaceViewModel(TemporaryServiceCollection services)
   {
      _services = services;
      _noteViewShortcuts = new NoteViewShortcuts(services.CoreApp.Config);
      services.CoreApp.Collection.OnNoteUpdated(note =>
      {
         if(IsShowing(note))
            PageOutdated?.Invoke();
      });
      services.CoreApp.Config.OnChange(() => PageOutdated?.Invoke());
   }

   /// <summary>
   /// Raised when the page no longer shows the current state, so it must be reloaded:<br/>
   /// the note on it was updated, or a setting changed, such as how sentence breakdowns are shown.<br/>
   /// It's raised on the thread that made the change.
   /// </summary>
   public event Action? PageOutdated;

   /// <summary>The web view now shows the page at <paramref name="pageUrl"/>.</summary>
   public void PageShown(Uri pageUrl)
   {
      _shownNoteId = CardServerUrl.NoteIdOfPage(pageUrl);
      Title = ShownNote() is {} note ? $"{note.GetQuestion()} - {AppTitle}" : AppTitle;
   }

   /// <summary>Runs the note view shortcut bound to <paramref name="keyGesture"/>, if any, on the note on the page.</summary>
   public void KeyPressed(string keyGesture) => _noteViewShortcuts.BoundTo(keyGesture)?.Run(ShownNote());

   /// <summary>The right-click menu for the note on the page: the same one Anki shows for it.</summary>
   public List<SpecMenuItem> NoteContextMenuSpec(string selection, string clipboard) =>
      new NoteContextMenu(_services).BuildContextMenuSpec(ShownNote(), selection, clipboard);

   bool IsShowing(JPNote note) => note.GetId().Equals(_shownNoteId);

   JPNote? ShownNote() => _shownNoteId is {} noteId ? _services.CoreApp.Collection.NoteFromNoteId(noteId) : null;
}
