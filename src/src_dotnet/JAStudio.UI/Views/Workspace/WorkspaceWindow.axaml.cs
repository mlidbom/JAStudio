using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.UI.Menus;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// JAStudio's workspace window: the main menu, the <see cref="NoteSearchPane"/> on the left,<br/>
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
      DataContext = _viewModel = new WorkspaceViewModel(services);
      TabViews.ItemTemplate = new FuncDataTemplate<NoteTabViewModel>((tab, _) => new NoteTabView(tab));

      var mainMenuSpec = new JapaneseMainMenu(services).BuildMenuSpec(GetClipboardText);
      foreach(var item in SpecMenuRenderer.BuildMenuItems(mainMenuSpec))
         MainMenu.Items.Add(item);

      _windowShortcuts = new Dictionary<string, Action>
                         {
                            ["Ctrl+o"] = ToggleNoteSearch,
                            ["Ctrl+Shift+o"] = Hide,
                            ["Ctrl+w"] = _viewModel.CloseSelectedTab,
                            ["Ctrl+tab"] = _viewModel.SelectNextTab,
                            ["Ctrl+Shift+tab"] = _viewModel.SelectPreviousTab
                         };
      foreach(var (keyGesture, action) in SpecMenuItem.KeyboardShortcutsIn(mainMenuSpec))
         _windowShortcuts.Add(keyGesture, action);
      foreach(var (keyGesture, action) in _windowShortcuts)
         KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse(keyGesture), Command = new RelayCommand(action) });
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
   /// Ctrl+O. In the active workspace with the note search pane open, closes the pane and gives the keyboard to the shown page.<br/>
   /// Otherwise brings the workspace to the front, opens the pane, and puts the keyboard in the search box.<br/>
   /// Can be called from any thread.
   /// </summary>
   public static void ToggleNoteSearch() => Dispatcher.UIThread.Post(() => Instance.ToggleNoteSearchInThisWindow());

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

   void ToggleNoteSearchInThisWindow()
   {
      if(IsActive && _viewModel.NoteSearch.IsOpen)
         CloseNoteSearch();
      else
         ShowAndFocusNoteSearch();
   }

   void CloseNoteSearch()
   {
      _viewModel.NoteSearch.IsOpen = false;
      FocusShownPage();
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
      _viewModel.NoteSearch.IsOpen = true;
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
      NoteSearchPane.FocusSearchBox();
   }

   void BringToFront()
   {
      if(WindowState == WindowState.Minimized)
         WindowState = WindowState.Normal;
      Show();
      Activate();
   }

   string GetClipboardText() => Clipboard?.TryGetTextAsync().Result ?? string.Empty;

   void OnNoteSearchPaneEdgeDragged(object? sender, VectorEventArgs e) =>
      NoteSearchPane.Width = Math.Max(NoteSearchPane.MinWidth, NoteSearchPane.Width + e.Vector.X);

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
