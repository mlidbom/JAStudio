using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Compze.Underscore;

namespace JAStudio.UI.Utils;

static class WindowPositioner
{
   [DllImport("user32.dll")]
   [return: MarshalAs(UnmanagedType.Bool)]
   static extern bool GetCursorPos(out Point lpPoint);

   [StructLayout(LayoutKind.Sequential)]
   struct Point
   {
      public int X;
      public int Y;
   }

   extension<TWindow>(TWindow window) where TWindow : Window
   {
      internal TWindow ShowNearCursor() => window._mutate(it => it.PositionNearCursor().Show());

      TWindow PositionNearCursor()
      {
         window.WindowStartupLocation = WindowStartupLocation.Manual;

         if(!GetCursorPos(out var cursor))
            return window;

         window.Position = new PixelPoint(cursor.X, cursor.Y);

         window.Opened += OnOpened;

         void OnOpened(object? sender, EventArgs e)
         {
            window.Opened -= OnOpened;
            window.ApplyPosition(cursor.X, cursor.Y);
         }

         return window;
      }

      internal void RepositionNearCursor()
      {
         if(!GetCursorPos(out var cursor))
            return;

         window.ApplyPosition(cursor.X, cursor.Y);
      }

      void ApplyPosition(int cursorX, int cursorY)
      {
         var scaling = window.GetScalingForScreen(cursorX, cursorY);

         // Window dimensions in physical (device) pixels.
         // Use Bounds if available (window already rendered), otherwise fall back to Width/Height properties.
         var logicalWidth = window.Bounds.Width > 0 ? window.Bounds.Width : window.Width;
         var logicalHeight = window.Bounds.Height > 0 ? window.Bounds.Height : window.Height;
         var windowWidthPx = (int)(logicalWidth * scaling);
         var windowHeightPx = (int)(logicalHeight * scaling);

         // Center the window on the cursor
         var x = cursorX - windowWidthPx / 2;
         var y = cursorY - windowHeightPx / 2;

         // Clamp to the working area of the screen the cursor is on
         var workingArea = window.GetWorkingArea(cursorX, cursorY);
         if(workingArea.HasValue)
         {
            var wa = workingArea.Value;
            x = Math.Clamp(x, wa.X, Math.Max(wa.X, wa.X + wa.Width - windowWidthPx));
            y = Math.Clamp(y, wa.Y, Math.Max(wa.Y, wa.Y + wa.Height - windowHeightPx));
         }

         window.Position = new PixelPoint(x, y);
      }

      PixelRect? GetWorkingArea(int cursorX, int cursorY)
      {
         var cursorPoint = new PixelPoint(cursorX, cursorY);

         foreach(var screen in window.Screens.All)
         {
            if(screen.Bounds.Contains(cursorPoint))
               return screen.WorkingArea;
         }

         return window.Screens.Primary?.WorkingArea;
      }

      double GetScalingForScreen(int cursorX, int cursorY)
      {
         var cursorPoint = new PixelPoint(cursorX, cursorY);

         foreach(var screen in window.Screens.All)
         {
            if(screen.Bounds.Contains(cursorPoint))
               return screen.Scaling;
         }

         return window.Screens.Primary?.Scaling ?? 1.0;
      }
   }
}
