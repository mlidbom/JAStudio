using System;
using Compze.Abstractions;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note;

/// <summary>
/// A note's reference, over the collection: on each read, the note with its id as the collection holds it now.<br/>
/// <see cref="ReferringToANote.Reference{TNote}"/> makes one.
/// </summary>
/// <remarks>
/// wip: until a taggregate store keeps the notes (dev_docs/taggregates/shared/the-migration.md, step 7), the reference finds the note in the<br/>
/// collection, which changes its notes in place. Then the store's own references replace it.
/// </remarks>
sealed class NoteReference<TNote> : ITaggregateReference<TNote> where TNote : JPNote
{
   readonly JPCollection _collection;
   readonly NoteId _id;

   internal NoteReference(TNote note)
   {
      _collection = note.Services.Collection;
      _id = note.Id;
   }

   public TaggregateId Id => _id;

   public TNote Current => _collection.NoteFromNoteId(_id) as TNote ?? throw new InvalidOperationException($"The collection holds no {typeof(TNote).Name} with the id {Id}.");

   /// <summary>The note, which the edit running on this thread is editing: fails otherwise, as the store's reference fails outside a unit of work.</summary>
   public TNote ForUpdate()
   {
      var note = Current;
      EditingANote.AssertIsBeingEdited(note);
      return note;
   }

   public override bool Equals(object? other) => other is NoteReference<TNote> reference && reference.Id == Id;
   public override int GetHashCode() => Id.GetHashCode();
}
