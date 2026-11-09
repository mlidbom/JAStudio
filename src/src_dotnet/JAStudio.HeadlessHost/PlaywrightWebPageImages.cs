using System;
using System.Threading.Tasks;
using JAStudio.Core.UI;
using JAStudio.Core.UI.Web;
using Microsoft.Playwright;
using ViewportSize = JAStudio.Core.UI.Web.ViewportSize;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio's images of web pages: each page loaded in a headless browser, driven by Playwright, and captured once it has loaded<br/>
/// and the network has gone quiet. The browser is the installed Microsoft Edge on Windows, and Google Chrome elsewhere, which Playwright finds<br/>
/// itself, so nothing is downloaded. It starts with the first page, and runs muted, since a card page plays its note's audio.
/// </summary>
/// <remarks>
/// Each page gets a browser context of its own, so pages share no state. Playwright's API is asynchronous; each image is made on the thread<br/>
/// pool, so that a caller on the UI thread, such as a workspace tab, waits for it without Playwright's continuations waiting for the UI thread.
/// </remarks>
sealed class PlaywrightWebPageImages : IWebPageImages, IDisposable
{
   readonly Lazy<Task<(IPlaywright Playwright, IBrowser Browser)>> _browser = new(LaunchTheBrowserAsync);

   public PngImage OfTheViewport(Uri page, ViewportSize viewport) => ImageOf(page, viewport, wholePage: false);

   public PngImage OfTheWholePage(Uri page, ViewportSize viewport) => ImageOf(page, viewport, wholePage: true);

   PngImage ImageOf(Uri page, ViewportSize viewport, bool wholePage) => Task.Run(async () =>
   {
      var (_, browser) = await _browser.Value;
      await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { ViewportSize = new Microsoft.Playwright.ViewportSize { Width = viewport.Width, Height = viewport.Height } });
      var browserPage = await context.NewPageAsync();
      await browserPage.GotoAsync(page.ToString(), new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
      return PngImage.Of(await browserPage.ScreenshotAsync(new PageScreenshotOptions { FullPage = wholePage, Type = ScreenshotType.Png }));
   }).GetAwaiter().GetResult();

   static async Task<(IPlaywright, IBrowser)> LaunchTheBrowserAsync()
   {
      var playwright = await Playwright.CreateAsync();
      var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
      {
         Channel = OperatingSystem.IsWindows() ? "msedge" : "chrome",
         Headless = true,
         Args = ["--mute-audio"]
      });
      return (playwright, browser);
   }

   /// <summary>Closes the browser, if a page was rendered and it started.</summary>
   public void Dispose()
   {
      if(!_browser.IsValueCreated) return;

      Task.Run(async () =>
      {
         var (playwright, browser) = await _browser.Value;
         await browser.CloseAsync();
         playwright.Dispose();
      }).GetAwaiter().GetResult();
   }
}
