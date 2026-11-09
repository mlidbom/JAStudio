using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// The view of one workspace tab (<see cref="NoteTabViewModel"/>) where there is no screen to show a native web view on, as in the headless<br/>
/// JAStudio: the tab's page as an image, rendered by a browser (<see cref="IWebPageImages"/>), where <see cref="NoteTabView"/> shows the page<br/>
/// in a web view. Only to look at: the page can't be clicked, and doesn't navigate.
/// </summary>
/// <remarks>
/// A <see cref="NativeWebView"/> needs a native window: on Avalonia's headless platform, creating one fails, and the workspace window with it,<br/>
/// since the workspace opens the tabs it had when it was last left.
/// </remarks>
public partial class NoteTabImageView : UserControl
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
      tab.PageOutdated += () => Dispatcher.UIThread.Post(ShowThePageIfTheTabIsShown);
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
