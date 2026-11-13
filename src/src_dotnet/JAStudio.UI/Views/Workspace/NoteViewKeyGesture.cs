using Avalonia.Input;
using JAStudio.UI.Shortcuts;

namespace JAStudio.UI.Views.Workspace;

/// <summary>
/// A key pressed on a note's page, written as a note view shortcut's key is (<see cref="NoteViewShortcut.KeyGesture"/>), such as <c>Ctrl+Shift+s</c>:<br/>
/// the modifiers in the order Ctrl, Shift, Alt, then the key in lower case. As <c>card.js</c> writes the keys pressed on a card's page.
/// </summary>
static class NoteViewKeyGesture
{
   /// <summary>The key gesture of <paramref name="keyPressed"/>; none for a key that types nothing, such as an arrow, which no note view shortcut is bound to.</summary>
   public static string? Of(KeyEventArgs keyPressed) =>
      keyPressed.KeySymbol is {} keySymbol
         ? (keyPressed.KeyModifiers.HasFlag(KeyModifiers.Control) ? "Ctrl+" : "")
         + (keyPressed.KeyModifiers.HasFlag(KeyModifiers.Shift) ? "Shift+" : "")
         + (keyPressed.KeyModifiers.HasFlag(KeyModifiers.Alt) ? "Alt+" : "")
         + keySymbol.ToLowerInvariant()
         : null;
}
