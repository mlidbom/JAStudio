using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace JAStudio.UI.Utils;

/// <summary>
/// Simple input dialog for getting text from the user.
/// </summary>
public class InputDialog : Window
{
   readonly TextBox _textBox;
   readonly TaskCompletionSource<string?> _resultSource;

   public InputDialog(string prompt, string initialValue = "")
   {
      _resultSource = new TaskCompletionSource<string?>();

      Width = 400;
      Height = 150;
      Title = "Input";
      CanResize = false;
      WindowStartupLocation = WindowStartupLocation.Manual;

      var stack = new StackPanel
                  {
                     Margin = new Thickness(10),
                     Spacing = 10
                  };

      // Prompt label
      stack.Children.Add(new TextBlock
                         {
                            Text = prompt,
                            FontWeight = FontWeight.Bold
                         });

      // Text input
      _textBox = new TextBox
                 {
                    Text = initialValue,
                    PlaceholderText = "Enter text..."
                 };
      _textBox.KeyDown += OnTextBoxKeyDown;
      stack.Children.Add(_textBox);

      // Buttons
      var buttonPanel = new StackPanel
                        {
                           Orientation = Orientation.Horizontal,
                           HorizontalAlignment = HorizontalAlignment.Right,
                           Spacing = 10
                        };

      var okButton = new Button
                     {
                        Content = "OK",
                        Width = 80,
                        IsDefault = true
                     };
      okButton.Click += OnOkClicked;
      buttonPanel.Children.Add(okButton);

      var cancelButton = new Button
                         {
                            Content = "Cancel",
                            Width = 80,
                            IsCancel = true
                         };
      cancelButton.Click += OnCancelClicked;
      buttonPanel.Children.Add(cancelButton);

      stack.Children.Add(buttonPanel);

      Content = stack;

      // Focus textbox when shown
      Opened += (_, _) => _textBox.Focus();
   }

   void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
   {
      if(e.Key == Key.Enter)
      {
         CompleteWithResult(_textBox.Text);
         e.Handled = true;
      } else if(e.Key == Key.Escape)
      {
         CompleteWithResult(null);
         e.Handled = true;
      }
   }

   void OnOkClicked(object? sender, RoutedEventArgs e)
   {
      CompleteWithResult(_textBox.Text);
   }

   void OnCancelClicked(object? sender, RoutedEventArgs e)
   {
      CompleteWithResult(null);
   }

   void CompleteWithResult(string? result)
   {
      _resultSource.TrySetResult(result);
      Close();
   }

   /// <summary>
   /// Shows the dialog and returns the entered text (or null if cancelled).
   /// </summary>
   public static string? ShowAsync(string prompt, string initialValue = "") =>
      new InputDialog(prompt, initialValue).ShowNearCursor()._resultSource.Task.Result;

   /// <summary>
   /// Shows the dialog synchronously on the UI thread and returns the entered text.
   /// If cancelled or empty, returns the clipboard content.
   /// </summary>
   public static string GetInputOrClipboard(string prompt)
   {
      string? result = null;

      Dispatcher.UIThread.Invoke(() =>
      {
         result = ShowAsync(prompt, GetClipboardText());
      });

      return result ?? GetClipboardText();
   }

   static string GetClipboardText()
   {
      var topLevel = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                        ? desktop.MainWindow
                        : null;

      if(topLevel?.Clipboard != null)
      {
         var task = topLevel.Clipboard.TryGetTextAsync();
         task.Wait();
         return task.Result ?? string.Empty;
      }

      throw new Exception("Could not get clipboard text");
   }
}
