using Avalonia.Controls;

namespace JAStudio.Mcp.Specifications.Tools.Screen;

/// <summary>
/// A window the specifications of the screen-tools drive: a check box, a text box, a button that counts its clicks, and a combo box, whose<br/>
/// drop-down list is a popup. Made and used on the UI thread.
/// </summary>
public sealed class WindowToDrive : Window
{
   public const string TheTitle = "A window to drive";

   WindowToDrive(string title)
   {
      Title = title;
      Width = 400;
      Height = 300;
      Button.Click += (_, _) => Clicks++;
      Content = new StackPanel { Children = { CheckBox, TextBox, Button, ComboBox } };
   }

   public CheckBox CheckBox { get; } = new() { Content = "A check box" };
   public TextBox TextBox { get; } = new() { Width = 200 };
   public Button Button { get; } = new() { Content = "Count the clicks" };
   public ComboBox ComboBox { get; } = new() { ItemsSource = new[] { "First", "Second" }, Width = 200 };

   /// <summary>How many times <see cref="Button"/> has been clicked.</summary>
   public int Clicks { get; private set; }

   /// <summary>A window to drive, titled <paramref name="title"/>, shown at the top left of the screen.</summary>
   public static WindowToDrive Shown(string title = TheTitle)
   {
      var window = new WindowToDrive(title);
      window.Show();
      return window;
   }
}
