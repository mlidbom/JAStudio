using System;
using System.Collections.Generic;
using System.Linq;
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
using JAStudio.UI.ViewModels.Workspace.NotePages;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;
using JAStudio.UI.ViewModels.Workspace.Tagging;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// A tab in a workspace (<see cref="WorkspaceViewModel"/>). It shows a note's page, and has its own back and forward history (<see cref="NoteTabHistory"/>),<br/>
/// as a browser tab does. Following a link on the page shows the linked note in the same tab.
/// </summary>
/// <remarks>
/// The tab decides which note it shows, and the view, <see cref="NoteTabView"/>, shows that note's page: a sentence's in Avalonia (<see cref="NotePage"/>),<br/>
/// the other notes' card pages in its web view. The view reports here the links followed and the keys pressed on a card page, and reloads it when<br/>
/// <see cref="PageOutdated"/> says so. A page in Avalonia the tab makes anew instead. See <c>dev_docs/jastudio-workspace.md</c>, "Technology".
/// </remarks>
/// <remarks>
/// A tab's page loads the first time the tab is shown, so a tab opened in the background has no page until then.<br/>
/// A hidden tab isn't reloaded in the background. If its page became outdated while it was hidden,<br/>
/// <see cref="PageOutdated"/> is raised when the tab is shown again.
/// </remarks>
partial class NoteTabViewModel : ObservableObject, IDisposable, INotePageHost
{
   readonly WorkspaceViewModel _workspace;
   readonly TemporaryServiceCollection _services;
   readonly IUIThreadDispatcher _uiThread;
   readonly NoteViewShortcuts _noteViewShortcuts;
   readonly NoteTabHistory _history;
   readonly List<IDisposable> _subscriptions;

   /// <summary>Whether the tab has been shown, and so has loaded its page. Until then there is no page to become outdated.</summary>
   bool _hasBeenShown;

   bool _pageOutdatedWhileHidden;

   /// <summary>Whether the shown note's own audio is yet to be played on its page in Avalonia, which it is when the tab is first shown with it.</summary>
   bool _theShownNotesAudioIsToBePlayed;

   /// <summary>The question of the note on the page, shown on the tab and in the window title.</summary>
   [ObservableProperty] public partial string Header { get; set; } = "";

   /// <summary>
   /// What the workspace's editor section shows for this tab: the editors of the note it shows.<br/>
   /// Each tab has its own, for the note it shows, and the section shows the shown tab's.
   /// </summary>
   [ObservableProperty] public partial INoteEditorSectionViewModel? EditorSection { get; set; }

   /// <summary>
   /// The shown note's page in Avalonia, for the notes whose pages are built in Avalonia: sentences, for now. None while the shown note's page is its<br/>
   /// card's page, which the view shows in its web view.
   /// </summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(ShowsTheCardPage))]
   public partial NotePageViewModel? NotePage { get; set; }

   /// <summary>Whether the shown note's page is its card's page, in the view's web view, rather than a page in Avalonia (<see cref="NotePage"/>).</summary>
   public bool ShowsTheCardPage => NotePage is null;

   public NoteTabViewModel(WorkspaceViewModel workspace, TemporaryServiceCollection services, NoteId firstNote)
   {
      _workspace = workspace;
      _services = services;
      _noteViewShortcuts = new NoteViewShortcuts(services.CoreApp.Config);
      _history = new NoteTabHistory(firstNote);
      _uiThread = services.Resolve<IUIThreadDispatcher>();
      ShowTheNoteNowShown();

      var collection = services.CoreApp.Collection;
      _subscriptions =
      [
         collection.NoteUpdated.Subscribe(ShowTheChangeIfItsTheShownNote),
         collection.StudyNoteChanged.Subscribe(ShowTheChangeIfItsTheShownNote),
         collection.NoteDeleted.Subscribe(note => _uiThread.PostToUIThread(() => LeaveTheNotesThatLeftTheCollection(it => it.Equals(note.GetId())))),
         collection.ReloadedFromBackend.Subscribe(_ => _uiThread.PostToUIThread(() =>
         {
            LeaveTheNotesThatLeftTheCollection(it => collection.NoteFromNoteId(it) is null);
            PageBecameOutdated();
            CreateTheEditorSectionForTheShownNote();
         })),
         services.CoreApp.Config.Changed.Subscribe(_ =>
         {
            PageBecameOutdated();
            _uiThread.PostToUIThread(() => EditorSection?.Refresh());
         })
      ];
      return;

      void ShowTheChangeIfItsTheShownNote(JPNote note)
      {
         if(!IsShowing(note)) return;

         PageBecameOutdated();
         _uiThread.PostToUIThread(() => EditorSection?.Refresh());
      }
   }

   /// <summary>The address of the shown note's page on the CardServer, which the view shows in its web view.</summary>
   public Uri ShownPageUrl => CardServerUrl.NotePageUrl(TheShownNote());

   /// <summary>Whether this is the workspace's <see cref="WorkspaceViewModel.SelectedTab"/>: the one being shown.</summary>
   public bool IsSelected => _workspace.SelectedTab == this;

