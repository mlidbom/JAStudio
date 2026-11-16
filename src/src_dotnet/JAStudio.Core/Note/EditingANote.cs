using System;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note;

/// <summary>
/// How every change to a note is made: <c>note.Edit(it => ...)</c>, which runs the edit, in a unit of work, on the note as it is committed now.<br/>
/// Inside the edit, read and change only <c>it</c>, never the note <c>Edit</c> was called on, which may be stale.
/// </summary>
/// <remarks>
/// <c>Edit</c> joins the unit of work running on its thread, or begins one (<see cref="JPCollection.InAUnitOfWork"/>), gets the note's private<br/>
/// copy for update by its id, and runs the edit on the copy, which becomes the note everyone reads when the unit of work commits. A note the<br/>
/// state store serves to readers refuses every change. The design is in dev_docs/taggregates/shared/how-jastudio-edits-a-note.md.
/// </remarks>
public static class EditingANote
{
   /// <summary>
   /// Runs <paramref name="edit"/> on <paramref name="note"/> as it is committed now, in a unit of work. <paramref name="edit"/> reads and changes only<br/>
   /// the note it is given. A note being created, not yet added to the collection, is edited itself.
   /// </summary>
   /// <remarks>
   /// A classic extension method, not a C# 14 extension block: the Python stub generator writes an extension block's compiler-generated types, whose<br/>
   /// names aren't Python, into the stub of the whole namespace.
   /// </remarks>
   public static void Edit<TNote>(this TNote note, Action<TNote> edit) where TNote : JPNote =>
      note.Services.Collection.InAUnitOfWork(unitOfWork => edit(unitOfWork.ForUpdate(note)));
}
