using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// A control as the description of the screen (<see cref="ScreenDescription"/>) describes it: its ref (<see cref="ControlRef"/>), its kind, its<br/>
/// text, its state, such as checked or focused, and where it is on the screen, such as<br/>
/// <c>c12 CheckBox "Usually written in kana" checked at 400,310 180×24</c>. Also whether it's worth a line of the description at all.
/// </summary>
/// <remarks>
/// It reads the control as Avalonia's automation tree, the tree screen readers read, gives it: through the control's <see cref="AutomationPeer"/>,<br/>
/// which tells what a person sees of the control and can do with it, such as that it holds text, or can be checked.
/// </remarks>
/// <remarks>Used on the UI thread only.</remarks>
public sealed class ControlDescription
{
   readonly AutomationPeer _peer;

   internal ControlDescription(AutomationPeer peer, Control control)
   {
      _peer = peer;
      Control = control;
   }

   /// <summary>The description of <paramref name="control"/>, as its automation peer gives it.</summary>
   public static ControlDescription Of(Control control) => new(ControlAutomationPeer.CreatePeerForElement(control), control);

   /// <summary>The control described.</summary>
   public Control Control { get; }

   /// <summary>
   /// The text the control's automation peer gives it, such as a button's or a label's, or none when the text is only the name of the type of what<br/>
   /// the control shows: Avalonia's fallback for an item with no text of its own, such as a workspace tab's, named for its view model's type.
   /// </summary>
   public string Text
   {
      get
      {
         var text = _peer.GetName();
         return text == Control.DataContext?.GetType().FullName ? "" : text;
      }
   }

   /// <summary>Whether the control can't be seen: hidden, or with no size, as a menu bar made invisible by sizing it to nothing.</summary>
   public bool IsHidden => _peer.IsOffscreen() || Control.Bounds.Width == 0 || Control.Bounds.Height == 0;

   /// <summary>
   /// Whether the control is worth a line of its own: one a person reads or acts on, such as a label, a button or a text box, rather than one<br/>
   /// that only lays others out.
   /// </summary>
   public bool IsWorthAListing => _peer.IsControlElement() && (Text != "" || _peer.IsKeyboardFocusable() || IsActedOn);

   /// <summary>Whether the control is a label whose text is <paramref name="textAbove"/>, the text of the control listed it's in, such as a button's.</summary>
   public bool IsALabelRepeating(string? textAbove) => _peer.GetAutomationControlType() == AutomationControlType.Text && Text == textAbove;

   /// <summary>The control named as a line of the description starts, by its ref, its kind and its text, such as <c>c12 Button "Save"</c>.</summary>
   public string Naming(ControlRefs refs) => $"{refs.RefOf(Control)} {Control.GetType().Name}{(Text == "" ? "" : $" {ScreenDescription.Quoted(Text)}")}";

   /// <summary>The control's line of the description: its <see cref="Naming"/>, its <see cref="States"/>, and where it is on the screen.</summary>
   public string Line(ControlRefs refs)
   {
      var states = States;
      var place = new PixelRect(Control.PointToScreen(default), PixelSize.FromSize(Control.Bounds.Size, 1));
      return $"{Naming(refs)}{(states.Count == 0 ? "" : $" {string.Join(", ", states)}")} {ScreenDescription.WhereItIs(place)}";
   }

   /// <summary>What a person sees of the state of the control, such as whether it's checked, what text it holds, or that it's out of sight.</summary>
   IReadOnlyList<string> States => [.. StatesTheAutomationPeerGives, .. IsInSight ? [] : new[] { "out of sight" }];

   /// <summary>What the control's automation peer tells of its state, such as whether it's checked or what text it holds.</summary>
   IEnumerable<string> StatesTheAutomationPeerGives
   {
      get
      {
         if(_peer.GetProvider<IValueProvider>() is {} value)
         {
            if(value.Value is { Length: > 0 } held) yield return $"value {ScreenDescription.Quoted(held)}";
            else if(_peer.GetPlaceholderText() is { Length: > 0 } placeholder) yield return $"empty, placeholder {ScreenDescription.Quoted(placeholder)}";
            else yield return "empty";
            if(value.IsReadOnly) yield return "read-only";
         }
         if(_peer.GetProvider<IRangeValueProvider>() is {} range) yield return $"value {range.Value} of {range.Minimum} to {range.Maximum}";
         if(_peer.GetProvider<IToggleProvider>() is {} toggle)
            yield return toggle.ToggleState switch
            {
               ToggleState.On  => "checked",
               ToggleState.Off => "unchecked",
               _               => "indeterminate"
            };
         if(_peer.GetProvider<ISelectionItemProvider>() is { IsSelected: true }) yield return "selected";
         if(_peer.GetProvider<IExpandCollapseProvider>() is {} expandCollapse && expandCollapse.ExpandCollapseState != ExpandCollapseState.LeafNode)
            yield return expandCollapse.ExpandCollapseState == ExpandCollapseState.Expanded ? "expanded" : "collapsed";
         if(!_peer.IsEnabled()) yield return "disabled";
         if(_peer.HasKeyboardFocus()) yield return "has the keyboard";
         if(_peer.GetAcceleratorKey() is { Length: > 0 } shortcut) yield return $"shortcut {shortcut}";
      }
   }

   /// <summary>Whether a person can act on the control, such as press it, type into it, check it, select it, expand it or slide it.</summary>
   bool IsActedOn =>
      _peer.GetProvider<IInvokeProvider>() != null
   || _peer.GetProvider<IValueProvider>() != null
   || _peer.GetProvider<IToggleProvider>() != null
   || _peer.GetProvider<ISelectionItemProvider>() != null
   || _peer.GetProvider<IExpandCollapseProvider>() != null
   || _peer.GetProvider<IRangeValueProvider>() != null;

   /// <summary>
   /// Whether the middle of the control is where a person can see it: not clipped away by what it's in, such as the part of a scrolled list or<br/>
   /// editor out of view, or by the edge of its window.
   /// </summary>
   bool IsInSight
   {
      get
      {
         var middle = Control.Middle;
         return Control.GetVisualAncestors().Where(it => it.ClipToBounds || it is TopLevel).All(IsWithin);

         bool IsWithin(Visual clipping) => Control.TranslatePoint(middle, clipping) is {} there && new Rect(clipping.Bounds.Size).Contains(there);
      }
   }
}