   /// <summary>
   /// Raised when the page no longer shows the current state, so it must be reloaded:<br/>
   /// the note on it was updated, its study-note in Anki changed, such as its tags, the collection was loaded again,<br/>
   /// or a setting changed, such as how sentence breakdowns are shown.<br/>
   /// It's raised on the thread that made the change, or, for a tab that was hidden, when the tab is shown.
   /// </summary>
   public event Action? PageOutdated;

   /// <summary>Raised on the UI thread when the tab shows another note than before, as when a link was followed or back was pressed, so that the view shows its page.</summary>
   public event Action? ShowsAnotherNote;

   /// <summary>
   /// Shows <paramref name="note"/> after the one shown, as following a link on the page does: going back shows the note shown now. Showing the note<br/>
   /// already shown changes nothing.
   /// </summary>
   public void ShowNote(NoteId note)
   {
      if(note.Equals(ShownNoteId)) return;

      _history.Show(note);
      ShowTheNoteNowShown();
   }

   /// <summary>
   /// The view reports that its web view shows the page at <paramref name="pageUrl"/>. That's the shown note's page, which it was told to load, unless<br/>
   /// the page went to another by itself, as the browser's own back does: then the tab shows that page's note, as if a link to it had been followed.
   /// </summary>
   public void PageShown(Uri pageUrl)
   {
      if(CardServerUrl.NoteIdOfPage(pageUrl) is {} note) ShowNote(note);
   }

   /// <summary>Shows the note shown before the one shown now, as a browser's back does.</summary>
   [RelayCommand(CanExecute = nameof(CanGoBack))]
   void GoBack()
   {
      _history.GoBack();
      ShowTheNoteNowShown();
   }

   /// <summary>Shows the note shown after the one shown now, which was left by going back, as a browser's forward does.</summary>
   [RelayCommand(CanExecute = nameof(CanGoForward))]
   void GoForward()
   {
      _history.GoForward();
      ShowTheNoteNowShown();
   }

   bool CanGoBack => _history.CanGoBack;

   bool CanGoForward => _history.CanGoForward;

   /// <summary>Runs the note view shortcut bound to <paramref name="keyGesture"/>, if any, on the note on the page.</summary>
   public void KeyPressed(string keyGesture) => _noteViewShortcuts.BoundTo(keyGesture)?.Run(TheShownNote());

   /// <summary>The right-click menu for the note on the page: the same one Anki shows for it.</summary>
   public List<SpecMenuItem> NoteContextMenuSpec(string selection, string clipboard) =>
      new NoteContextMenu(_services).BuildContextMenuSpec(TheShownNote(), selection, clipboard);

   /// <summary>Ctrl+T: picks a tag through <paramref name="pickATagInADialog"/> and adds it to the note on the page.</summary>
   /// <param name="pickATagInADialog">Shows a tag picker dialog, and returns the tag path picked in it, or null when it was cancelled.</param>
   internal void AddATagToTheShownNote(Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog) =>
      new OneNoteTagging(_services).AddATagTo(TheShownNote, pickATagInADialog);

   /// <summary>Ctrl+Shift+T: picks one of the tags of the note on the page through <paramref name="pickATagInADialog"/>, and removes it.</summary>
   /// <param name="pickATagInADialog">Shows a tag picker dialog, and returns the tag path picked in it, or null when it was cancelled.</param>
   internal void RemoveATagFromTheShownNote(Func<TagPickerDialogViewModel, Task<string?>> pickATagInADialog) =>
      new OneNoteTagging(_services).RemoveATagFrom(TheShownNote, pickATagInADialog);

   /// <summary>
   /// Ctrl+M: opens the match-condition window of the vocab on the page through <paramref name="openTheMatchConditionWindowOf"/>. Does nothing when the page<br/>
   /// shows no vocab.
   /// </summary>
   internal void OpenTheMatchConditionWindowOfTheShownVocab(Action<VocabNote> openTheMatchConditionWindowOf)
   {
      if(TheShownNote() is VocabNote vocab)
         openTheMatchConditionWindowOf(vocab);
   }

   /// <summary>
   /// Reloads the page, now or when the tab is shown next, if it shows one of <paramref name="vocab"/>, whose sentences were reparsed after a change made<br/>
   /// outside the tab, as in a vocab's match-condition window: a vocab's page shows the sentences it matches.
   /// </summary>
   internal void ShowTheSentencesReparsedFor(IReadOnlyCollection<VocabNote> vocab)
   {
      if(vocab.Any(IsShowing))
         PageBecameOutdated();
   }

   /// <summary>The page asked for <paramref name="pageUrl"/>, a note's page, to be opened in a new tab, as when a link is middle-clicked.</summary>
   public void OpenInBackgroundTab(Uri pageUrl) =>
      OpenNoteInBackgroundTab(CardServerUrl.NoteIdOfPage(pageUrl) ?? throw new ArgumentException($"{pageUrl} is no note's page.", nameof(pageUrl)));

