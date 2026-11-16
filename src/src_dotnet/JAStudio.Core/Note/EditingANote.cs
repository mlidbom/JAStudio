using System;
using System.Collections.Immutable;
using Compze.Contracts;

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
/// copy, which becomes the note everyone reads when the unit of work commits. Until then, a change to a note of the collection that no edit on its<br/>
/// thread is editing fails (<see cref="AssertIsBeingEdited"/>), as a change to a note the store serves to readers will.
/// </remarks>
public static class EditingANote
{
   /// <summary>The ids of the notes the edits running on this thread are editing, or null when none is running.</summary>
   [ThreadStatic] static ImmutableHashSet<NoteId>? _theNotesBeingEdited;

   /// <summary>
   /// Runs <paramref name="edit"/> on <paramref name="note"/> as it is committed now, in a unit of work. <paramref name="edit"/> reads and changes only<br/>
   /// the note it is given.
   /// </summary>
   /// <remarks>
   /// A classic extension method, not a C# 14 extension block: the Python stub generator writes an extension block's compiler-generated types, whose<br/>
   /// names aren't Python, into the stub of the whole namespace.
   /// </remarks>
   public static void Edit<TNote>(this TNote note, Action<TNote> edit) where TNote : JPNote
   {
      var theNotesBeingEditedBefore = _theNotesBeingEdited;
      _theNotesBeingEdited = (theNotesBeingEditedBefore ?? []).Add(note.Id);
      try
      {
         edit(note);
      }
      finally
      {
         _theNotesBeingEdited = theNotesBeingEditedBefore;
      }
   }

   /// <summary>Fails unless an edit running on this thread is editing <paramref name="note"/>.</summary>
   /// <remarks>wip: goes when a taggregate store keeps the notes, whose committed notes refuse every change.</remarks>
   internal static void AssertIsBeingEdited(JPNote note)
   {
      var theNotesBeingEdited = _theNotesBeingEdited ?? [];
      Contract.State.Assert(theNotesBeingEdited.Contains(note.Id),
                            () => $"The {note.GetType().Name} {note} is changed {(theNotesBeingEdited.IsEmpty ? "outside any edit" : "in an edit of another note")}: a note of the collection is changed only in note.Edit(it => ...), through it.");
   }
}
