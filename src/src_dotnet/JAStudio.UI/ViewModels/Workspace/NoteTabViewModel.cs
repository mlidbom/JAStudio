using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.UI.Web;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Shortcuts;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.Tagging;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// A tab in a workspace (<see cref="WorkspaceViewModel"/>). It shows a note's page, and has its own back and forward history,<br/>
/// as a browser tab does. Following a link on the page shows the linked note in the same tab.
/// </summary>
/// <remarks>
/// The view, <see cref="NoteTabView"/>, owns the web view that shows the page.<br/>
/// It reports here what the page shows and which keys are pressed on it, and reloads the page when <see cref="PageOutdated"/> says so.
/// </remarks>
/// <remarks>
/// A tab's page loads the first time the tab is shown, so a tab opened in the background has no page until then.<br/>
/// A hidden tab isn't reloaded in the background. If its page became outdated while it was hidden,<br/>
/// <see cref="PageOutdated"/> is raised when the tab is shown again.
/// </remarks>
partial class NoteTabViewModel : ObservableObject, IDisposable
{
   const string NoNoteHeader = "No note";

   readonly WorkspaceViewModel _workspace;
   readonly TemporaryServiceCollection _services;
   readonly NoteViewShortcuts _noteViewShortcuts;
   readonly List<IDisposable> _subscriptions;

   /// <summary>The note on the page, as last reported by the page. Null for a page that shows no note.</summary>
   NoteId? _shownNoteId;

   /// <summary>Whether the tab has been shown, and so has loaded its page. Until then there is no page to become outdated.</summary>
   bool _hasBeenShown;

   bool _pageOutdatedWhileHidden;

   /// <summary>The question of the note on the page, shown on the tab and in the window title.</summary>
   [ObservableProperty] string _header = NoNoteHeader;

   [ObservableProperty] bool _canGoBack;
   [ObservableProperty] bool _canGoForward;

   /// <summary>
   /// What the workspace's editor section shows for this tab: the editors of the note it shows.<br/>
   /// Each tab has its own, for the note it shows, and the section shows the shown tab's.<br/>
   /// Null when the tab shows no note.
   /// </summary>
   [ObservableProperty] INoteEditorSectionViewModel? _editorSection;

