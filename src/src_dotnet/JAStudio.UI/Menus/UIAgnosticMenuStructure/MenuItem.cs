using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Anki;
using JAStudio.UI.Utils;
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

   /// <summary>
   /// The keyboard shortcut that runs this command, shown beside it. Only Command items have one.<br/>
   /// The menu only shows it. Binding it is the job of whoever owns the menu: the workspace window binds its main menu's,<br/>
   /// and Anki's global shortcuts bind those of the JA Studio menu in Anki's menu bar.<br/>
   /// It's written like <see cref="Shortcuts.NoteViewShortcut.KeyGesture"/>, for example <c>Ctrl+Shift+s</c>.
   /// </summary>
   public virtual string? KeyboardShortcut => null;

   /// <summary>The keyboard shortcuts of the visible commands in <paramref name="menu"/>, submenus included, with what they run.</summary>
   public static IEnumerable<(string KeyGesture, Action Action)> KeyboardShortcutsIn(IEnumerable<SpecMenuItem> menu) =>
      menu.Where(it => it.IsVisible)
          .SelectMany(it => it.Kind switch
          {
             SpecMenuItemKind.Submenu => KeyboardShortcutsIn(it.Children),
             SpecMenuItemKind.Command when it.KeyboardShortcut is {} keyGesture => [(keyGesture, it.Action)],
             _ => []
          });

   /// <summary>
   /// The item of <paramref name="menu"/> that <paramref name="key"/> picks, its <see cref="AcceleratorKey"/>, ignoring case, or null when no item has that key.<br/>
   /// The tree editors use it so that typing a key on the tree does what typing it in the add menu would: in the search, t opens the tag dialog,<br/>
   /// and q opens the add menu with the question's submenu open.
   /// </summary>
   public static SpecMenuItem? ItemPickedBy(char key, IEnumerable<SpecMenuItem> menu) =>
      menu.FirstOrDefault(it => it.AcceleratorKey is {} itemKey && char.ToLowerInvariant(itemKey) == char.ToLowerInvariant(key));

   /// <summary>
   /// This item, picked by <paramref name="key"/>: <paramref name="key"/> is its <see cref="AcceleratorKey"/>, and its <see cref="Name"/> shows it as<br/>
   /// <see cref="ShortcutFinger.Mnemonic"/> does. For a menu whose keys are picked once its items are made, as <see cref="AutomaticMnemonics"/> picks them.<br/>
   /// Only Command and Submenu items have a key.
   /// </summary>
   internal virtual SpecMenuItem WithAcceleratorKey(char key) => throw new InvalidOperationException($"A {Kind} menu item has no accelerator key");

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
                        //refactor: Whether Python runs stands in for whether JAStudio runs inside Anki. The refresh should go through
                        //   IAnkiWindows.Refresh, which every host registers, but this static factory has no services to resolve it from.
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

      internal override SpecMenuItem WithAcceleratorKey(char key) =>
         new CommandSpec(ShortcutFinger.Mnemonic(key, Name), Action, key, KeyboardShortcut, IsEnabled) { IsVisible = IsVisible };
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

      internal override SpecMenuItem WithAcceleratorKey(char key) => new SubmenuSpec(ShortcutFinger.Mnemonic(key, Name), Children, key) { IsVisible = IsVisible };
   }
}
