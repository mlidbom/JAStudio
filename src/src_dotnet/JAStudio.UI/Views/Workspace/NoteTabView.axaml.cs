using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Avalonia.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.UI.Web;
using JAStudio.UI.Menus;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// The view of one workspace tab (<see cref="NoteTabViewModel"/>): back and forward buttons, and the page of the note the tab shows, a sentence's in<br/>
/// Avalonia (<see cref="NotePagePresenter"/>), the other notes' card pages in a web view. Right-clicking a card's page gives the same menu as a note<br/>
/// shown in Anki, editing included.
/// </summary>
/// <remarks>
/// Every tab has its own web view, and a hidden tab's view stays in the visual tree, only invisible:<br/>
/// removing a <see cref="NativeWebView"/> from the visual tree destroys its native control, losing the page, its history and its scroll position.
/// </remarks>
/// <remarks>
/// The CardServer renders the note's page, and a <see cref="NativeWebView"/> (WebView2 on Windows) shows it.<br/>
/// The page's script, <c>card.js</c>, posts messages to this view through the web view: the note link clicked, which the tab then shows, which page it<br/>
/// shows after every navigation, the selected text when it is right-clicked, every key pressed on it, and the mouse's back and forward buttons.<br/>
/// The tab keeps the history (<see cref="NoteTabHistory"/>), so the web view only shows the page of the note the tab shows, and keeps no history of its<br/>
/// own (<see cref="LoadThePageOfTheShownNote"/>). A page reports the pages it shows in case it goes to another by itself, which the tab then follows.<br/>
/// This code-behind only translates between the web view and the <see cref="NoteTabViewModel"/>.
/// </remarks>
public partial class NoteTabView : UserControl, INoteTabView
{
   readonly NoteTabViewModel _viewModel;
   readonly IFatalErrorHandler _fatalErrorHandler;

   /// <summary>Whether the web view was told to load the page of the note the tab shows, which it is the first time the tab is shown.</summary>
   bool _pageLoadStarted;

   /// <summary>The note whose page the web view shows, as it was last told to load or reported showing, so that it isn't loaded again for nothing.</summary>
   NoteId? _noteThePageShows;

   [Obsolete("For XAML designer/previewer only")]
#pragma warning disable CS8618 // The XAML designer needs a parameterless constructor, and only the real constructor has the tab's view model.
   public NoteTabView() => InitializeComponent();
#pragma warning restore CS8618

   /// <param name="tab">The tab the view shows.</param>
   /// <param name="fatalErrorHandler">Reports a failure of the web view to show a page, which no caller waits for.</param>
   internal NoteTabView(NoteTabViewModel tab, IFatalErrorHandler fatalErrorHandler)
   {
      InitializeComponent();
      _fatalErrorHandler = fatalErrorHandler;
      DataContext = _viewModel = tab;
      NotePagePresenter.Present(tab);

      WebView.EnvironmentRequested += ConfigureWebView2Environment;
      WebView.AdapterCreated += (_, _) => FocusPageIfShown();
      WebView.NewWindowRequested += OpenNewWindowRequestInBackgroundTab;
      WebView.WebMessageReceived += OnPageMessage;
      tab.PageOutdated += () => Dispatcher.UIThread.Post(() => WebView.Refresh());
      tab.ShowsAnotherNote += ShowThePageOfTheShownNote;

      LoadPageTheFirstTimeTheTabIsShown();
   }

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(change.Property != IsVisibleProperty) return;

