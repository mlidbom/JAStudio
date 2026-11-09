using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using JAStudio.Mcp.UI;
using JAStudio.Mcp.UI.Screen;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Screen;

/// <summary>
/// The screen-tools: they show Claude the headless JAStudio's screen (<see cref="HeadlessScreen"/>), every window and popup it has open, and act on<br/>
/// it as a person does, with the mouse and the keyboard, so that Claude can use JAStudio's UI on the real corpus: reproduce a bug, check a fix, or<br/>
/// try a window being built. Only a JAStudio with a headless UI offers them: in the live JAStudio, they'd act on Magnus's screen.
/// </summary>
/// <remarks>
/// The controls are named by refs (<see cref="ControlRef"/>), from <c>describe_the_screen</c>, rather than by where they are in an image: a ref<br/>
/// names the control exactly, however the image is scaled to be shown, and the description gives a control's text and state exactly too.
/// </remarks>
[McpServerToolType]
public class ScreenTools
{
   const string NothingIsOpen = "Nothing is open on the screen: render_the_workspace opens the workspace.";

   readonly HeadlessUI _headlessUI;
   readonly ScreenInput _input;

   public ScreenTools(HeadlessUI headlessUI)
   {
      _headlessUI = headlessUI;
      _input = new ScreenInput(headlessUI.Screen);
   }

   HeadlessScreen Screen => _headlessUI.Screen;

   [McpServerTool(Name = "render_the_screen", ReadOnly = true)]
   [Description("""
                Renders the headless JAStudio's screen: every window it has open, and every popup, such as an open menu or a drop-down's list, each drawn where it
                is, the ones in front over the ones behind, as a person would see them. The headless platform draws windows with no frame or title bar, so each window
                has an outline. The text lists the windows and popups from the front, with their refs, and the control that has the keyboard.
                """)]
   public IEnumerable<ContentBlock> RenderTheScreen() => TheScreenAfter("", withTheImage: true);

   [McpServerTool(Name = "describe_the_screen", ReadOnly = true)]
   [Description("""
                Describes the headless JAStudio's screen as text: every window and popup, from the front, with the controls it shows, indented under the control
                they're in. Each control has its ref, such as c12, which the tools that act on a control take, then its kind, its text, its state, such as checked,
                selected, expanded, disabled or has the keyboard, and where it is on the screen. The controls are those a person reads or acts on, from Avalonia's
                automation tree, which screen readers read: labels, buttons, text boxes, lists and their items, menus and their items, tabs. A control keeps its ref
                as long as it exists, so only controls that appeared since need a new description.
                """)]
   public string DescribeTheScreen([Description("A window's or a popup's ref, to describe only it. Every window and popup if none.")] string? window = null) =>
      _headlessUI.OnTheUIThread(() =>
      {
         var topLevels = window == null ? Screen.TopLevelsFromTheFront : [Screen.TopLevelWithRef(new ControlRef(window))];
         return topLevels.Count == 0 ? NothingIsOpen : ScreenDescription.Of(Screen, topLevels);
      });

   [McpServerTool(Name = "click_a_control")]
   [Description("""
                Clicks a control on the headless JAStudio's screen, in its middle, with the headless platform's mouse: it goes where a person's click would and does
                what it does, such as press a button, check a check box, put the keyboard in a text box, open a menu, or close a popup clicked outside of. Fails,
                saying why, when a click at the control's middle would reach something else, such as a window in front of it, or the edge of a list the control is
                scrolled out of. Then gives the screen as it is after the click, as render_the_screen does.
                """)]
   public IEnumerable<ContentBlock> ClickAControl(
      [Description("The control's ref, such as \"c12\", as describe_the_screen lists it.")] string control,
      [Description("The mouse button: \"left\", \"right\" or \"middle\".")] string button = "left",
      [Description("Whether to click twice, as a double click.")] bool twice = false,
      [Description("Modifier keys held while clicking, joined by +, such as \"Ctrl\" or \"Ctrl+Shift\". None if not given.")] string? keysHeld = null,
      [Description("Whether to give the screen's image afterwards, rather than only the text.")] bool withTheImage = true)
   {
      var mouseButton = MouseButtons.Named(button);
      var modifiersHeld = keysHeld == null ? RawInputModifiers.None : ModifierKeys.Named(keysHeld);
      var clicked = _headlessUI.Do(() =>
      {
         var controlClicked = Screen.Refs.ControlWithRef(new ControlRef(control));
         var naming = ControlDescription.Of(controlClicked).Naming(Screen.Refs);
         _input.Click(controlClicked, mouseButton, twice ? 2 : 1, modifiersHeld);
         return naming;
      });
      return TheScreenAfter($"{(twice ? "Double-clicked" : "Clicked")} {clicked}{(mouseButton == MouseButton.Left ? "" : $" with the {button} button")}{(keysHeld == null ? "" : $", holding {keysHeld}")}.", withTheImage);
   }

