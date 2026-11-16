using System;

namespace JAStudio.Core.Note;

/// <summary>
/// How every change to a note is made: <c>note.Edit(it => ...)</c>, which runs the edit, in a unit of work, on the note as it is committed now.<br/>
/// Inside the edit, read and change only <c>it</c>, never the note <c>Edit</c> was called on, which may be stale.
/// </summary>
/// <remarks>
/// The design is in dev_docs/taggregates/shared/how-jastudio-edits-a-note.md.
/// </remarks>
/// <remarks>
/// wip: until a taggregate store keeps the notes (dev_docs/taggregates/shared/the-migration.md, step 7), the edit runs on the note itself. Then<br/>
/// <c>Edit</c> joins the unit of work that is running, or begins one, gets the note's private copy for update by its id, and runs the edit on the<br/>
/// copy, which becomes the note everyone reads when the unit of work commits.
/// </remarks>
public static class EditingANote
{
   extension<TNote>(TNote note) where TNote : JPNote
   {
      /// <summary>
      /// Runs <paramref name="edit"/> on the note as it is committed now, in a unit of work. <paramref name="edit"/> reads and changes only the<br/>
      /// note it is given.
      /// </summary>
      public void Edit(Action<TNote> edit) => edit(note);
   }
}
