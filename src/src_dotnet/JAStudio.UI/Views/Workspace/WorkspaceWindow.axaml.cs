using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.ViewModels.Workspace.Tagging;
using JAStudio.UI.Views.Workspace.Tagging;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// JAStudio's workspace window: the main menu, the <see cref="Navigation.NavigationPane"/> on the left,<br/>
/// and tabs, each showing a note whose links are followed in the tab.<br/>
/// See <c>dev_docs/jastudio-workspace.md</c> for where it's heading: editors, and more workspaces.
/// </summary>
/// <remarks>
/// There is a single workspace for now. Closing it only hides it, so it reopens instantly with its tabs intact.
/// </remarks>
/// <remarks>
/// Each tab's view is a <see cref="NoteTabView"/>, created by the item template of <c>TabViews</c>, given its tab's view model.<br/>
/// This code-behind owns the window's keyboard shortcuts, and the mechanics of showing and hiding the window.
/// </remarks>
public partial class WorkspaceWindow : Window
{
   static WorkspaceWindow? _instance;

   readonly WorkspaceViewModel _viewModel;

   /// <summary>
   /// The window's own shortcuts and its main menu's, by key in the form of <see cref="Shortcuts.NoteViewShortcut.KeyGesture"/>.<br/>
   /// They're bound as Avalonia key bindings for when the keyboard is outside the page,<br/>
   /// and run when a page reports their key (<see cref="TryRunWindowShortcut"/>), since keys pressed on a page go to its web view.
   /// </summary>
   readonly Dictionary<string, Action> _windowShortcuts;

   [Obsolete("For XAML designer/previewer only")]
#pragma warning disable CS8618 // The XAML designer needs a parameterless constructor, and only the real constructor has the services for the view model.
   public WorkspaceWindow() => InitializeComponent();
#pragma warning restore CS8618

