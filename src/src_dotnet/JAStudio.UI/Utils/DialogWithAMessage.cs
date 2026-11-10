using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace JAStudio.UI.Utils;

/// <summary>
/// A small dialog that shows a message above a row of buttons, and gives back the <typeparamref name="TResult"/> of the button pressed: the dialog<br/>
/// <see cref="MessageDialog"/>, <see cref="ConfirmDialog"/> and <see cref="ChoiceDialog{TChoice}"/> show to tell or ask a person something.<br/>
/// Enter presses the first button, and Esc the last, the cancel button, which closing the dialog presses too.
/// </summary>
sealed class DialogWithAMessage<TResult> : Window
{
   const double SpaceAroundAndBetween = 16;

   readonly TaskCompletionSource<TResult> _resultSource = new();

   /// <param name="title">The dialog's title.</param>
   /// <param name="message">What the dialog tells or asks, wrapped to <paramref name="width"/>.</param>
   /// <param name="width">The dialog's width.</param>
   /// <param name="buttons">The dialog's buttons from left to right, each with what pressing it gives back. The first is the default button, the last the cancel button.</param>
   DialogWithAMessage(string title, string message, double width, IReadOnlyList<(string Label, TResult Result)> buttons)
   {
      Title = title;
      Width = width;
      CanResize = false;
      SizeToContent = SizeToContent.Height;

      List<Button> buttonRow = [..buttons.Select(it => ButtonGivingBack(it.Label, it.Result))];
      buttonRow[0].IsDefault = true;
      buttonRow[^1].IsCancel = true;
      var resultOfCancelling = buttons[^1].Result;

      var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Spacing = 8 };
      buttonPanel.Children.AddRange(buttonRow);

      var content = new StackPanel { Margin = new Thickness(SpaceAroundAndBetween), Spacing = SpaceAroundAndBetween };
      content.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, MaxWidth = width - 2 * SpaceAroundAndBetween });
      content.Children.Add(buttonPanel);
      Content = content;

      KeyDown += (_, e) =>
      {
         if(e.Key != Key.Escape) return;

         GiveBack(resultOfCancelling);
         e.Handled = true;
      };
      Opened += (_, _) =>
      {
         NativeKeyboardFocus.MoveToWindowItself(this);
         buttonRow[0].Focus();
      };
      Closed += (_, _) => _resultSource.TrySetResult(resultOfCancelling);
      return;

      Button ButtonGivingBack(string label, TResult result)
      {
         var button = new Button { Content = label, MinWidth = 80, HorizontalContentAlignment = HorizontalAlignment.Center };
         button.Click += (_, _) => GiveBack(result);
         return button;
      }
   }

   void GiveBack(TResult result)
   {
      _resultSource.TrySetResult(result);
      Close();
   }

   /// <summary>Shows the dialog centred on the screen, as from a menu outside JAStudio's windows, and gives back what the button pressed gives.</summary>
   /// <inheritdoc cref="DialogWithAMessage{TResult}(string, string, double, IReadOnlyList{ValueTuple{string, TResult}})"/>
   public static Task<TResult> ShowCentredOnTheScreenAsync(string title, string message, double width, IReadOnlyList<(string Label, TResult Result)> buttons)
   {
      var dialog = new DialogWithAMessage<TResult>(title, message, width, buttons) { WindowStartupLocation = WindowStartupLocation.CenterScreen };
      dialog.Show();
      return dialog._resultSource.Task;
   }

   /// <summary>Shows the dialog modally, centred over <paramref name="owner"/>, and gives back what the button pressed gives.</summary>
   /// <inheritdoc cref="DialogWithAMessage{TResult}(string, string, double, IReadOnlyList{ValueTuple{string, TResult}})"/>
   public static async Task<TResult> ShowModallyOverAsync(Window owner, string title, string message, double width, IReadOnlyList<(string Label, TResult Result)> buttons)
   {
      var dialog = OverTheOwner(title, message, width, buttons);
      await dialog.ShowDialog(owner);
      return await dialog._resultSource.Task;
   }

   /// <summary>
   /// Shows the dialog modally, centred over <paramref name="owner"/>, and returns at once, rather than when a button is pressed: for a dialog whose result<br/>
   /// no one needs, such as one that only tells something.
   /// </summary>
   /// <inheritdoc cref="DialogWithAMessage{TResult}(string, string, double, IReadOnlyList{ValueTuple{string, TResult}})"/>
   public static void ShowModallyOver(Window owner, string title, string message, double width, IReadOnlyList<(string Label, TResult Result)> buttons) =>
      _ = OverTheOwner(title, message, width, buttons).ShowDialog(owner); // ShowDialog shows the dialog before it returns; its task only completes when the dialog closes

   static DialogWithAMessage<TResult> OverTheOwner(string title, string message, double width, IReadOnlyList<(string Label, TResult Result)> buttons) =>
      new(title, message, width, buttons) { WindowStartupLocation = WindowStartupLocation.CenterOwner, ShowInTaskbar = false };
}
