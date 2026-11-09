using System;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// Images of web pages, such as the card server's, as a browser shows them once they've loaded: how the headless JAStudio, which has no screen<br/>
/// to show a native web view on, shows a page, and how Claude looks at a card.
/// </summary>
/// <remarks>The headless JAStudio's renders them with a headless browser. JAStudio inside Anki has none: it shows pages in native web views.</remarks>
public interface IWebPageImages
{
   /// <summary>What a browser shows of the page at <paramref name="page"/> in a viewport of the size <paramref name="viewport"/>.</summary>
   PngImage OfTheViewport(Uri page, ViewportSize viewport);

   /// <summary>The whole of the page at <paramref name="page"/>, laid out for a viewport of the size <paramref name="viewport"/>, however tall it is.</summary>
   PngImage OfTheWholePage(Uri page, ViewportSize viewport);
}
