using System.Linq;
using Avalonia.Controls;

namespace JAStudio.UI.Utils;

/// <summary>Questions about the windows a window owns, such as a modal dialog shown over it with <see cref="Window.ShowDialog(Window)"/>.</summary>
static class OwnedWindowsExtensions
{
   /// <summary>
   /// Whether a window <paramref name="window"/> owns is open over it, such as a modal dialog.<br/>
   /// Code that would give <paramref name="window"/> the keyboard holds back then, since it would take the keyboard from that dialog.
   /// </summary>
   internal static bool HasAWindowOpenOverIt(this Window window) => window.OwnedWindows.Any(it => it.IsVisible);
}
