using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// The mouse and the keyboard of the <see cref="HeadlessScreen"/>: input as a person gives it, through Avalonia's headless platform, so that it<br/>
/// goes where a person's would and does what a person's does, such as move the keyboard, open a menu, or close a popup clicked outside of.
/// </summary>
/// <remarks>Used on the UI thread only.</remarks>
public sealed class ScreenInput
{
   readonly HeadlessScreen _screen;

   public ScreenInput(HeadlessScreen screen) => _screen = screen;

   /// <summary>
   /// Clicks <paramref name="control"/> in its middle with <paramref name="button"/>, twice for a double click, while <paramref name="modifiersHeld"/> are held.<br/>
   /// A click in a window that isn't active activates it first, as on a desktop.
   /// </summary>
   /// <remarks>
   /// Each press and release of the button goes to the window or popup in front at that point of the screen at that moment, as on a real screen: a press<br/>
   /// can close what it pressed in, such as a menu, and its release then goes to what's behind, or nowhere.
   /// </remarks>
   public void Click(Control control, MouseButton button, int clicks, RawInputModifiers modifiersHeld)
   {
      var (topLevel, middle) = WhereTheMouseReaches(control);
      ActivateItWhenItsAnInactiveWindow(topLevel);
      var pointOnTheScreen = topLevel.PointToScreen(middle);
      topLevel.MouseMove(middle, modifiersHeld);
      for(var click = 0; click < clicks; click++)
      {
         ToWhatIsInFrontAt(pointOnTheScreen, (inFront, point) => inFront.MouseDown(point, button, modifiersHeld));
         ToWhatIsInFrontAt(pointOnTheScreen, (inFront, point) => inFront.MouseUp(point, button, modifiersHeld));
      }
      return;

      static void ActivateItWhenItsAnInactiveWindow(TopLevel topLevel)
      {
         if(topLevel is not Window { IsActive: false } window) return;

         window.Activate();
         Dispatcher.UIThread.RunJobs();
      }
   }

   /// <summary>Turns the mouse wheel over <paramref name="control"/>'s middle by <paramref name="notchesDown"/>, scrolling down, or up when it's negative.</summary>
   public void Scroll(Control control, int notchesDown)
   {
      var (topLevel, middle) = WhereTheMouseReaches(control);
      topLevel.MouseMove(middle);
      topLevel.MouseWheel(middle, new Vector(0, -notchesDown));
   }

   /// <summary>
   /// Presses <paramref name="chords"/>, one after another, each released before the next is pressed. A key that types text types it too, as a real<br/>
   /// keyboard's does, such as a letter typed into a text box, or into a menu, which picks the item that letter is the key of. Each goes to the window<br/>
   /// that has the keyboard once the UI has handled the one before, which may have opened a window.
   /// </summary>
   public void Press(IEnumerable<KeyChord> chords)
   {
      foreach(var chord in chords)
      {
         var window = _screen.WindowManager.WindowWithTheKeyboard();
         window.KeyPress(chord.Key, chord.ModifiersHeld, chord.PhysicalKey, chord.KeySymbol);
         if(chord.TextTyped is {} text) window.KeyTextInput(text);
         _screen.WindowManager.WindowWithTheKeyboard().KeyRelease(chord.Key, chord.ModifiersHeld, chord.PhysicalKey, chord.KeySymbol);
         Dispatcher.UIThread.RunJobs();
      }
   }

   /// <summary>Types <paramref name="text"/>, as a keyboard or an input method types text, into the control that has the keyboard.</summary>
   public void Type(string text) => _screen.WindowManager.WindowWithTheKeyboard().KeyTextInput(text);

   /// <summary>Gives <paramref name="input"/> to the window or popup in front at <paramref name="pointOnTheScreen"/>, at that point of it, if anything is there.</summary>
   void ToWhatIsInFrontAt(PixelPoint pointOnTheScreen, Action<TopLevel, Point> input)
   {
      if(_screen.InFrontAt(pointOnTheScreen) is {} inFront) input(inFront, inFront.PointToClient(pointOnTheScreen));
   }

   /// <summary>
   /// The top level <paramref name="control"/> is in, and its middle there, where the mouse reaches it. Or, when a click at its middle would<br/>
   /// reach something else, such as a window in front of it, or the edge of the list it's scrolled out of, an exception saying what.
   /// </summary>
   (TopLevel topLevel, Point middle) WhereTheMouseReaches(Control control)
   {
      var topLevel = TopLevel.GetTopLevel(control) is {} itsTopLevel && _screen.Shows(itsTopLevel)
                        ? itsTopLevel
                        : throw new ArgumentException($"{_screen.Refs.RefOf(control)} isn't on the screen any more: describe_the_screen lists what is.", nameof(control));
      var middle = control.TranslatePoint(control.Middle, topLevel)
                ?? throw new InvalidOperationException($"{_screen.Refs.RefOf(control)} has no place in its {topLevel.GetType().Name}.");

      if(_screen.InFrontAt(topLevel.PointToScreen(middle)) is {} inFront && inFront != topLevel)
         throw new ArgumentException($"{_screen.Refs.RefOf(control)} is behind {_screen.Refs.RefOf(inFront)}, which a click at its middle would reach instead.", nameof(control));
      if(topLevel.InputHitTest(middle, enabledElementsOnly: false) is Visual reached && reached != control && !control.IsVisualAncestorOf(reached))
         throw new ArgumentException($"A click at the middle of {_screen.Refs.RefOf(control)} would reach {Named(reached)} instead: it's scrolled out of sight, or something is over it.", nameof(control));

      return (topLevel, middle);
   }

   /// <summary>What a click reached, named by its ref when it's a control, so describe_the_screen's description of it can be found.</summary>
   string Named(Visual reached) => reached is Control control ? $"{_screen.Refs.RefOf(control)}, a {control.GetType().Name}" : $"a {reached.GetType().Name}";
}
