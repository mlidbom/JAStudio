using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.UI.Web;
using JAStudio.UI.Menus;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Views;

/// <summary>
/// JAStudio's window for viewing notes, replacing Anki's previewer.<br/>
/// It shows a note's card page and follows the links to other notes in place.<br/>
/// Right-clicking gives the same menu as a note shown in Anki, editing included.
/// </summary>
/// <remarks>
/// There is a single note viewer. Closing it only hides it, so it reopens instantly with its history intact.
/// </remarks>
/// <remarks>
/// The CardServer renders the page, and a <see cref="NativeWebView"/> (WebView2 on Windows) shows it.<br/>
/// The page's script, <c>card.js</c>, posts messages to this window through the web view:<br/>
/// which page it shows after every navigation, and the selected text when it is right-clicked.<br/>
/// The page reports navigation itself because following a link is Blazor client-side routing,<br/>
/// which raises no <see cref="NativeWebView.NavigationCompleted"/>.
/// </remarks>
public partial class NoteViewerWindow : Window
{
   static NoteViewerWindow? _instance;

   readonly TemporaryServiceCollection _services;

   /// <summary>The note on the page currently shown, as last reported by the page. Null for a page that shows no note.</summary>
   NoteId? _shownNoteId;

   [Obsolete("For XAML designer/previewer only")]
#pragma warning disable CS8618 // The XAML designer needs a parameterless constructor, and only the real constructor has the services.
   public NoteViewerWindow() => InitializeComponent();
#pragma warning restore CS8618

   NoteViewerWindow(TemporaryServiceCollection services)
   {
      _services = services;
      InitializeComponent();

      WebView.EnvironmentRequested += ConfigureWebView2Environment;
      WebView.NewWindowRequested += OpenNewWindowRequestInPlace;
      WebView.WebMessageReceived += OnPageMessage;
      services.CoreApp.Collection.OnNoteUpdated(note => Dispatcher.UIThread.Post(() => ReloadIfShowing(note)));
   }

   /// <summary>
   /// Shows <paramref name="note"/> in the note viewer, opening the window first if needed, and brings it to the front.<br/>
   /// Can be called from any thread.
   /// </summary>
   public static void OpenNote(JPNote note, TemporaryServiceCollection services) =>
      Dispatcher.UIThread.Post(() => (_instance ??= new NoteViewerWindow(services)).ShowNote(note));

   void ShowNote(JPNote note)
   {
      WebView.Navigate(CardServerUrl.NotePageUrl(note));
      if(WindowState == WindowState.Minimized)
         WindowState = WindowState.Normal;
      Show();
      Activate();
   }

   async void OnPageMessage(object? sender, WebMessageReceivedEventArgs e)
   {
      using var message = JsonDocument.Parse(e.Body ?? throw new InvalidOperationException("The note viewer page posted an empty message."));
      switch(StringProperty("request"))
      {
         case "pageShown":
            OnPageShown(new Uri(StringProperty("url")));
            break;
         case "showNoteContextMenu":
            await ShowNoteContextMenuAsync(StringProperty("selection"));
            break;
         default:
            throw new ArgumentOutOfRangeException(nameof(e), e.Body, "Unknown note viewer page message.");
      }

      return;

      string StringProperty(string name) =>
         message.RootElement.GetProperty(name).GetString() ?? throw new InvalidOperationException($"The note viewer page message has no {name}: {e.Body}");
   }

   void OnPageShown(Uri pageUrl)
   {
      _shownNoteId = CardServerUrl.NoteIdOfPage(pageUrl);
      Title = ShownNote() is {} note ? $"{note.GetQuestion()} - Note viewer" : "Note viewer";
      BackButton.IsEnabled = WebView.CanGoBack;
      ForwardButton.IsEnabled = WebView.CanGoForward;
   }

   JPNote? ShownNote() => _shownNoteId is {} noteId ? _services.CoreApp.Collection.NoteFromNoteId(noteId) : null;

   void ReloadIfShowing(JPNote updatedNote)
   {
      if(updatedNote.GetId().Equals(_shownNoteId))
         WebView.Refresh();
   }

   /// <summary>Opens the shown note's right-click menu, the same one Anki shows for it, where the mouse cursor is.</summary>
   async Task ShowNoteContextMenuAsync(string selection)
   {
      var clipboard = await (Clipboard ?? throw new InvalidOperationException("The note viewer window has no clipboard.")).TryGetTextAsync() ?? string.Empty;
      var menuSpec = new NoteContextMenu(_services).BuildContextMenuSpec(ShownNote(), selection, clipboard);

      new ContextMenu
      {
         ItemsSource = SpecMenuRenderer.BuildMenuItems(menuSpec).ToList(),
         Placement = PlacementMode.AnchorAndGravity,
         PlacementRect = new Rect(this.PointToClient(WindowPositioner.CursorPosition()), new Size(1, 1)),
         PlacementAnchor = PopupAnchor.TopLeft,
         PlacementGravity = PopupGravity.BottomRight
      }.Open(this);
   }

   /// <summary>
   /// Keeps the web view's profile in JAStudio's own folder.<br/>
   /// Left to itself, WebView2 puts it beside the executable, which inside Anki is Anki's install folder.
   /// </summary>
   static void ConfigureWebView2Environment(object? sender, WebViewEnvironmentRequestedEventArgs e)
   {
      e.EnableDevTools = true;
      if(e is WindowsWebView2EnvironmentRequestedEventArgs webView2)
         webView2.UserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio", "NoteViewerWebView2");
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
