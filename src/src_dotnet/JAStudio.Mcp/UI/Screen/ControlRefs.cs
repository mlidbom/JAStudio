using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// The <see cref="ControlRef"/>s the screen-tools name the screen's controls by: the description of the screen gives each control it lists a<br/>
/// ref, and the tools that act on a control take it. A control keeps its ref as long as it exists, so a ref from an earlier description still<br/>
/// names the same control: only a control that has appeared since needs a new description.
/// </summary>
/// <remarks>Used on the UI thread only, as the controls are. A control that no longer exists is forgotten with its ref.</remarks>
public sealed class ControlRefs
{
   readonly ConditionalWeakTable<Control, ControlRef> _refsOfTheControls = new();
   readonly Dictionary<ControlRef, WeakReference<Control>> _controlsByRef = [];
   int _refsGiven;

   /// <summary>The ref of <paramref name="control"/>: the one it was given, or a new one the first time.</summary>
   public ControlRef RefOf(Control control) => _refsOfTheControls.GetValue(control, GiveARef);

   /// <summary>
   /// The control <paramref name="controlRef"/> names, or, when it names none that's still on the screen, an exception saying so. A control that was<br/>
   /// taken off the screen, as the lines of a tree are when the tree is shown anew, may still exist for a while, but can't be described or acted on.
   /// </summary>
   public Control ControlWithRef(ControlRef controlRef)
   {
      if(!_controlsByRef.TryGetValue(controlRef, out var control) || !control.TryGetTarget(out var existing))
         throw new ArgumentException($"No control has the ref {controlRef}: describe_the_screen gives the refs of the controls on the screen.", nameof(controlRef));
      if(TopLevel.GetTopLevel(existing) is null)
         throw new ArgumentException($"The control with the ref {controlRef} is no longer on the screen, as when what showed it was shown anew: describe_the_screen gives the refs of the controls on it now.",
                                     nameof(controlRef));

      return existing;
   }

   ControlRef GiveARef(Control control)
   {
      var controlRef = new ControlRef($"c{++_refsGiven}");
      _controlsByRef[controlRef] = new WeakReference<Control>(control);
      return controlRef;
   }
}
