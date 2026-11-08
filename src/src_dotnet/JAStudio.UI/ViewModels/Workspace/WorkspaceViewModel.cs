using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.Views.Workspace;
using JAStudio.UI.Views.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// A workspace: its tabs (<see cref="NoteTabViewModel"/>), each showing a note's page, which of them is shown,<br/>
/// its <see cref="Navigation"/> pane, and the window title. See <c>dev_docs/jastudio-workspace.md</c>.
/// </summary>
/// <remarks>
/// The view is <see cref="WorkspaceWindow"/>: the <see cref="NavigationPane"/>, the tab strip, each tab's <see cref="NoteTabView"/>,<br/>
/// and the editor section.
/// </remarks>
partial class WorkspaceViewModel : ObservableObject
{
   const string AppTitle = "JAStudio";

   readonly TemporaryServiceCollection _services;
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly SavedWorkspaceState _savedState;

   /// <summary>
   /// Whether the state the workspace was last left in is shown again, or there was none. Until then, saving the state would overwrite it<br/>
   /// with an empty workspace.
   /// </summary>
   bool _theSavedStateIsShown;

   public WorkspaceViewModel(TemporaryServiceCollection services)
   {
      _services = services;
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      _savedState = new SavedWorkspaceState(services.Resolve<IEnvironmentPaths>());
      Navigation = new NavigationPaneViewModel(this, services);
      ShowTheSavedStateOnceTheNotesAreLoaded();
   }

   /// <summary>The navigation pane on the left: ways to find notes, such as the search, which open in this workspace's tabs.</summary>
   public NavigationPaneViewModel Navigation { get; }

   /// <summary>The open tabs, in the order the tab strip shows them.</summary>
   public ObservableCollection<NoteTabViewModel> Tabs { get; } = [];

   /// <summary>The tab being shown. Null only when no tab is open.</summary>
   [ObservableProperty] NoteTabViewModel? _selectedTab;

   /// <summary>The shown tab's <see cref="NoteTabViewModel.Header"/>, followed by the application's name.</summary>
   [ObservableProperty] string _title = AppTitle;

   /// <summary>
   /// Whether the editor section, on the right of the tabs, is open. It shows the editors of the shown tab's note,<br/>
   /// so it stays open as links are followed and tabs are switched.
   /// </summary>
   [ObservableProperty] bool _isEditorSectionOpen;

   /// <summary>The editor section's width, as its edge was last dragged. Bound to the section, so that it's kept between sessions.</summary>
   [ObservableProperty] double _editorSectionWidth = 480;

   /// <summary>Saves what the workspace shows (<see cref="WorkspaceState"/>), to show it again after a restart. The window calls it whenever it's left.</summary>
   internal void SaveTheState()
   {
      if(_theSavedStateIsShown) _savedState.Save(State());
   }

   /// <summary>Shows <paramref name="note"/>: selects a tab that already shows it, or else opens a new tab for it.</summary>
   public void OpenNote(JPNote note) => ShowTabFor(note);

   /// <summary>Shows <paramref name="note"/> as <see cref="OpenNote"/> does, with the editor section open.</summary>
   public void EditNote(JPNote note)
   {
      ShowTabFor(note);
      IsEditorSectionOpen = true;
   }

   public void ToggleEditorSection() => IsEditorSectionOpen = !IsEditorSectionOpen;

   /// <summary>
   /// Opens a new tab for the page at <paramref name="pageUrl"/> without showing it, as a browser does with a middle-clicked link.<br/>
   /// The page loads when the tab is first shown, so it doesn't play its note's audio unseen.
   /// </summary>
   public void OpenPageInBackgroundTab(Uri pageUrl) => AddTab(pageUrl);

   /// <summary>Opens a new tab for <paramref name="note"/> without showing it. See <see cref="OpenPageInBackgroundTab"/>.</summary>
   public void OpenNoteInBackgroundTab(JPNote note) => OpenPageInBackgroundTab(CardServerUrl.NotePageUrl(note));

   /// <summary>Closes <paramref name="tab"/>. If it was the one shown, the tab after it is shown, or else the one before it.</summary>
   public void CloseTab(NoteTabViewModel tab)
   {
      var index = Tabs.IndexOf(tab);
      if(index < 0) throw new ArgumentException("The tab is not open in this workspace.", nameof(tab));

      if(tab == SelectedTab)
         SelectedTab = NeighborOfTheClosedTab();
      Tabs.RemoveAt(index);
      tab.Dispose();

      return;

      NoteTabViewModel? NeighborOfTheClosedTab() =>
         index + 1 < Tabs.Count ? Tabs[index + 1]
         : index > 0            ? Tabs[index - 1]
                                : null;
   }

   public void CloseSelectedTab()
   {
      if(SelectedTab is {} tab)
         CloseTab(tab);
   }

   /// <summary>Shows the tab after the shown one, or the first tab after the last one.</summary>
   public void SelectNextTab() => SelectTabAtDistanceFromTheSelectedOne(1);

   /// <summary>Shows the tab before the shown one, or the last tab before the first one.</summary>
   public void SelectPreviousTab() => SelectTabAtDistanceFromTheSelectedOne(-1);