   [McpServerTool(Name = "type_text")]
   [Description("""
                Types text into the control that has the keyboard on the headless JAStudio's screen, as a keyboard or an input method types it, Japanese as well.
                It only types text: keys such as Enter, Tab or Escape, and shortcuts, are press_keys'. Then gives the screen as it is afterwards.
                """)]
   public IEnumerable<ContentBlock> TypeText(
      [Description("The text to type.")] string text,
      [Description("Whether to give the screen's image afterwards, rather than only the text.")] bool withTheImage = true)
   {
      var typedInto = _headlessUI.Do(() =>
      {
         var naming = Screen.WindowManager.ControlWithTheKeyboard() is {} control ? ControlDescription.Of(control).Naming(Screen.Refs) : "nothing, since no control has the keyboard";
         _input.Type(text);
         return naming;
      });
      return TheScreenAfter($"Typed \"{text}\" into {typedInto}.", withTheImage);
   }

   [McpServerTool(Name = "press_keys")]
   [Description("""
                Presses keys on the headless JAStudio's keyboard, one after another, each released before the next: keys such as Enter, Escape, Tab or the arrows,
                and shortcuts such as Ctrl+O. Each is given by its name, after those of the modifier keys held with it, joined by +, and keys one after another are
                separated by spaces: "Ctrl+Shift+S", or "Down Down Enter". The names are Avalonia's: letters and digits, Enter, Escape, Tab, Space, Back (the
                backspace), Delete, Up, Down, Left, Right, Home, End, PageUp, PageDown and F1 to F12, and the modifiers Ctrl, Shift, Alt and Meta. The keys go to the
                control that has the keyboard, in the active window, and a key without Ctrl, Alt or Meta held types what it types on a US keyboard, as a real one
                does: a letter into a text box, or into one of JAStudio's menus, which picks the item that letter is the key of. Text such as Japanese is
                type_text's. Then gives the screen as it is afterwards.
                """)]
   public IEnumerable<ContentBlock> PressKeys(
      [Description("The keys, such as \"Ctrl+O\" or \"Down Down Enter\".")] string keys,
      [Description("Whether to give the screen's image afterwards, rather than only the text.")] bool withTheImage = true)
   {
      var chords = KeyChord.SequenceIn(keys);
      _headlessUI.Do(() => _input.Press(chords));
      return TheScreenAfter($"Pressed {keys}.", withTheImage);
   }

   [McpServerTool(Name = "scroll_a_control")]
   [Description("""
                Turns the headless JAStudio's mouse wheel over a control, scrolling what it's in, such as a list, as a person scrolls: for what a list shows only once
                scrolled to. Then gives the screen as it is afterwards.
                """)]
   public IEnumerable<ContentBlock> ScrollAControl(
      [Description("The control's ref, such as \"c12\", as describe_the_screen lists it: the list itself, or anything in it.")] string control,
      [Description("How many notches to turn the wheel: down, or up when negative. A notch scrolls about three lines of a list.")] int notchesDown,
      [Description("Whether to give the screen's image afterwards, rather than only the text.")] bool withTheImage = true)
   {
      var scrolled = _headlessUI.Do(() =>
      {
         var controlScrolled = Screen.Refs.ControlWithRef(new ControlRef(control));
         var naming = ControlDescription.Of(controlScrolled).Naming(Screen.Refs);
         _input.Scroll(controlScrolled, notchesDown);
         return naming;
      });
      return TheScreenAfter($"Scrolled {(notchesDown < 0 ? "up" : "down")} {Math.Abs(notchesDown)} notches over {scrolled}.", withTheImage);
   }

   [McpServerTool(Name = "close_a_window")]
   [Description("""
                Closes a window of the headless JAStudio's screen as its title bar's close button does, which the headless platform draws no title bar for. The window
                may ask first, or only hide, as the workspace does. Then gives the screen as it is afterwards.
                """)]
   public IEnumerable<ContentBlock> CloseAWindow(
      [Description("The window's ref, as render_the_screen or describe_the_screen lists it.")] string window,
      [Description("Whether to give the screen's image afterwards, rather than only the text.")] bool withTheImage = true)
   {
      var closed = _headlessUI.Do(() =>
      {
         var windowToClose = Screen.TopLevelWithRef(new ControlRef(window)) as Window ?? throw new ArgumentException($"{window} is a popup, not a window: a click outside of it, or Escape, closes it.", nameof(window));
         var naming = ControlDescription.Of(windowToClose).Naming(Screen.Refs);
         windowToClose.Close();
         return naming;
      });
      return TheScreenAfter($"Closed {closed}.", withTheImage);
   }

   /// <summary>The screen as it is: <paramref name="whatWasDone"/>, the windows and popups from the front, and, <paramref name="withTheImage"/>, its image.</summary>
   IEnumerable<ContentBlock> TheScreenAfter(string whatWasDone, bool withTheImage) => _headlessUI.OnTheUIThread<IEnumerable<ContentBlock>>(() =>
   {
      var lines = Screen.TopLevelsFromTheFront.Select(it => ScreenDescription.TopLevelLine(Screen, it)).ToList();
      if(Screen.AreaShown is not {} area) return [Text($"{whatWasDone} {NothingIsOpen}".Trim())];

      var whatTheImageShows = withTheImage ? $"\nThe image shows the screen from {area.X},{area.Y}, {area.Width}×{area.Height} pixels." : "";
      var text = Text($"{whatWasDone}\nOn the screen, from the front:\n{string.Join("\n", lines)}{whatTheImageShows}".Trim());
      if(!withTheImage) return [text];

      var image = Screen.Render(area);
      return [text, ImageContentBlock.FromBytes(image.Png, "image/png")];
   });

   static TextContentBlock Text(string text) => new() { Text = text };
}