   WorkspaceWindow(TemporaryServiceCollection services)
   {
      InitializeComponent();
      this.PlacedWhereItWasLast(services);
      DataContext = _viewModel = new WorkspaceViewModel(services);
      TabViews.ItemTemplate = new FuncDataTemplate<NoteTabViewModel>((tab, _) => new NoteTabView(tab));

      var mainMenuSpec = new WorkspaceMainMenu(services).BuildMenuSpec(GetClipboardText);
      foreach(var item in SpecMenuRenderer.BuildMenuItems(mainMenuSpec))
         MainMenu.Items.Add(item);

      _windowShortcuts = new Dictionary<string, Action>
                         {
                            ["Ctrl+o"] = ToggleNoteSearch,
                            ["Ctrl+Shift+o"] = Hide,
                            ["Ctrl+e"] = ToggleEditorSection,
                            ["Ctrl+w"] = _viewModel.CloseSelectedTab,
                            ["Ctrl+t"] = AddATagToTheShownNote,
                            ["Ctrl+Shift+t"] = RemoveATagFromTheShownNote,
                            ["Ctrl+tab"] = _viewModel.SelectNextTab,
                            ["Ctrl+Shift+tab"] = _viewModel.SelectPreviousTab
                         };
      foreach(var (keyGesture, action) in SpecMenuItem.KeyboardShortcutsIn(mainMenuSpec))
         _windowShortcuts.Add(keyGesture, action);
      foreach(var (keyGesture, action) in _windowShortcuts)
         KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse(keyGesture), Command = new RelayCommand(action) });

      Activated += (_, _) => Dispatcher.UIThread.Post(GiveTheKeyboardBackToTheWindowUnlessAPageHasIt, DispatcherPriority.Background);
      AddHandler(PointerPressedEvent, (_, _) => NativeKeyboardFocus.MoveToWindowItself(this), RoutingStrategies.Tunnel, handledEventsToo: true);
   }

   /// <summary>
   /// When the window is activated, as after Alt+Tab back to it, Windows gives the keyboard back to the native window that had it last,<br/>
   /// which can be a tab's web view even when an Avalonia control, such as the search's condition tree, has Avalonia's focus.<br/>
   /// The keys then go to the page instead of the control. So unless a page is meant to have the keyboard, the window takes it back.
   /// </summary>
   /// <remarks>Clicking an Avalonia control takes the keyboard back too: the window's handler for pointer presses does that, before any control handles the press.</remarks>
   /// <remarks>
   /// Written on 2026-11-07 while Windows' keyboard input was broken system-wide, so the problem it answers may well have been that fault.<br/>
   /// It's kept because it's harmless and was tested together with the rest. See <c>dev_docs/jastudio-workspace.md</c>.
   /// </remarks>
   /// <remarks>
   /// It's posted, so a dialog may have opened over the window meanwhile, such as a search condition's editor.<br/>
   /// Taking the keyboard then would take it from the dialog. <see cref="Window.IsActive"/> can still say the window is active at that point,<br/>
   /// so the dialog itself is looked for too.
   /// </remarks>
   void GiveTheKeyboardBackToTheWindowUnlessAPageHasIt()
   {
      if(!IsActive) return;
      if(this.HasAWindowOpenOverIt()) return;
      if(FocusManager.GetFocusedElement() is NativeWebView) return;

      NativeKeyboardFocus.MoveToWindowItself(this);
   }

   static WorkspaceWindow Instance => _instance ?? throw new InvalidOperationException("The workspace window has not been created yet.");

   internal static void CreateAndRegister(TemporaryServiceCollection services)
   {
      _instance = new WorkspaceWindow(services);
      if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         desktop.MainWindow = _instance;
   }

   public static void ToggleVisibility()
   {
      if(Instance.IsVisible)
         Instance.Hide();
      else
         Instance.BringToFront();
   }

   /// <summary>
   /// Shows <paramref name="note"/> in the workspace, in the tab already showing it or else in a new tab, and brings the workspace to the front.<br/>
   /// Can be called from any thread.
   /// </summary>
   public static void OpenNote(JPNote note) => Dispatcher.UIThread.Post(() => Instance.ShowNote(note));

   /// <summary>
   /// Shows <paramref name="note"/> as <see cref="OpenNote"/> does, with the tab's editor section open, and brings the workspace to the front.<br/>
   /// Can be called from any thread.
   /// </summary>
   public static void EditNote(JPNote note) => Dispatcher.UIThread.Post(() => Instance.ShowNoteInEditor(note));

   /// <summary>
   /// Ctrl+O. In the active workspace with the navigation pane open on its search, closes the pane and gives the keyboard to the shown page.<br/>
   /// Otherwise brings the workspace to the front, opens the pane on its search, and puts the keyboard on the search's condition tree.<br/>
   /// Can be called from any thread.
   /// </summary>
   public static void ToggleNoteSearch() => Dispatcher.UIThread.Post(() => Instance.ToggleNoteSearchInThisWindow());

   /// <summary>
   /// Lists <paramref name="noteList"/> in the navigation pane's "List" tab, instead of the list shown before, and brings the workspace to the front.<br/>
   /// Can be called from any thread.
   /// </summary>
   internal static void ShowNoteList(NoteList noteList) => Dispatcher.UIThread.Post(() => Instance.ShowNoteListInThisWindow(noteList));

   /// <summary>Runs the window's shortcut bound to <paramref name="keyGesture"/>, if there is one, and says whether there was.</summary>
   internal bool TryRunWindowShortcut(string keyGesture)
   {
      if(!_windowShortcuts.TryGetValue(keyGesture, out var windowShortcut)) return false;

      windowShortcut();
      return true;
   }

   void ShowNote(JPNote note)
   {
      _viewModel.OpenNote(note);
      BringToFront();
   }

   void ShowNoteInEditor(JPNote note)
   {
      _viewModel.EditNote(note);
      BringToFront();
   }

   /// <summary>
   /// Ctrl+T: adds a tag, picked in a dialog over the window, to the note the shown tab shows.<br/>
   /// The search's results grid takes Ctrl+T itself while it has the keyboard, for its selected notes.
   /// </summary>
   void AddATagToTheShownNote() => _viewModel.SelectedTab?.AddATagToTheShownNote(PickATagInADialogOverTheWindow);

   /// <summary>Ctrl+Shift+T: removes one of the tags of the note the shown tab shows, picked in a dialog over the window.</summary>
   void RemoveATagFromTheShownNote() => _viewModel.SelectedTab?.RemoveATagFromTheShownNote(PickATagInADialogOverTheWindow);

   Task<string?> PickATagInADialogOverTheWindow(TagPickerDialogViewModel dialog) => TagPickerDialog.ShowCenteredOverAsync(this, dialog);

   /// <summary>Ctrl+E. Afterwards the keyboard is on the page, so the note view shortcuts work whether the section opened or closed.</summary>
   void ToggleEditorSection()
   {
      _viewModel.ToggleEditorSection();
      FocusShownPage();
   }

   void ToggleNoteSearchInThisWindow()
   {
      if(IsActive && _viewModel.Navigation is { IsOpen: true, ShownTab: NavigationPaneTab.Search })
         CloseNavigationPane();
      else
         ShowAndFocusNoteSearch();
   }

   void CloseNavigationPane()
   {
      _viewModel.Navigation.IsOpen = false;
      FocusShownPage();
   }

   void ShowNoteListInThisWindow(NoteList noteList)
   {
      _viewModel.Navigation.ShowNoteList(noteList);
      BringToFront();
   }

   void FocusShownPage()
   {
      if(_viewModel.SelectedTab is not {} shownTab) return;

      if(TabViews.ContainerFromItem(shownTab) is not ContentPresenter { Child: NoteTabView shownTabView })
         throw new InvalidOperationException("The shown tab has no NoteTabView.");
      shownTabView.FocusPage();
   }

   void ShowAndFocusNoteSearch()
   {
      _viewModel.Navigation.ShowSearch();
      BringToFront();
      Dispatcher.UIThread.Post(FocusNoteSearch, DispatcherPriority.Background);
   }

   /// <summary>
   /// Posted by <see cref="ShowAndFocusNoteSearch"/>, so that it runs after the window is activated and the pane is laid out.<br/>
   /// The keyboard is usually in a tab's web view, which keeps it unless the window takes it back first.
   /// </summary>
   void FocusNoteSearch()
   {
      NativeKeyboardFocus.MoveToWindowItself(this);
      NavigationPane.FocusSearch();
   }

   void BringToFront()
   {
      if(WindowState == WindowState.Minimized)
         WindowState = WindowState.Normal;
      Show();
      Activate();
   }

   string GetClipboardText() => Clipboard?.TryGetTextAsync().Result ?? string.Empty;

   /// <summary>Middle-clicking a tab's header closes the tab, as in a browser.</summary>
   void OnTabHeaderPointerReleased(object? sender, PointerReleasedEventArgs e)
   {
      if(e.InitialPressMouseButton == MouseButton.Middle && sender is Control { DataContext: NoteTabViewModel tab })
         tab.Close();
   }

   protected override void OnClosing(WindowClosingEventArgs e)
   {
      base.OnClosing(e);
      e.Cancel = true;
      Hide();
   }
}
