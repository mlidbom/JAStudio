using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// The <see cref="HeadlessScreen"/> as text, for Claude to read and act on: windows and popups, each with the controls it shows, indented under<br/>
/// the control they're in, each control on a line of its own (<see cref="ControlDescription"/>).
/// </summary>
/// <remarks>
/// The controls are those of Avalonia's automation tree, the tree screen readers read, so each one listed is something a person sees or acts on:<br/>
/// a button, a text box, a list's item, a label. Controls that only lay others out, such as a panel or a border, aren't listed, nor are those<br/>
/// hidden, nor a label repeating the text of the control it's in, such as a button's. Each control is listed once.
/// </remarks>
/// <remarks>Used on the UI thread only.</remarks>
public sealed class ScreenDescription
{
   readonly HeadlessScreen _screen;
   readonly StringBuilder _text = new();

   /// <summary>
   /// The controls listed so far, each listed once: the automation tree can hold a control twice, as a combo box's items are its children there,<br/>
   /// as well as the children of the popup they're shown in.
   /// </summary>
   readonly HashSet<Control> _listed = [];

   ScreenDescription(HeadlessScreen screen) => _screen = screen;

   /// <summary><paramref name="topLevels"/> of <paramref name="screen"/>, in that order, described.</summary>
   public static string Of(HeadlessScreen screen, IEnumerable<TopLevel> topLevels)
   {
      var description = new ScreenDescription(screen);
      foreach(var topLevel in topLevels)
         description.Describe(topLevel);
      return description._text.ToString();
   }

   /// <summary>The line of a window or a popup: its ref, its title, where it is, and whether it has the keyboard.</summary>
   internal static string TopLevelLine(HeadlessScreen screen, TopLevel topLevel)
   {
      var kind = topLevel is Window window ? $"Window {Quoted(window.Title ?? "")}" : "Popup";
      var active = topLevel is Window { IsActive: true } ? ", active" : "";
      var focused = screen.WindowManager.ControlWithTheKeyboard() is {} withTheKeyboard && TopLevel.GetTopLevel(withTheKeyboard) == topLevel ? $", the keyboard on {screen.Refs.RefOf(withTheKeyboard)}" : "";
      return $"{screen.Refs.RefOf(topLevel)} {kind} {WhereItIs(HeadlessScreen.PlaceOf(topLevel))}{active}{focused}";
   }

   /// <summary><paramref name="text"/> in quotes, on one line: quotes and line breaks in it written as in C#.</summary>
   internal static string Quoted(string text) => $"\"{text.Replace("\\", @"\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n")}\"";

   /// <summary>Where <paramref name="place"/> is on the screen, as each line of the description ends: <c>at 400,310 180×24</c>, its top left corner, then its width and height.</summary>
   internal static string WhereItIs(PixelRect place) => $"at {place.X},{place.Y} {place.Width}×{place.Height}";

   void Describe(TopLevel topLevel)
   {
      _text.AppendLine(TopLevelLine(_screen, topLevel));
      DescribeTheControlsIn(ControlAutomationPeer.CreatePeerForElement(topLevel), indent: 1, textAbove: null);
   }

   /// <summary>
   /// Describes the controls in <paramref name="parent"/>, at <paramref name="indent"/>, and those in them, more indented. A label whose text is<br/>
   /// <paramref name="textAbove"/>, the text of the control listed it's in, isn't listed.
   /// </summary>
   void DescribeTheControlsIn(AutomationPeer parent, int indent, string? textAbove)
   {
      foreach(var peer in parent.GetChildren())
      {
         if(peer is not ControlAutomationPeer { Owner: var control }) continue;

         var described = new ControlDescription(peer, control);
         if(described.IsHidden || _listed.Contains(control)) continue;

         if(described.IsWorthAListing && !described.IsALabelRepeating(textAbove))
         {
            _text.Append(' ', indent * 2).AppendLine(described.Line(_screen.Refs));
            _listed.Add(control);
            DescribeTheControlsIn(peer, indent + 1, described.Text);
         }
         else
            DescribeTheControlsIn(peer, indent, textAbove);
      }
   }
}