   public NoteTabViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services, Uri firstPageUrl)
   {
      _workspace = workspace;
      _services = services;
      _noteViewShortcuts = new NoteViewShortcuts(services.CoreApp.Config);
      FirstPageUrl = firstPageUrl;
      PageShown(firstPageUrl);

      var uiThread = services.Resolve<IUIThreadDispatcher>();
      _subscriptions =
      [
         services.CoreApp.Collection.NoteUpdated.Subscribe(note =>
         {
            if(!IsShowing(note)) return;

            PageBecameOutdated();
            uiThread.PostToUIThread(() => EditorSection?.Refresh());
         }),
         services.CoreApp.Collection.ReloadedFromBackend.Subscribe(_ =>
         {
            PageBecameOutdated();
            uiThread.PostToUIThread(CreateTheEditorSectionForTheShownNote);
         }),
         services.CoreApp.Config.Changed.Subscribe(_ => PageBecameOutdated())
      ];
   }

   /// <summary>The page the tab opened on, loaded the first time the tab is shown. Links followed on the page take the tab on from there.</summary>
   public Uri FirstPageUrl { get; }

   /// <summary>Whether this is the workspace's <see cref="WorkspaceViewModel.SelectedTab"/>: the one being shown.</summary>
   public bool IsSelected => _workspace.SelectedTab == this;

   /// <summary>
   /// Raised when the page no longer shows the current state, so it must be reloaded:<br/>
   /// the note on it was updated, the collection was reloaded after an Anki sync,<br/>
   /// or a setting changed, such as how sentence breakdowns are shown.<br/>
   /// It's raised on the thread that made the change, or, for a tab that was hidden, when the tab is shown.
   /// </summary>
   public event Action? PageOutdated;

   /// <summary>
   /// The tab now shows the page at <paramref name="pageUrl"/>.<br/>
   /// The page reports this after every navigation. For <see cref="FirstPageUrl"/> the tab assumes it from the start,<br/>
   /// so that its header is right while the page loads.
   /// </summary>
   public void PageShown(Uri pageUrl)
   {
      var shownNoteId = CardServerUrl.NoteIdOfPage(pageUrl);
      var showsAnotherNote = !Equals(shownNoteId, _shownNoteId);
      _shownNoteId = shownNoteId;
      Header = ShownNote()?.GetQuestion() ?? NoNoteHeader;
      _workspace.TabHeaderChanged(this);
      if(showsAnotherNote)
         CreateTheEditorSectionForTheShownNote();
   }

   /// <summary>Runs the note view shortcut bound to <paramref name="keyGesture"/>, if any, on the note on the page.</summary>
   public void KeyPressed(string keyGesture) => _noteViewShortcuts.BoundTo(keyGesture)?.Run(ShownNote());

   /// <summary>The right-click menu for the note on the page: the same one Anki shows for it.</summary>
   public List<SpecMenuItem> NoteContextMenuSpec(string selection, string clipboard) =>
      new NoteContextMenu(_services).BuildContextMenuSpec(ShownNote(), selection, clipboard);

   /// <summary>Ctrl+T: picks a tag through <paramref name="pickATagInADialog"/> and adds it to the note on the page. Does nothing when the page shows no note.</summary>
   /// <param name="pickATagInADialog">Shows a tag picker dialog, and returns the tag path picked in it, or null when it was cancelled.</param>
   internal void AddATagToTheShownNote(Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog)
   {
      if(ShownNote() is null) return;

      new OneNoteTagging(_services).AddATagTo(TheShownNote, pickATagInADialog);
   }

   /// <summary>Ctrl+Shift+T: picks one of the tags of the note on the page through <paramref name="pickATagInADialog"/>, and removes it. Does nothing when the page shows no note.</summary>
   /// <param name="pickATagInADialog">Shows a tag picker dialog, and returns the tag path picked in it, or null when it was cancelled.</param>
   internal void RemoveATagFromTheShownNote(Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog)
   {
      if(ShownNote() is null) return;

      new OneNoteTagging(_services).RemoveATagFrom(TheShownNote, pickATagInADialog);
   }

   /// <summary>The page asked for <paramref name="pageUrl"/> to be opened in a new tab, as when a link is middle-clicked.</summary>
   public void OpenInBackgroundTab(Uri pageUrl) => _workspace.OpenPageInBackgroundTab(pageUrl);

   [RelayCommand]
   public void Close() => _workspace.CloseTab(this);

   /// <summary>Unsubscribes from the notifications that tell the tab its page is outdated. Called when the tab is closed.</summary>
   public void Dispose() => _subscriptions.ForEach(it => it.Dispose());

   internal bool IsShowing(JPNote note) => note.GetId().Equals(_shownNoteId);

   /// <summary>Called by the workspace when this tab became, or stopped being, its <see cref="WorkspaceViewModel.SelectedTab"/>.</summary>
   internal void SelectionChanged()
   {
      OnPropertyChanged(nameof(IsSelected));
      if(!IsSelected) return;

      _hasBeenShown = true;
      ReloadPageIfItBecameOutdatedWhileHidden();
   }

   void PageBecameOutdated()
   {
      if(!_hasBeenShown) return;

      if(IsSelected)
         PageOutdated?.Invoke();
      else
         _pageOutdatedWhileHidden = true;
   }

   void ReloadPageIfItBecameOutdatedWhileHidden()
   {
      if(!_pageOutdatedWhileHidden) return;

      _pageOutdatedWhileHidden = false;
      PageOutdated?.Invoke();
   }

   /// <summary>
   /// Gives the editor section the editors for the note the tab now shows.<br/>
   /// Also called after the collection reloads, which may have removed the note.<br/>
   /// A reparse for a change to the vocab's matching config changes which sentences its page shows, so the page is then reloaded.
   /// </summary>
   void CreateTheEditorSectionForTheShownNote() =>
      EditorSection = ShownNote() switch
      {
         VocabNote vocab => new VocabEditorSectionViewModel(_services, vocab.GetId(), PageBecameOutdated),
         KanjiNote kanji => new KanjiEditorSectionViewModel(_services, kanji.GetId()),
         SentenceNote sentence => new SentenceEditorSectionViewModel(_services, sentence.GetId()),
         null => null,
         var note => throw new InvalidOperationException($"No editors exist for notes of type {note.GetType().Name}.")
      };

   JPNote? ShownNote() => _shownNoteId is {} noteId ? _services.CoreApp.Collection.NoteFromNoteId(noteId) : null;

   /// <summary>The note on the page, as the collection holds it now, for an action that started while the page showed one.</summary>
   JPNote TheShownNote() => ShownNote() ?? throw new InvalidOperationException("The page no longer shows a note in the collection.");
}
