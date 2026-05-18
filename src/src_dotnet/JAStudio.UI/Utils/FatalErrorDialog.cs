using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Compze.Underscore;

namespace JAStudio.UI.Utils;

/// <summary>
/// Simple dialog shown when a background task crashes.
/// Informs the user that application state may be corrupted
/// and they should restart Anki and check logs.
/// </summary>
class FatalErrorDialog : Window
{
   FatalErrorDialog(Exception ex)
   {
      Title = "Unexpected Error";
      Width = 500;
      MinHeight = 200;
      SizeToContent = SizeToContent.Height;
      CanResize = false;
      WindowStartupLocation = WindowStartupLocation.CenterScreen;

      Content = new StackPanel
                {
                   Margin = new Thickness(20),
                   Spacing = 14,
                   Children =
                   {
                      new TextBlock
                      {
                         Text = "An unexpected error occurred",
                         FontWeight = FontWeight.Bold,
                         FontSize = 16
                      },
                      new TextBlock
                      {
                         Text = "Application state can no longer be trusted. Please restart Anki.\nCheck the logs for full details.",
                         TextWrapping = TextWrapping.Wrap
                      },
                      new TextBlock
                      {
                         Text = ex.GetType().Name + ": " + ex.Message,
                         TextWrapping = TextWrapping.Wrap,
                         Foreground = Brushes.DarkRed,
                         FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                         FontSize = 12
                      },
                      new Button
                      {
                         Content = "OK",
                         Width = 80,
                         HorizontalAlignment = HorizontalAlignment.Right,
                         IsDefault = true
                      }._mutate(it => it.Click += (_, _) => Close())
                   }
                };
   }

   /// <summary>
   /// Show a fatal error dialog. Must be called on the Avalonia UI thread.
   /// </summary>
   public static void Show(Exception ex)
   {
      var dialog = new FatalErrorDialog(ex);
      dialog.Show();
   }
}
