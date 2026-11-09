using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace JAStudio.UI.Utils;

/// <summary>
/// A dialog that asks a person to choose one of a few <typeparamref name="TChoice"/>s, a button each, or to cancel: centred on the screen<br/>
/// (<see cref="ShowAsync"/>). Enter picks the first choice, and Esc, Cancel or closing the dialog cancels.
/// </summary>
class ChoiceDialog<TChoice> : Window where TChoice : struct
{
   readonly TaskCompletionSource<TChoice?> _resultSource = new();

   ChoiceDialog(string title, string message, IReadOnlyList<(string Label, TChoice Choice)> choices)
   {
      //refactor: ConfirmDialog is this dialog with the choices Yes and No, built the same way. It should be one of these.
      Title = title;
      Width = 480;
      CanResize = false;
      SizeToContent = SizeToContent.Height;
      WindowStartupLocation = WindowStartupLocation.CenterScreen;

      var stack = new StackPanel { Margin = new Thickness(16), Spacing = 16 };
      stack.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, MaxWidth = 448 });

      var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
      var choiceButtons = new List<Button>();
      foreach(var (label, choice) in choices)
      {
         var button = new Button { Content = label, IsDefault = choiceButtons.Count == 0 };
         button.Click += (_, _) => Complete(choice);
         choiceButtons.Add(button);
         buttonPanel.Children.Add(button);
      }

      var cancelButton = new Button { Content = "Cancel", IsCancel = true };
      cancelButton.Click += (_, _) => Complete(null);
      buttonPanel.Children.Add(cancelButton);

      stack.Children.Add(buttonPanel);
      Content = stack;

      KeyDown += OnKeyDown;
      Opened += (_, _) =>
      {
         NativeKeyboardFocus.MoveToWindowItself(this);
         choiceButtons[0].Focus();
      };
      Closed += (_, _) => _resultSource.TrySetResult(null);
   }

   void OnKeyDown(object? sender, KeyEventArgs e)
   {
      if(e.Key != Key.Escape) return;

      Complete(null);
      e.Handled = true;
   }

   void Complete(TChoice? choice)
   {
      _resultSource.TrySetResult(choice);
      Close();
   }

   /// <summary>Shows the dialog centred on the screen, as from a menu outside JAStudio's windows, and returns the choice picked, or null when cancelled.</summary>
   public static Task<TChoice?> ShowAsync(string title, string message, IReadOnlyList<(string Label, TChoice Choice)> choices)
   {
      var dialog = new ChoiceDialog<TChoice>(title, message, choices);
      dialog.Show();
      return dialog._resultSource.Task;
   }
}
