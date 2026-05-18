using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki;
using JAStudio.Core.Note;
using Python.Runtime;

namespace JAStudio.UI.Menus.UIAgnosticMenuStructure;

/// <summary>
/// The kind of menu item: Command (leaf action), Submenu (container), or Separator (divider line).
/// </summary>
public enum SpecMenuItemKind
{
   Command,
   Submenu,
   Separator
}

/// <summary>
/// UI-agnostic menu item specification. Represents either a leaf command, a submenu container, or a separator.
/// This specification is independent of any UI framework (Avalonia, WPF, Qt, etc.) and can be passed to
/// framework-specific adapters that recursively build the actual UI menu structure.
/// Can also be passed to Python via pythonnet for building menus in PyQt.
///
/// Create instances via the static factory methods: Command(), Submenu(), Separator().
/// </summary>
public abstract class SpecMenuItem
{
   /// <summary>The kind of menu item this instance represents.</summary>
   public abstract SpecMenuItemKind Kind { get; }

   /// <summary>
   /// Display name of the menu item (e.g., "Open in Browser", "Lookup").
   /// Empty for separators.
   /// </summary>
   public string Name { get; }

   /// <summary>
   /// Keyboard accelerator character for Alt+Key navigation (e.g., 'K' for Alt+K).
   /// Used by ShortcutFinger to generate underlined accelerator text in Name.
   /// </summary>
   public char? AcceleratorKey { get; }

   /// <summary>
   /// Whether the menu item is visible in the menu.
   /// Use this for conditional menu items that should be completely hidden.
   /// </summary>
   public bool IsVisible { get; set; } = true;

   /// <summary>Action to execute when clicked. Only valid for Command items.</summary>
   public virtual Action Action => throw new InvalidOperationException($"Action is not available on a {Kind} menu item");

   /// <summary>Child menu items. Only valid for Submenu items.</summary>
   public virtual IReadOnlyList<SpecMenuItem> Children => throw new InvalidOperationException($"Children is not available on a {Kind} menu item");

   /// <summary>Keyboard shortcut display text (e.g., "Ctrl+O"). Display only.</summary>
   public virtual string? KeyboardShortcut => null;

   /// <summary>Whether the menu item is enabled (clickable). Disabled items appear grayed out.</summary>
   public virtual bool IsEnabled => true;

   /// <summary>Does this submenu have at least one visible, non-separator child? Only valid for Submenu items.</summary>
   public virtual bool HasVisibleChildren => throw new InvalidOperationException($"HasVisibleChildren is not available on a {Kind} menu item");

   private protected SpecMenuItem(string name = "", char? acceleratorKey = null)
   {
      Name = name;
      AcceleratorKey = acceleratorKey;
   }

   // ── Factory methods ────────────────────────────────────────────────

   /// <summary>Create a separator menu item (horizontal divider line).</summary>
   public static SpecMenuItem Separator() => new SeparatorSpec();

   /// <summary>Create a leaf command menu item.</summary>
   public static SpecMenuItem Command(string name, Action action, char? acceleratorKey = null, string? shortcut = null, bool enabled = true)
      => new CommandSpec(name, action, acceleratorKey, shortcut, enabled);

   public static SpecMenuItem UICommand(string name, Action action, char? acceleratorKey = null, string? shortcut = null, bool enabled = true)
   {
      return Command(name,
                     () =>
                     {
                        action();
                        if(PythonEngine.IsInitialized)
                        {
                           AnkiFacade.UIUtils.Refresh();
                        }
                     }, acceleratorKey, shortcut, enabled);
   }

   /// <summary>Create a submenu container.</summary>
   public static SpecMenuItem Submenu(string name, IReadOnlyList<SpecMenuItem> children, char? acceleratorKey = null)
      => new SubmenuSpec(name, children, acceleratorKey);

   // ── Private subclasses ─────────────────────────────────────────────

   sealed class SeparatorSpec : SpecMenuItem
   {
      public override SpecMenuItemKind Kind => SpecMenuItemKind.Separator;
   }

   sealed class CommandSpec : SpecMenuItem
   {
      public override SpecMenuItemKind Kind => SpecMenuItemKind.Command;
      public override Action Action { get; }
      public override string? KeyboardShortcut { get; }
      public override bool IsEnabled { get; }

      internal CommandSpec(string name, Action action, char? acceleratorKey, string? shortcut, bool enabled)
         : base(name, acceleratorKey)
      {
         Action = action;
         KeyboardShortcut = shortcut;
         IsEnabled = enabled;
      }
   }

   sealed class SubmenuSpec : SpecMenuItem
   {
      public override SpecMenuItemKind Kind => SpecMenuItemKind.Submenu;
      public override IReadOnlyList<SpecMenuItem> Children { get; }

      /// <summary>Auto-disabled when there are no visible, non-separator children.</summary>
      public override bool IsEnabled => HasVisibleChildren;

      public override bool HasVisibleChildren => Children.Any(c => c.IsVisible && c.Kind != SpecMenuItemKind.Separator);

      internal SubmenuSpec(string name, IReadOnlyList<SpecMenuItem> children, char? acceleratorKey)
         : base(name, acceleratorKey) =>
         Children = children;
   }
}
