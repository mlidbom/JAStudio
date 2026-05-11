using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace JAStudio.UI.Utils;

/// <summary>
/// Simple Yes/No confirmation dialog that appears near the cursor.
/// </summary>
class ConfirmDialog : Window
{
   readonly TaskCompletionSource<bool> _resultSource = new();

   ConfirmDialog(string title, string message)
   {
      Title = title;
      Width = 400;
      CanResize = false;
      SizeToContent = SizeToContent.Height;
      WindowStartupLocation = WindowStartupLocation.Manual;

      var stack = new StackPanel
                  {
                     Margin = new Thickness(16),
                     Spacing = 16
                  };

      stack.Children.Add(new TextBlock
                         {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            MaxWidth = 368
                         });

      var buttonPanel = new StackPanel
                        {
                           Orientation = Orientation.Horizontal,
                           HorizontalAlignment = HorizontalAlignment.Right,
                           Spacing = 8
                        };

      var yesButton = new Button { Content = "Yes", Width = 80, IsDefault = true };
      yesButton.Click += (_, _) => Complete(true);
      buttonPanel.Children.Add(yesButton);

      var noButton = new Button { Content = "No", Width = 80, IsCancel = true };
      noButton.Click += (_, _) => Complete(false);
      buttonPanel.Children.Add(noButton);

      stack.Children.Add(buttonPanel);
      Content = stack;

      KeyDown += OnKeyDown;
      Opened += (_, _) => yesButton.Focus();
   }

   void OnKeyDown(object? sender, KeyEventArgs e)
   {
      if(e.Key == Key.Escape)
      {
         Complete(false);
         e.Handled = true;
      }
   }

   void Complete(bool result)
   {
      _resultSource.TrySetResult(result);
      Close();
   }

   public static Task<bool> ShowAsync(string title, string message)
   {
      var dialog = new ConfirmDialog(title, message);
      dialog.ShowNearCursor();
      return dialog._resultSource.Task;
   }
}
