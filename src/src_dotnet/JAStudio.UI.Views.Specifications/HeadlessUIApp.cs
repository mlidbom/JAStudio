using Avalonia;
using Avalonia.Headless;

namespace JAStudio.UI.Views.Specifications;

/// <summary>
/// JAStudio's Avalonia application on Avalonia's headless platform: windows are laid out and drawn with Skia as on a screen, but on no screen,<br/>
/// so a specification can open them, press their buttons and render them to an image, without touching the desktop.
/// </summary>
static class HeadlessUIApp
{
   /// <summary>Found by name by <see cref="HeadlessUnitTestSession.StartNew(System.Type,AvaloniaTestIsolationLevel)"/>, as Avalonia's designer finds an application's.</summary>
   public static AppBuilder BuildAvaloniaApp() =>
      AppBuilder.Configure<UIApp>()
                .UseSkia()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