   /// <summary>Called by <paramref name="tab"/> whenever it sets its <see cref="NoteTabViewModel.Header"/>, which changes when its page shows another note.</summary>
   internal void TabHeaderChanged(NoteTabViewModel tab)
   {
      if(tab == SelectedTab)
         UpdateTitle();
   }

   /// <remarks>
   /// Clicking a tab in the tab strip sets <see cref="SelectedTab"/> through a binding, which would swallow an exception thrown here,<br/>
   /// so it's given to the fatal error handler instead. See <see cref="IFatalErrorHandler.HandleExceptionsFrom"/>.
   /// </remarks>
   partial void OnSelectedTabChanged(NoteTabViewModel? oldValue, NoteTabViewModel? newValue) =>
      _fatalErrorHandler.HandleExceptionsFrom(() =>
      {
         oldValue?.SelectionChanged();
         newValue?.SelectionChanged();
         UpdateTitle();
      });

   /// <summary>
   /// Shows the state the workspace was last left in, if it was saved, as soon as the notes are loaded: at once if they are, or else when<br/>
   /// the collection is first loaded, which notifies <see cref="Core.Note.Collection.JPCollection.ReloadedFromBackend"/>.
   /// </summary>
   void ShowTheSavedStateOnceTheNotesAreLoaded()
   {
      if(_savedState.Load() is not {} saved)
      {
         _theSavedStateIsShown = true;
         return;
      }

      var collection = _services.CoreApp.Collection;
      var uiThread = _services.Resolve<IUIThreadDispatcher>();
      collection.ReloadedFromBackend.Subscribe(_ => uiThread.PostToUIThread(ShowItIfNotShownYet));
      if(collection.IsInitialized) uiThread.PostToUIThread(ShowItIfNotShownYet);
      return;

      void ShowItIfNotShownYet()
      {
         if(_theSavedStateIsShown) return;

         Show(saved);
         _theSavedStateIsShown = true;
      }
   }

   WorkspaceState State() =>
      new(TabNoteIds: [.. Tabs.Select(it => it.ShownNoteId).OfType<NoteId>().Select(it => it.ToString())],
          ShownTabNoteId: SelectedTab?.ShownNoteId?.ToString(),
          IsNavigationPaneOpen: Navigation.IsOpen,
          ShownNavigationPaneTab: Navigation.ShownTab,
          NavigationPaneWidth: Navigation.Width,
          IsEditorSectionOpen: IsEditorSectionOpen,
          EditorSectionWidth: EditorSectionWidth,
          Search: NoteConditionJson.Of(Navigation.Search.CurrentSearch),
          NoteListTitle: Navigation.NoteList.ShownList is { Notes.Count: > 0 } list ? list.Title : null,
          NoteListNoteIds: [.. Navigation.NoteList.ShownList.Notes.Select(it => it.GetId().ToString())]);

   /// <summary>Shows <paramref name="saved"/>. Notes deleted since it was saved are left out.</summary>
   void Show(WorkspaceState saved)
   {
      foreach(var note in NotesWithTheIds(saved.TabNoteIds))
         OpenNoteInBackgroundTab(note);
      SelectedTab = Tabs.FirstOrDefault(it => it.ShownNoteId?.ToString() == saved.ShownTabNoteId) ?? Tabs.FirstOrDefault();
      IsEditorSectionOpen = saved.IsEditorSectionOpen;
      EditorSectionWidth = saved.EditorSectionWidth;

      if(NoteConditionJson.ConditionFrom(saved.Search) is { IsEmpty: false } search)
         Navigation.Search.StartFrom(search);
      if(saved.NoteListTitle is {} noteListTitle)
         Navigation.NoteList.Show(new NoteList(noteListTitle, NotesWithTheIds(saved.NoteListNoteIds)));
      Navigation.Width = saved.NavigationPaneWidth;
      Navigation.ShownTab = saved.ShownNavigationPaneTab;
      Navigation.IsOpen = saved.IsNavigationPaneOpen;
      return;

      List<JPNote> NotesWithTheIds(IEnumerable<string> noteIds) =>
         [.. noteIds.Select(NoteId.Parse).Select(_services.CoreApp.Collection.NoteFromNoteId).OfType<JPNote>()];
   }

   NoteTabViewModel ShowTabFor(JPNote note)
   {
      var tab = Tabs.FirstOrDefault(it => it.IsShowing(note)) ?? AddTab(CardServerUrl.NotePageUrl(note));
      SelectedTab = tab;
      return tab;
   }

   NoteTabViewModel AddTab(Uri pageUrl)
   {
      var tab = new NoteTabViewModel(this, _services, pageUrl);
      Tabs.Add(tab);
      return tab;
   }

   void SelectTabAtDistanceFromTheSelectedOne(int distance)
   {
      if(SelectedTab is not {} selectedTab) return;

      SelectedTab = Tabs[(Tabs.IndexOf(selectedTab) + distance + Tabs.Count) % Tabs.Count];
   }

   void UpdateTitle() => Title = SelectedTab is {} tab ? $"{tab.Header} - {AppTitle}" : AppTitle;
}
