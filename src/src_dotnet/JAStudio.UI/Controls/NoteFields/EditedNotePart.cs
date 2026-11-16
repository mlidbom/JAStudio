using System;
using JAStudio.Core.Note;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>Makes an <see cref="EditedNotePart{TPart}"/>.</summary>
static class EditedNotePart
{
   /// <summary>The part <paramref name="partOf"/> finds on the note <paramref name="currentNote"/> finds as the collection holds it now.</summary>
   public static EditedNotePart<TPart> Of<TNote, TPart>(Func<TNote> currentNote, Func<TNote, TPart> partOf) where TNote : JPNote =>
      new(() => partOf(currentNote()), change => currentNote().Edit(it => change(partOf(it))));
}
