using System;
using System.Collections.Generic;
using JAStudio.Core.UI;
using JAStudio.Core.UI.Web;

namespace JAStudio.Mcp.Specifications;

/// <summary>
/// Images of web pages that records which pages it was asked for, and gives the same tiny image for each: the specifications' stand-in for the<br/>
/// headless JAStudio's browser, which may not be installed where they run, as on CI.
/// </summary>
class WebPageImagesThatRecordThePagesAsked : IWebPageImages
{
   /// <summary>A PNG of a single transparent pixel.</summary>
   static readonly PngImage TheImageGiven = PngImage.Of(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg=="));

   readonly List<PageAsked> _pagesAsked = [];

   /// <summary>Every page asked for, in the order asked.</summary>
   public IReadOnlyList<PageAsked> PagesAsked => _pagesAsked;

   public PngImage OfTheViewport(Uri page, ViewportSize viewport) => Recorded(new PageAsked(page, viewport, WholePage: false));

   public PngImage OfTheWholePage(Uri page, ViewportSize viewport) => Recorded(new PageAsked(page, viewport, WholePage: true));

   PngImage Recorded(PageAsked pageAsked)
   {
      _pagesAsked.Add(pageAsked);
      return TheImageGiven;
   }

   /// <summary>A page an image was asked for, with the viewport, and whether the image was to be of the whole page or of what the viewport shows.</summary>
   public record PageAsked(Uri Page, ViewportSize Viewport, bool WholePage);
}
