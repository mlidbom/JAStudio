using System;
using JAStudio.Core.Note;

namespace JAStudio.UI.Shortcuts;

/// <summary>A keyboard shortcut for a note being viewed: its key, and what it does. See <see cref="NoteViewShortcuts"/>.</summary>
public class NoteViewShortcut
{
   readonly Action<JPNote?> _action;

   public NoteViewShortcut(string keyGesture, Action<JPNote?> action)
   {
      KeyGesture = keyGesture;
      _action = action;
   }

   /// <summary>
   /// The key, written the way Qt and Avalonia both parse it, and the way <c>card.js</c> describes a key press:<br/>
   /// the modifiers first, in the order Ctrl, Shift, Alt, then a lower-case letter or a digit.<br/>
   /// For example <c>a</c>, <c>0</c> or <c>Ctrl+Shift+Alt+s</c>.
   /// </summary>
   public string KeyGesture { get; }

   /// <summary>
   /// Does what the shortcut does.<br/>
   /// <paramref name="viewedNote"/> is the note being viewed, if any.<br/>
   /// A shortcut that acts on one type of note does nothing for other types, or when no note is viewed.
   /// </summary>
   public void Run(JPNote? viewedNote) => _action(viewedNote);

   /// <summary>
   /// Does what the shortcut does when no note is being viewed: view toggles work, note actions do nothing.<br/>
   /// Python needs this: its generated stubs can't say that <see cref="Run(JPNote)"/> accepts null.
   /// </summary>
   public void Run() => _action(null);
}
