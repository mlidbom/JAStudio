using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.UI.Web;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// A workspace: its tabs (<see cref="NoteTabViewModel"/>), each showing a note's page, which of them is shown,<br/>
/// its <see cref="NoteSearch"/>, and the window title. See <c>dev_docs/jastudio-workspace.md</c>.
/// </summary>
/// <remarks>
/// The view is <see cref="WorkspaceWindow"/>: the <see cref="NoteSearchPane"/>, the tab strip, and each tab's <see cref="NoteTabView"/>.
/// </remarks>
partial class WorkspaceViewModel : ObservableObject
{
   const string AppTitle = "JAStudio";

   readonly TemporaryServiceCollection _services;

   public WorkspaceViewModel(TemporaryServiceCollection services)
   {
      _services = services;
      NoteSearch = new NoteSearchViewModel(this, services);
   }

   /// <summary>The search pane, whose results open in this workspace's tabs.</summary>
   public NoteSearchViewModel NoteSearch { get; }

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

   partial void OnSelectedTabChanged(NoteTabViewModel? oldValue, NoteTabViewModel? newValue)
   {
      oldValue?.SelectionChanged();
      newValue?.SelectionChanged();
      UpdateTitle();
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
