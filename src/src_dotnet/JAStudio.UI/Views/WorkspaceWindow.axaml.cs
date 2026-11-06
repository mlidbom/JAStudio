using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.UI.Web;
using JAStudio.UI.Menus;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

/// <summary>
/// JAStudio's workspace window: the main menu, and the note being viewed, whose links are followed in place.<br/>
/// Right-clicking the note gives the same menu as a note shown in Anki, editing included.<br/>
/// See <c>dev_docs/jastudio-workspace.md</c> for where it's heading: tabs, search and editors.
/// </summary>
/// <remarks>
/// There is a single workspace for now. Closing it only hides it, so it reopens instantly with its history intact.
/// </remarks>
/// <remarks>
/// The CardServer renders the note's page, and a <see cref="NativeWebView"/> (WebView2 on Windows) shows it.<br/>
/// The page's script, <c>card.js</c>, posts messages to this window through the web view:<br/>
/// which page it shows after every navigation, and the selected text when it is right-clicked.<br/>
/// The page reports navigation itself because following a link is Blazor client-side routing,<br/>
/// which raises no <see cref="NativeWebView.NavigationCompleted"/>.<br/>
/// This code-behind only translates between the web view and the <see cref="WorkspaceViewModel"/>.
/// </remarks>
public partial class WorkspaceWindow : Window
{
   static WorkspaceWindow? _instance;

   readonly WorkspaceViewModel _viewModel;

   [Obsolete("For XAML designer/previewer only")]
#pragma warning disable CS8618 // The XAML designer needs a parameterless constructor, and only the real constructor has the services for the view model.
   public WorkspaceWindow() => InitializeComponent();
#pragma warning restore CS8618

   WorkspaceWindow(TemporaryServiceCollection services)
   {
      InitializeComponent();
      DataContext = _viewModel = new WorkspaceViewModel(services);

      KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+Shift+O"), Command = new RelayCommand(Hide) });
      KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+O"), Command = new RelayCommand(() => NoteSearchDialog.ToggleVisibility(services)) });
      foreach(var item in SpecMenuRenderer.BuildMenuItems(new JapaneseMainMenu(services).BuildMenuSpec(GetClipboardText)))
         MainMenu.Items.Add(item);

      WebView.EnvironmentRequested += ConfigureWebView2Environment;
      WebView.NewWindowRequested += OpenNewWindowRequestInPlace;
      WebView.WebMessageReceived += OnPageMessage;
      _viewModel.ShownNoteUpdated += () => Dispatcher.UIThread.Post(() => WebView.Refresh());
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
   /// Shows <paramref name="note"/> in the workspace and brings the workspace to the front.<br/>
   /// Can be called from any thread.
   /// </summary>
   public static void OpenNote(JPNote note) => Dispatcher.UIThread.Post(() => Instance.ShowNote(note));

   void ShowNote(JPNote note)
   {
      WebView.Navigate(CardServerUrl.NotePageUrl(note));
      BringToFront();
   }

   void BringToFront()
   {
      if(WindowState == WindowState.Minimized)
         WindowState = WindowState.Normal;
      Show();
      Activate();
   }

   string GetClipboardText() => Clipboard?.TryGetTextAsync().Result ?? string.Empty;

   async void OnPageMessage(object? sender, WebMessageReceivedEventArgs e)
   {
      using var message = JsonDocument.Parse(e.Body ?? throw new InvalidOperationException("The workspace page posted an empty message."));
      switch(StringProperty("request"))
      {
         case "pageShown":
            OnPageShown(new Uri(StringProperty("url")));
            break;
         case "showNoteContextMenu":
            await ShowNoteContextMenuAsync(StringProperty("selection"));
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(e), e.Body, "Unknown workspace page message.");
      }

      return;

      string StringProperty(string name) =>
         message.RootElement.GetProperty(name).GetString() ?? throw new InvalidOperationException($"The workspace page message has no {name}: {e.Body}");
   }

   void OnPageShown(Uri pageUrl)
   {
      _viewModel.PageShown(pageUrl);
      _viewModel.CanGoBack = WebView.CanGoBack;
      _viewModel.CanGoForward = WebView.CanGoForward;
   }

   /// <summary>Opens the shown note's right-click menu, the same one Anki shows for it, where the mouse cursor is.</summary>
   async Task ShowNoteContextMenuAsync(string selection)
   {
      var clipboard = await (Clipboard ?? throw new InvalidOperationException("The workspace window has no clipboard.")).TryGetTextAsync() ?? string.Empty;
      PopupMenuWindow.ShowAt(WindowPositioner.CursorPosition(), _viewModel.NoteContextMenuSpec(selection, clipboard));
   }

   /// <summary>
   /// Keeps the web view's profile in JAStudio's own folder:<br/>
   /// left to itself, WebView2 puts it beside the executable, which inside Anki is Anki's install folder.<br/>
   /// Lets the page play the shown note's audio without a click first, as Anki does on a card.
   /// </summary>
   static void ConfigureWebView2Environment(object? sender, WebViewEnvironmentRequestedEventArgs e)
   {
      e.EnableDevTools = true;
      if(e is WindowsWebView2EnvironmentRequestedEventArgs webView2)
      {
         webView2.UserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio", "WorkspaceWebView2");
         webView2.AdditionalBrowserArguments = "--autoplay-policy=no-user-gesture-required";
      }
   }

   /// <summary>Middle-clicking a link would otherwise open a bare web view window, without JAStudio's menus.</summary>
   void OpenNewWindowRequestInPlace(object? sender, WebViewNewWindowRequestedEventArgs e)
   {
      e.Handled = true;
      WebView.Navigate(e.Request ?? throw new InvalidOperationException("The page requested a new window without a URL."));
   }

   void OnBackClick(object? sender, RoutedEventArgs e) => WebView.GoBack();

   void OnForwardClick(object? sender, RoutedEventArgs e) => WebView.GoForward();

   protected override void OnClosing(WindowClosingEventArgs e)
   {
      base.OnClosing(e);
      e.Cancel = true;
      Hide();
   }
}
