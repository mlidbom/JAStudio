using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace JAStudio.UI.Utils;

/// <summary>
/// A Yes/No confirmation dialog: centred on the screen (<see cref="ShowAsync"/>), or modal over a window (<see cref="ShowOverAsync"/>).<br/>
/// Enter picks Yes, and Esc, No or closing the dialog picks No.
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
      Opened += (_, _) =>
      {
         NativeKeyboardFocus.MoveToWindowItself(this);
         yesButton.Focus();
      };
      Closed += (_, _) => _resultSource.TrySetResult(false);
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

   /// <summary>Shows the dialog centred on the screen, as from a menu outside JAStudio's windows, and returns whether Yes was picked.</summary>
   public static Task<bool> ShowAsync(string title, string message)
   {
      var dialog = new ConfirmDialog(title, message) { WindowStartupLocation = WindowStartupLocation.CenterScreen };
      dialog.Show();
      return dialog._resultSource.Task;
   }

   /// <summary>Shows the dialog modally, centered over <paramref name="owner"/>, and returns whether Yes was picked.</summary>
   public static async Task<bool> ShowOverAsync(Window owner, string title, string message)
   {
      var dialog = new ConfirmDialog(title, message) { WindowStartupLocation = WindowStartupLocation.CenterOwner, ShowInTaskbar = false };
      await dialog.ShowDialog(owner);
      return await dialog._resultSource.Task;
   }
}
