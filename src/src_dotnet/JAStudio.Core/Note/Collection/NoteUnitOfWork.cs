using System;
using System.Collections.Generic;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// A unit of work in which JAStudio changes notes, as the code that changes them sees it: the state store's unit of work, through which it gets<br/>
/// the notes it edits, adds new notes and deletes notes, and what is to be done once it has committed, such as deleting a deleted note's media<br/>
/// files. It commits or rolls back as a whole. <see cref="NoteUnitsOfWork"/> runs it, and once it has committed, each note it changed is<br/>
/// notified through <see cref="JPCollection.NoteUpdated"/>.
/// </summary>
/// <remarks>review-naming: NoteUnitOfWork, after Compze's TaggregateUnitOfWork, is a name taggregates-opus chose (2026-11-16).</remarks>
public sealed class NoteUnitOfWork
{
   readonly ITaggregateStoreUpdater _store;

   /// <summary>The notes this unit of work may change, in the order it first got them: the copies it got for update, and the new notes it added.</summary>
   readonly List<JPNote> _notesItMayChange = [];

   readonly HashSet<JPNote> _notesItMayChangeAsInstances = new(ReferenceEqualityComparer.Instance);
   readonly List<Action> _whatToDoOnceCommitted = [];

   internal NoteUnitOfWork(ITaggregateStoreUpdater store) => _store = store;

   /// <summary>
   /// This unit of work's copy of <paramref name="note"/>, which it may change, and which every read of the note inside it gets from then on. A note<br/>
   /// being created, not yet added to the collection, has no copy: it is the note itself.
   /// </summary>
   public TNote ForUpdate<TNote>(TNote note) where TNote : JPNote
   {
      if(!_store.TryGetForUpdate<TNote>(note.Id, out var copy)) return note;
      MayChange(copy);
      return copy;
   }

   /// <summary>
   /// The note that published <paramref name="tevent"/>, as this unit of work holds it: what a participation handler of the tevent changes other notes<br/>
   /// from. Compze runs the handler inside the unit of work as the note ends publishing it, so the note is this unit of work's copy, or a new note it added.
   /// </summary>
   public TNote NoteThatPublished<TNote>(INoteTevent tevent) where TNote : JPNote => _store.Get<TNote>(tevent.TaggregateId);

   /// <summary>Adds <paramref name="note"/>, a note just created, to the collection when this unit of work commits.</summary>
   public void Add(JPNote note)
   {
      _store.Save(note);
      MayChange(note);
   }

   /// <summary>Deletes <paramref name="note"/> from the collection when this unit of work commits.</summary>
   public void Delete(JPNote note) => _store.Delete<JPNote>(note.Id);

   /// <summary>Has <paramref name="action"/> done once this unit of work has committed, after its changed notes are notified, and never if it rolls back.</summary>
   public void OnceCommitted(Action action) => _whatToDoOnceCommitted.Add(action);

   /// <summary>
   /// Notifies each note this unit of work changed, as committed now, through <paramref name="notifyThatTheNoteWasUpdated"/>, then does what was to<br/>
   /// be done once it committed. A copy that published no change was not committed, and isn't notified.
   /// </summary>
   internal void Committed(ITaggregateStoreReader notes, Action<JPNote> notifyThatTheNoteWasUpdated)
   {
      foreach(var note in _notesItMayChange)
         if(notes.TryGet<JPNote>(note.Id, out var committed) && ReferenceEquals(committed, note))
            notifyThatTheNoteWasUpdated(committed);

      _whatToDoOnceCommitted.ForEach(it => it());
   }

   void MayChange(JPNote note)
   {
      if(_notesItMayChangeAsInstances.Add(note)) _notesItMayChange.Add(note);
   }
}
