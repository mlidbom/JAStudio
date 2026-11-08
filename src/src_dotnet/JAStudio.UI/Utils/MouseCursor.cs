using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;

namespace JAStudio.UI.Utils;

/// <summary>The mouse cursor's position on the screen, read from Windows.</summary>
/// <remarks>
/// Windows only. The one use left is the note page's right-click menu in a workspace tab: Avalonia doesn't see the pointer over the page's native<br/>
/// web view. That menu is to be replaced by a UI on the page itself, which knows what was clicked: see <c>dev_docs/multiplatform.md</c>.
/// </remarks>
static class MouseCursor
{
   [DllImport("user32.dll", SetLastError = true)]
   [return: MarshalAs(UnmanagedType.Bool)]
   static extern bool GetCursorPos(out Point lpPoint);

   [StructLayout(LayoutKind.Sequential)]
   struct Point
   {
      public int X;
      public int Y;
   }

   /// <summary>The mouse cursor's position on the screen, in the screen's own pixels.</summary>
   internal static PixelPoint ScreenPosition() =>
      GetCursorPos(out var cursor) ? new PixelPoint(cursor.X, cursor.Y) : throw new Win32Exception(Marshal.GetLastWin32Error());
}
