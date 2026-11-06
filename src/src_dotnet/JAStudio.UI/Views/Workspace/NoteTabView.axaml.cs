using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using JAStudio.UI.Menus;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// The view of one workspace tab (<see cref="NoteTabViewModel"/>): back and forward buttons, and the web view showing the tab's page.<br/>
/// Right-clicking the page gives the same menu as a note shown in Anki, editing included.
/// </summary>
/// <remarks>
/// Every tab has its own web view, and a hidden tab's view stays in the visual tree, only invisible:<br/>
/// removing a <see cref="NativeWebView"/> from the visual tree destroys its native control, losing the page, its history and its scroll position.
/// </remarks>
/// <remarks>
/// The CardServer renders the note's page, and a <see cref="NativeWebView"/> (WebView2 on Windows) shows it.<br/>
/// The page's script, <c>card.js</c>, posts messages to this view through the web view:<br/>
/// which page it shows after every navigation, the selected text when it is right-clicked, and every key pressed on it.<br/>
/// The page reports navigation itself because following a link is Blazor client-side routing,<br/>
/// which raises no <see cref="NativeWebView.NavigationCompleted"/>.<br/>
/// This code-behind only translates between the web view and the <see cref="NoteTabViewModel"/>.
/// </remarks>
public partial class NoteTabView : UserControl
{
   readonly NoteTabViewModel _viewModel;

   [Obsolete("For XAML designer/previewer only")]
#pragma warning disable CS8618 // The XAML designer needs a parameterless constructor, and only the real constructor has the tab's view model.
   public NoteTabView() => InitializeComponent();
#pragma warning restore CS8618

   internal NoteTabView(NoteTabViewModel tab)
   {
      InitializeComponent();
      DataContext = _viewModel = tab;

      WebView.EnvironmentRequested += ConfigureWebView2Environment;
      WebView.AdapterCreated += (_, _) => FocusPageIfShown();
      WebView.NewWindowRequested += OpenNewWindowRequestInNewTab;
      WebView.WebMessageReceived += OnPageMessage;
      tab.PageOutdated += () => Dispatcher.UIThread.Post(() => WebView.Refresh());

      WebView.Navigate(tab.FirstPageUrl);
   }

   /// <summary>
   /// When a tab is shown, the keyboard goes to its page, so that the page's keys act on the note being shown.<br/>
   /// Otherwise the keyboard focus would stay in the web view of the tab that was hidden.
   /// </summary>
   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(change.Property == IsVisibleProperty)
         FocusPageIfShown();
   }

   /// <summary>
   /// Posted so that it runs after the native web view is shown, which Avalonia does after rendering,<br/>
   /// and after a click on the tab's header has finished focusing the header.
   /// </summary>
   void FocusPageIfShown()
   {
      if(IsVisible)
         Dispatcher.UIThread.Post(() => WebView.Focus(), DispatcherPriority.Background);
   }

   WorkspaceWindow WorkspaceWindow => TopLevel.GetTopLevel(this) as WorkspaceWindow
                                   ?? throw new InvalidOperationException("A note tab is only shown in a workspace window.");

   async void OnPageMessage(object? sender, WebMessageReceivedEventArgs e)
   {
      using var message = JsonDocument.Parse(e.Body ?? throw new InvalidOperationException("The note's page posted an empty message."));
      switch(StringProperty("request"))
      {
         case "pageShown":
            OnPageShown(new Uri(StringProperty("url")));
            break;
         case "showNoteContextMenu":
            await ShowNoteContextMenuAsync(StringProperty("selection"));
            break;
         case "keyPressed":
            OnKeyPressedOnPage(StringProperty("keyGesture"));
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(e), e.Body, "Unknown note page message.");
      }

      return;

      string StringProperty(string name) =>
         message.RootElement.GetProperty(name).GetString() ?? throw new InvalidOperationException($"The note page message has no {name}: {e.Body}");
   }

   /// <summary>Runs the workspace window's shortcut for the key pressed on the page, or else the note view shortcut, if any.</summary>
   void OnKeyPressedOnPage(string keyGesture)
   {
      if(!WorkspaceWindow.TryRunWindowShortcut(keyGesture))
         _viewModel.KeyPressed(keyGesture);
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
      var clipboard = await (WorkspaceWindow.Clipboard ?? throw new InvalidOperationException("The workspace window has no clipboard.")).TryGetTextAsync() ?? string.Empty;
      PopupMenuWindow.ShowAt(WindowPositioner.CursorPosition(), _viewModel.NoteContextMenuSpec(selection, clipboard));
   }

   /// <summary>
   /// Keeps the web view's profile in JAStudio's own folder:<br/>
   /// left to itself, WebView2 puts it beside the executable, which inside Anki is Anki's install folder.<br/>
   /// Every tab's web view shares the profile, so they must all ask for the same browser arguments.<br/>
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

   /// <summary>
   /// Middle-clicking a link opens it in a new tab, which is shown at once, like every other way of opening a note in the workspace.<br/>
   /// Left to itself, the web view would open a bare web view window, without JAStudio's menus.
   /// </summary>
   /// <remarks>
   /// The new tab isn't left in the background, as a browser would leave it, because its page would play its note's audio unseen:<br/>
   /// the web view doesn't tell a hidden page that it's hidden.
   /// </remarks>
   void OpenNewWindowRequestInNewTab(object? sender, WebViewNewWindowRequestedEventArgs e)
   {
      e.Handled = true;
      _viewModel.OpenInNewTab(e.Request ?? throw new InvalidOperationException("The page requested a new window without a URL."));
   }

   void OnBackClick(object? sender, RoutedEventArgs e) => WebView.GoBack();

   void OnForwardClick(object? sender, RoutedEventArgs e) => WebView.GoForward();
}