   /// <summary>Opens <paramref name="note"/> in a new tab without showing it, as middle-clicking a link to it on the page does.</summary>
   public void OpenNoteInBackgroundTab(NoteId note) => _workspace.OpenNoteInBackgroundTab(note);

   [RelayCommand]
   public void Close() => _workspace.CloseTab(this);

   /// <summary>Unsubscribes from the notifications that tell the tab its page is outdated. Called when the tab is closed.</summary>
   public void Dispose() => _subscriptions.ForEach(it => it.Dispose());

   internal bool IsShowing(JPNote note) => note.GetId().Equals(ShownNoteId);

   /// <summary>The id of the note the tab shows.</summary>
   internal NoteId ShownNoteId => _history.Shown;

   /// <summary>Called by the workspace when this tab became, or stopped being, its <see cref="WorkspaceViewModel.SelectedTab"/>.</summary>
   internal void SelectionChanged()
   {
      OnPropertyChanged(nameof(IsSelected));
      if(!IsSelected) return;

      _hasBeenShown = true;
      ReloadPageIfItBecameOutdatedWhileHidden();
      PlayTheShownNotesAudioIfTheTabIsShown();
   }

   /// <summary>
   /// After notes left the collection, deleted or gone after a reload, the tab closes if it shows one of them, and otherwise forgets them, so that going<br/>
   /// back or forward never reaches one.
   /// </summary>
   void LeaveTheNotesThatLeftTheCollection(Func<NoteId, bool> leftTheCollection)
   {
      if(leftTheCollection(ShownNoteId))
      {
         Close();
         return;
      }

      _history.Forget(leftTheCollection);
      NotifyWhetherItCanGoBackAndForward();
   }

   void ShowTheNoteNowShown()
   {
      Header = TheShownNote().GetQuestion();
      _workspace.TabHeaderChanged(this);
      CreateTheEditorSectionForTheShownNote();
      NotifyWhetherItCanGoBackAndForward();
      NotePage = PageOfTheShownNote();
      _theShownNotesAudioIsToBePlayed = true;
      PlayTheShownNotesAudioIfTheTabIsShown();
      ShowsAnotherNote?.Invoke();
   }

   /// <summary>The shown note's page in Avalonia, as it is now; none for a note whose page is its card's page.</summary>
   NotePageViewModel? PageOfTheShownNote() =>
      TheShownNote() switch
      {
         SentenceNote sentence => new SentencePageViewModel(sentence, _services, this),
         _ => null
      };

   /// <summary>
   /// Plays the shown note's own audio on its page in Avalonia, once the tab is shown with it, as the card's page does in the web view, and Anki on a<br/>
   /// card. Only once for each note shown, so a tab shown again doesn't play it again, nor does a page made anew after a change.
   /// </summary>
   void PlayTheShownNotesAudioIfTheTabIsShown()
   {
      if(!IsSelected || !_theShownNotesAudioIsToBePlayed) return;

      _theShownNotesAudioIsToBePlayed = false;
      NotePage?.PlayTheNotesAudio();
   }

   void NotifyWhetherItCanGoBackAndForward()
   {
      GoBackCommand.NotifyCanExecuteChanged();
      GoForwardCommand.NotifyCanExecuteChanged();
   }

   void PageBecameOutdated()
   {
      if(!_hasBeenShown) return;

      if(IsSelected)
         ShowThePageAnew();
      else
         _pageOutdatedWhileHidden = true;
   }

   void ReloadPageIfItBecameOutdatedWhileHidden()
   {
      if(!_pageOutdatedWhileHidden) return;

      _pageOutdatedWhileHidden = false;
      ShowThePageAnew();
   }

   /// <summary>Shows the page as the note and the settings are now: a page in Avalonia is made anew, on the UI thread, and the view reloads a card's page.</summary>
   void ShowThePageAnew()
   {
      if(NotePage is null)
         PageOutdated?.Invoke();
      else
         _uiThread.PostToUIThread(() => NotePage = PageOfTheShownNote());
   }

   /// <summary>
   /// Gives the editor section the editors for the note the tab now shows.<br/>
   /// Also called after the collection reloads, which replaces the note objects.<br/>
   /// A reparse for a change to the vocab's matching config changes which sentences its page shows, so the page is then reloaded.
   /// </summary>
   void CreateTheEditorSectionForTheShownNote() =>
      EditorSection = TheShownNote() switch
      {
         VocabNote vocab => new VocabEditorSectionViewModel(_services, vocab.GetId(), PageBecameOutdated, _workspace.EditNote),
         KanjiNote kanji => new KanjiEditorSectionViewModel(_services, kanji.GetId()),
         SentenceNote sentence => new SentenceEditorSectionViewModel(_services, sentence.GetId(), this),
         var note => throw new InvalidOperationException($"No editors exist for notes of type {note.GetType().Name}.")
      };

   /// <summary>The note the tab shows, as the collection holds it now. The tab closes when its note leaves the collection, so it's always there.</summary>
   JPNote TheShownNote() =>
      _services.CoreApp.Collection.NoteFromNoteId(ShownNoteId) ?? throw new InvalidOperationException($"The note the tab shows, {ShownNoteId}, is no longer in the collection.");
}
