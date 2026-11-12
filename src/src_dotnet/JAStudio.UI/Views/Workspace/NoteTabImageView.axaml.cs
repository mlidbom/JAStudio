using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// The view of one workspace tab (<see cref="NoteTabViewModel"/>) where there is no screen to show a native web view on, as in the headless<br/>
/// JAStudio: the tab's page as an image, rendered by a browser (<see cref="IWebPageImages"/>), where <see cref="NoteTabView"/> shows the page<br/>
/// in a web view. The page can't be clicked, and doesn't navigate, but its keys act on the note shown as the web view's do: the note view<br/>
/// shortcuts, such as those that toggle the sentence breakdown's settings.
/// </summary>
/// <remarks>
/// A <see cref="NativeWebView"/> needs a native window: on Avalonia's headless platform, creating one fails, and the workspace window with it,<br/>
/// since the workspace opens the tabs it had when it was last left.
/// </remarks>
public partial class NoteTabImageView : UserControl, INoteTabView
{
   readonly NoteTabViewModel _tab;
   readonly IWebPageImages _webPageImages;

   /// <summary>The image <see cref="Page"/> shows, disposed when another replaces it.</summary>
   Bitmap? _imageOfThePage;

   [Obsolete("For XAML designer/previewer only")]
#pragma warning disable CS8618 // The XAML designer needs a parameterless constructor, and only the real constructor has the tab's view model.
   public NoteTabImageView() => InitializeComponent();
#pragma warning restore CS8618

   internal NoteTabImageView(NoteTabViewModel tab, IWebPageImages webPageImages)
   {
      InitializeComponent();
      DataContext = _tab = tab;
      _webPageImages = webPageImages;

      PageArea.SizeChanged += (_, _) => ShowThePageIfTheTabIsShown();
      PageArea.KeyDown += (_, keyPressed) => RunTheNoteViewShortcutOf(keyPressed);
      tab.PageOutdated += () => Dispatcher.UIThread.Post(ShowThePageIfTheTabIsShown);
   }

   protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnAttachedToVisualTree(e);
      FocusThePageIfShown();
   }

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(change.Property == IsVisibleProperty) FocusThePageIfShown();
   }

   /// <summary>
   /// When the tab is shown, the keyboard goes to its page, as <see cref="NoteTabView"/> gives it to its web view: as the tab opens, for one<br/>
   /// opened shown, and as it's shown again.
   /// </summary>
   void FocusThePageIfShown()
   {
      if(IsVisible) FocusPage();
   }

   /// <summary>Posted, so that it runs after a click on the tab's header has finished focusing the header.</summary>
   public void FocusPage() => Dispatcher.UIThread.Post(() => PageArea.Focus(), DispatcherPriority.Background);

   /// <summary>
   /// Runs the note view shortcut of the key pressed on the page, if any, as <c>card.js</c> has the web view's page report a key to<br/>
   /// <see cref="NoteTabView"/>. The workspace window's own shortcuts need nothing here: its key bindings take them from any control in it.
   /// </summary>
   void RunTheNoteViewShortcutOf(KeyEventArgs keyPressed)
   {
      if(keyPressed.KeySymbol is not {} keySymbol) return;

      _tab.KeyPressed(KeyGestureAsANoteViewShortcutWritesIt());

      return;

      string KeyGestureAsANoteViewShortcutWritesIt() =>
         (keyPressed.KeyModifiers.HasFlag(KeyModifiers.Control) ? "Ctrl+" : "")
       + (keyPressed.KeyModifiers.HasFlag(KeyModifiers.Shift) ? "Shift+" : "")
       + (keyPressed.KeyModifiers.HasFlag(KeyModifiers.Alt) ? "Alt+" : "")
       + keySymbol.ToLowerInvariant();
   }

   /// <summary>
   /// Shows an image of the tab's page as it is now, as large as the area the page has. A hidden tab's page isn't rendered: a tab opened in the<br/>
   /// background has its page rendered once it's shown, as <see cref="NoteTabView"/> loads it.
   /// </summary>
   void ShowThePageIfTheTabIsShown()
   {
      var viewport = new ViewportSize((int)PageArea.Bounds.Width, (int)PageArea.Bounds.Height);
      if(!IsVisible || viewport.Width == 0 || viewport.Height == 0) return;

      using var png = new MemoryStream(_webPageImages.OfTheViewport(_tab.FirstPageUrl, viewport).Png);
      var imageReplaced = _imageOfThePage;
      Page.Source = _imageOfThePage = new Bitmap(png);
      imageReplaced?.Dispose();
   }
}
