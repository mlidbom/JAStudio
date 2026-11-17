using System;
using System.Collections.Generic;
using Compze.Abstractions;
using Compze.Tessaging.TessageBus;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// A unit of work in which JAStudio changes notes, as the code that changes them sees it: the state store's unit of work, through which it gets<br/>
/// the notes it edits, adds new notes and deletes notes, and what is to be done once it has committed, such as deleting a deleted note's media<br/>
/// files. It commits or rolls back as a whole. <see cref="NoteUnitsOfWork"/> runs it. Once its work is done, it publishes for each note it added or<br/>
/// changed that the note was (<see cref="INoteChangedInAUnitOfWork"/>), and once it has committed, each note it changed is notified through<br/>
/// <see cref="JPCollection.NoteUpdated"/>.
/// </summary>
/// <remarks>review-naming: NoteUnitOfWork, after Compze's TaggregateUnitOfWork, is a name taggregates-opus chose (2026-11-16).</remarks>
public sealed class NoteUnitOfWork
{
   readonly ITaggregateStoreUpdater _store;
   readonly IUnitOfWorkTeventPublisher _tevents;

   /// <summary>
   /// The ids of the notes that published a tevent in this unit of work, in the order they first did: the notes it added and changed, and those<br/>
   /// it deleted (<see cref="KeepTrackOfTheNotesThatPublishThrough"/>).
   /// </summary>
   readonly List<TaggregateId> _notesThatPublished = [];

   readonly HashSet<TaggregateId> _notesThatPublishedAsASet = [];

   /// <summary>The notes this unit of work may change, in the order it first got them: the copies it got for update, and the new notes it added.</summary>
   readonly List<JPNote> _notesItMayChange = [];

   readonly HashSet<JPNote> _notesItMayChangeAsInstances = new(ReferenceEqualityComparer.Instance);
   readonly List<Action> _whatToDoOnceCommitted = [];

   internal NoteUnitOfWork(ITaggregateStoreUpdater store, IUnitOfWorkTeventPublisher tevents)
   {
      _store = store;
      _tevents = tevents;
   }

   /// <summary>
   /// Declares, through <paramref name="handle"/>, the participation handler by which the unit of work running keeps track of each note that publishes<br/>
   /// a tevent in it, so that it can tell once its work is done which notes it added or changed (<see cref="PublishWhichNotesItAddedOrChanged"/>).
   /// </summary>
   internal static void KeepTrackOfTheNotesThatPublishThrough(TessageBusHandlerRegistrar handle) => handle
     .ForTevent(static (INoteTevent it, JPCollection collection) => collection.TheRunningUnitOfWork.KeepTrackOf(it.TaggregateId));

   void KeepTrackOf(TaggregateId noteThatPublished)
   {
      if(_notesThatPublishedAsASet.Add(noteThatPublished)) _notesThatPublished.Add(noteThatPublished);
   }

   /// <summary>
   /// Publishes, for each note this unit of work added or changed, that it was, its class's <see cref="INoteChangedInAUnitOfWork"/>, in the order the<br/>
   /// notes first published a tevent in it. A note it deleted gets none. <see cref="NoteUnitsOfWork"/> calls it once the work in the outermost unit<br/>
   /// of work is done: what the participation handlers changed has published too, then.
   /// </summary>
   internal void PublishWhichNotesItAddedOrChanged()
   {
      foreach(var noteId in _notesThatPublished)
         if(_store.TryGet<JPNote>(noteId, out var note)) _tevents.Publish(note.ChangedInAUnitOfWork());
   }

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

   /// <summary>
   /// Deletes <paramref name="note"/>, as it is in this unit of work, from the collection when this unit of work commits. The note first publishes<br/>
   /// that it is deleted (<see cref="Tevents.INoteTevent.Deleted"/>), so that participation handlers change what it leaves in other notes while it<br/>
   /// is still there: the store's session refuses a note deleted in it.
   /// </summary>
   public void Delete(JPNote note)
   {
      var theNoteAsItIs = ForUpdate(note);
      theNoteAsItIs.Delete();
      //todo:compze: a taggregate that publishes an ITaggregateDeletedTevent is to leave Compze's state store as its unit of work commits, and
      //   ITaggregateStoreUpdater.Delete is to go (Magnus, 2026-11-17, through jastudio-rep-opus). Once it lands, this call goes, and
      //   JPNote.Delete is the whole deletion.
      _store.Delete<JPNote>(theNoteAsItIs.Id);
   }

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
