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
/// JAStudio's own, not the state store's: it reads the note through the collection (<see cref="JPCollection.NoteFromNoteId"/>), so each read<br/>
/// passes through the collection's <see cref="LookupRecorder{TNote}"/>, as every other lookup of a note does. The note for update comes from the<br/>
/// unit of work (<see cref="NoteUnitOfWork.ForUpdate{TNote}"/>).
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

   public override bool Equals(object? other) => other is NoteReference<TNote> reference && reference.Id == Id;
   public override int GetHashCode() => Id.GetHashCode();
}
