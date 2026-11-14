using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace JAStudio.UI.Utils;

/// <summary>
/// Moves the Windows keyboard focus back to an Avalonia window from a native control embedded in it, such as a <see cref="NativeWebView"/>.
/// </summary>
/// <remarks>
/// Avalonia does this itself when the window is clicked, but not when code focuses one of its controls.<br/>
/// So focusing an Avalonia control in code, while a web view in the same window has the keyboard, leaves the keys going to the web view.<br/>
/// The WebView2 adapter of <see cref="NativeWebView"/> doesn't give the keyboard back when it loses Avalonia's focus.
/// </remarks>
static class NativeKeyboardFocus
{
   [DllImport("user32.dll", SetLastError = true)]
   static extern IntPtr SetFocus(IntPtr hWnd);

   /// <summary>Gives the Windows keyboard focus to <paramref name="window"/>'s own native window, so that Avalonia's focus decides where the keys go.</summary>
   /// <remarks>
   /// Elsewhere it does nothing: whether the macOS and Linux web views keep the keyboard the same way isn't known yet (<c>dev_docs/multiplatform.md</c>).
   /// </remarks>
   internal static void MoveToWindowItself(TopLevel window)
   {
      if(!OperatingSystem.IsWindows()) return;

      var windowHandle = window.TryGetPlatformHandle() ?? throw new InvalidOperationException("The window has no native window yet.");
      Marshal.SetLastPInvokeError(0);
      if(SetFocus(windowHandle.Handle) == IntPtr.Zero && Marshal.GetLastPInvokeError() != 0)
         throw new Win32Exception(Marshal.GetLastPInvokeError());
   }
}
