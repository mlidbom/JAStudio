using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Platform;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// The window manager of the <see cref="HeadlessScreen"/>, which Avalonia's headless platform has none of: it keeps track of which window is in<br/>
/// front, activates windows as a desktop's window manager does, and so knows which window has the keyboard.
/// </summary>
/// <remarks>
/// JAStudio depends on windows being activated and deactivated as on a desktop: its menus close as their window is deactivated, and the workspace<br/>
/// saves its state.
/// </remarks>
/// <remarks>
/// Used on the UI thread only. Windows are in front in the order they were last activated, those that stay on top in front of the rest. One window<br/>
/// is active at a time, so activating one deactivates the one that was, and when the active window closes or hides, the one in front is activated.
/// </remarks>
public sealed class HeadlessWindowManager
{
   /// <summary>Every window opened and not yet closed, shown or hidden, from the one activated longest ago to the one activated last.</summary>
   readonly List<Window> _windowsByActivation = [];

   HeadlessWindowManager() {}

   /// <summary>Starts managing the windows of the process's UI as they open, close, hide and are activated. Called once, on the UI thread, before any window opens.</summary>
   internal static HeadlessWindowManager ManagingTheWindowsFromNowOn()
   {
      var windowManager = new HeadlessWindowManager();
      Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) => windowManager.BringToTheFront(window));
      Window.WindowClosedEvent.AddClassHandler<Window>((window, _) =>
      {
         windowManager._windowsByActivation.Remove(window);
         windowManager.ActivateTheWindowInFrontUnlessOneIsActive();
      });
      Visual.IsVisibleProperty.Changed.AddClassHandler<Window>((_, change) =>
      {
         if(!change.GetNewValue<bool>()) windowManager.ActivateTheWindowInFrontUnlessOneIsActive();
      });
      WindowBase.IsActiveProperty.Changed.AddClassHandler<Window>((window, change) =>
      {
         if(change.GetNewValue<bool>() && windowManager.HasOpen(window)) windowManager.ActivateOnly(window);
      });
      return windowManager;
   }

   /// <summary>The windows shown, from the one in front to the one at the back.</summary>
   public IReadOnlyList<Window> WindowsFromTheFront => [.. WindowsFromTheBack.Reverse()];

   /// <summary>The windows shown, from the one at the back to the one in front.</summary>
   public IEnumerable<Window> WindowsFromTheBack =>
      _windowsByActivation.Where(it => it.IsVisible).OrderBy(it => it.Topmost);

   /// <summary>
   /// The window the keyboard's input goes to: the active one. When none is, as after the active one closed, the one in front, which a<br/>
   /// real screen would have made active.
   /// </summary>
   public Window WindowWithTheKeyboard()
   {
      var windows = WindowsFromTheFront;
      if(windows.Count == 0) throw new InvalidOperationException("No window is open to take the keyboard's input: render_the_workspace opens the workspace.");
      return windows.FirstOrDefault(it => it.IsActive) ?? windows[0];
   }

   /// <summary>The control the keyboard's input goes to, or null when none has it.</summary>
   public Control? ControlWithTheKeyboard() => WindowsFromTheFront.Count == 0 ? null : WindowWithTheKeyboard().FocusManager.GetFocusedElement() as Control;

   /// <summary>
   /// Whether <paramref name="window"/> is open, shown or hidden. The headless platform posts a window's activation, which can then come after the<br/>
   /// window has closed.
   /// </summary>
   bool HasOpen(Window window) => _windowsByActivation.Contains(window);

   /// <summary>Brings <paramref name="window"/>, activated, to the front, and deactivates every other window, as a window manager does.</summary>
   void ActivateOnly(Window window)
   {
      BringToTheFront(window);
      DeactivateEveryWindowBut(window);
   }

   void BringToTheFront(Window window)
   {
      _windowsByActivation.Remove(window);
      _windowsByActivation.Add(window);
   }

   /// <summary>Deactivates every window but <paramref name="activated"/>, as a window manager does when it activates one.</summary>
   void DeactivateEveryWindowBut(Window activated)
   {
      foreach(var window in _windowsByActivation.Where(it => it != activated && it.IsActive).ToList())
         Deactivate(window);
   }

   /// <summary>
   /// Deactivates <paramref name="window"/> as its platform does when another window is activated: through the platform window's <c>Deactivated</c>,<br/>
   /// which <see cref="WindowBase"/> handles by becoming inactive and raising <see cref="WindowBase.Deactivated"/>.
   /// </summary>
   /// <remarks>
   /// Avalonia offers no other way to deactivate a window, since a desktop's window manager does it, and its reference assemblies hide the platform<br/>
   /// window's <c>Deactivated</c> from code outside Avalonia. So it's called through reflection, and fails loudly should Avalonia rename it.
   /// </remarks>
   static void Deactivate(Window window)
   {
      var platformWindow = window.PlatformImpl ?? throw new InvalidOperationException($"The open window {window.Title} has no platform window.");
      var deactivated = typeof(IWindowBaseImpl).GetProperty("Deactivated")
                     ?? throw new InvalidOperationException($"Avalonia's {nameof(IWindowBaseImpl)} has no Deactivated any more: deactivating windows needs another way.");
      (deactivated.GetValue(platformWindow) as Action)?.Invoke();
   }

   /// <summary>
   /// Keeps the window of <paramref name="popup"/>, which just opened, from being activated, as a desktop shows a popup: without activating it.<br/>
   /// The headless platform activates every window it shows, popups too, which takes the keyboard from the control that opened the popup, and so<br/>
   /// closes a popup that closes as its control loses the keyboard, such as the suggestions of an <see cref="AutoCompleteBox"/>.
   /// </summary>
   /// <remarks>
   /// The platform posts the activation as it shows the popup's window, and the job reads the platform window's <c>Activated</c> when it runs, so<br/>
   /// clearing it before then keeps the activation from happening. It's done through reflection, as <see cref="Deactivate"/> is, for the same reason.
   /// </remarks>
   internal static void KeepFromBeingActivated(Popup popup)
   {
      var popupWindow = TopLevel.GetTopLevel(popup.Child) as PopupRoot ?? throw new InvalidOperationException("A popup on the headless screen is shown in a window of its own, since the screen doesn't draw popups in their window's overlay layer.");
      var platformWindow = popupWindow.PlatformImpl ?? throw new InvalidOperationException("An open popup's window has no platform window.");
      var activated = typeof(IWindowBaseImpl).GetProperty("Activated")
                   ?? throw new InvalidOperationException($"Avalonia's {nameof(IWindowBaseImpl)} has no Activated any more: showing popups without activating them needs another way.");
      activated.SetValue(platformWindow, null);
   }

   /// <summary>Activates the window in front when no window shown is active, as a window manager does when the active window closes or hides.</summary>
   void ActivateTheWindowInFrontUnlessOneIsActive()
   {
      var shown = WindowsFromTheFront;
      if(shown.Count > 0 && !shown.Any(it => it.IsActive)) shown[0].Activate();
   }
}
