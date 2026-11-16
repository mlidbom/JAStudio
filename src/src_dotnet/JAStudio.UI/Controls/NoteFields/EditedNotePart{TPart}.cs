using System;
using JAStudio.Core.Note;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// A part of a note that an editor edits, such as a kanji's on readings or a vocab's parts of speech: read on the note as the collection holds it now<br/>
/// (<see cref="Current"/>), and changed in an edit of that note (<see cref="Edit"/>). <see cref="EditedNotePart.Of"/> makes one.
/// </summary>
/// <remarks>
/// The editor holds this rather than the part, since the collection replaces a note's object when it reloads the note, and a change is made in an<br/>
/// edit of the note (<see cref="EditingANote"/>), on the part of the note the edit gives.
/// </remarks>
sealed class EditedNotePart<TPart>
{
   readonly Func<TPart> _current;
   readonly Action<Action<TPart>> _edit;

   /// <param name="current">Finds the part on the note as the collection holds it now.</param>
   /// <param name="edit">Changes the part with the change it is given, in an edit of the note.</param>
   internal EditedNotePart(Func<TPart> current, Action<Action<TPart>> edit)
   {
      _current = current;
      _edit = edit;
   }

   /// <summary>The part, on the note as the collection holds it now.</summary>
   public TPart Current => _current();

   /// <summary>Changes the part with <paramref name="change"/>, in an edit of the note, which saves it.</summary>
   public void Edit(Action<TPart> change) => _edit(change);
}
