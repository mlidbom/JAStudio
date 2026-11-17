using System;
using Compze.Abstractions;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note;

/// <summary>
/// A note's reference: on each read, the note with its id as the collection holds it now, inside a unit of work as it has changed it.<br/>
/// <see cref="ReferringToANote.Reference{TNote}"/> makes one.
/// </summary>
/// <remarks>
/// JAStudio's own, not the state store's: it gets the note for update in JAStudio's unit of work, which does what is to be done once it has<br/>
/// committed (<see cref="NoteUnitOfWork.OnceCommitted"/>). The store's own would get it in the store's unit of work, unknown to JAStudio's.
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

   /// <summary>The copy of the note that the unit of work running on this thread may change. Fails when none runs.</summary>
   public TNote ForUpdate() => _collection.TheRunningUnitOfWork.ForUpdate(Current);

   public override bool Equals(object? other) => other is NoteReference<TNote> reference && reference.Id == Id;
   public override int GetHashCode() => Id.GetHashCode();
}