      LoadPageTheFirstTimeTheTabIsShown();
      FocusPageIfShown();
   }

   /// <summary>
   /// A tab opened in the background loads its page only when it's first shown,<br/>
   /// because the web view doesn't tell a hidden page that it's hidden, and the page would play its note's audio unseen.
   /// </summary>
   void LoadPageTheFirstTimeTheTabIsShown()
   {
      if(_pageLoadStarted || !IsVisible) return;

      _pageLoadStarted = true;
      LoadThePageOfTheShownNote();
   }

   /// <summary>
   /// Shows the page of the note the tab now shows, and gives it the keyboard, as following a link in a browser leaves it on the page. A card's page is<br/>
   /// loaded in the web view, unless it isn't loaded yet, since the tab wasn't shown yet, or the web view already shows it.
   /// </summary>
   void ShowThePageOfTheShownNote()
   {
      if(_pageLoadStarted && _viewModel.ShowsTheCardPage && !_viewModel.ShownNoteId.Equals(_noteThePageShows))
         LoadThePageOfTheShownNote();
      FocusPageIfShown();
   }

   /// <summary>
   /// Loads the shown note's card page in the web view: the first by navigating to it, each after that by replacing the page shown with it, so that the<br/>
   /// web view's history only ever holds the page shown.
   /// </summary>
   /// <remarks>
   /// The tab keeps the history. A web view with a history of its own went back through that on the browser's own Back, Alt+Left or the mouse's back<br/>
   /// button, which the page also reports to the tab, so the two went back each their own way, to different pages (Magnus, 2026-11-13). With nothing<br/>
   /// in its history, the browser's own Back and Forward do nothing.
   /// </remarks>
   void LoadThePageOfTheShownNote()
   {
      if(!_viewModel.ShowsTheCardPage) return;

      var isTheFirstPage = _noteThePageShows is null;
      _noteThePageShows = _viewModel.ShownNoteId;
      var pageUrl = _viewModel.ShownPageUrl;
      if(isTheFirstPage)
         WebView.Navigate(pageUrl);
      else
         _fatalErrorHandler.StartAndHandleExceptionsFrom(() => WebView.InvokeScript($"location.replace({JsonSerializer.Serialize(pageUrl.ToString())})"));
   }

   /// <summary>
   /// When a tab is shown, the keyboard goes to its page, so that the page's keys act on the note being shown.<br/>
   /// Otherwise the keyboard focus would stay in the web view of the tab that was hidden.
   /// </summary>
   /// <remarks>
   /// Posted so that it runs after the native web view is shown, which Avalonia does after rendering,<br/>
   /// and after a click on the tab's header has finished focusing the header.
   /// </remarks>
   void FocusPageIfShown()
   {
      if(IsVisible)
         FocusPage();
   }

   /// <summary>Gives the keyboard to the page, for its keys to act on the note being shown. See <see cref="FocusPageIfShown"/> for why it's posted.</summary>
   /// <remarks>
   /// A page in Avalonia gets it once the window has taken back Windows' keyboard focus from the web view: focusing an Avalonia control in code<br/>
   /// doesn't take it away from a web view (<see cref="NativeKeyboardFocus"/>).
   /// </remarks>
   public void FocusPage()
   {
      if(_viewModel.ShowsTheCardPage)
      {
         Dispatcher.UIThread.Post(() => WebView.Focus(), DispatcherPriority.Background);
         return;
      }

      NativeKeyboardFocus.MoveToWindowItself(WorkspaceWindow);
      NotePagePresenter.FocusThePage();
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
         case "showNote":
            _viewModel.ShowNote(NoteId.Parse(StringProperty("noteId")));
            break;
         case "goBack":
            WorkspaceWindow.TryRunWindowShortcut(WorkspaceWindow.GoBackKeyGesture);
            break;
         case "goForward":
            WorkspaceWindow.TryRunWindowShortcut(WorkspaceWindow.GoForwardKeyGesture);
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
      _noteThePageShows = CardServerUrl.NoteIdOfPage(pageUrl);
      _viewModel.PageShown(pageUrl);
   }

   /// <summary>Opens the shown note's right-click menu, the same one Anki shows for it, where the mouse cursor is.</summary>
   async Task ShowNoteContextMenuAsync(string selection)
   {
      var clipboard = await (WorkspaceWindow.Clipboard ?? throw new InvalidOperationException("The workspace window has no clipboard.")).TryGetTextAsync() ?? string.Empty;
      PopupMenuWindow.ShowAt(MouseCursor.ScreenPosition(), _viewModel.NoteContextMenuSpec(selection, clipboard));
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
      if(e is not WindowsWebView2EnvironmentRequestedEventArgs webView2) return;

      webView2.UserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JAStudio", "WorkspaceWebView2");
      webView2.AdditionalBrowserArguments = "--autoplay-policy=no-user-gesture-required";
   }

   /// <summary>
   /// Middle-clicking a link opens it in a new tab in the background, as in a browser.<br/>
   /// Left to itself, the web view would open a bare web view window, without JAStudio's menus.
   /// </summary>
   void OpenNewWindowRequestInBackgroundTab(object? sender, WebViewNewWindowRequestedEventArgs e)
   {
      e.Handled = true;
      _viewModel.OpenInBackgroundTab(e.Request ?? throw new InvalidOperationException("The page requested a new window without a URL."));
   }
}
