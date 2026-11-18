using System;
using System.Collections.Generic;
using Compze.Teventive.TaggregateStores;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// A unit of work in which JAStudio changes notes, as the code that changes them sees it: the state store's unit of work, through which it gets<br/>
/// the notes it edits or deletes and adds new notes, and what is to be done once it has committed, such as deleting a deleted note's media<br/>
/// files. It commits or rolls back as a whole. <see cref="NoteUnitsOfWork"/> runs it. Once it has committed, the tevents the notes published<br/>
/// in it are observed, each unit of work's together (<see cref="Compze.Tessaging.TessageBus.IDynamicTeventObservers"/>).
/// </summary>
/// <remarks>review-naming: NoteUnitOfWork, after Compze's TaggregateUnitOfWork, is a name taggregates-opus chose (2026-11-16).</remarks>
public sealed class NoteUnitOfWork
{
   readonly ITaggregateStoreUpdater _store;
   readonly List<Action> _whatToDoOnceCommitted = [];

   internal NoteUnitOfWork(ITaggregateStoreUpdater store) => _store = store;

   /// <summary>
   /// This unit of work's copy of <paramref name="note"/>, which it may change, and which every read of the note inside it gets from then on. A note<br/>
   /// being created, not yet added to the collection, has no copy: it is the note itself.
   /// </summary>
   public TNote ForUpdate<TNote>(TNote note) where TNote : JPNote => _store.TryGetForUpdate<TNote>(note.Id, out var copy) ? copy : note;

   /// <summary>
   /// The note that published <paramref name="tevent"/>, as this unit of work holds it: what a participation handler of the tevent changes other notes<br/>
   /// from. Compze runs the handler inside the unit of work as the note ends publishing it, so the note is this unit of work's copy, or a new note it added.
   /// </summary>
   public TNote NoteThatPublished<TNote>(INoteTevent tevent) where TNote : JPNote => _store.Get<TNote>(tevent.TaggregateId);

   /// <summary>Adds <paramref name="note"/>, a note just created, to the collection when this unit of work commits.</summary>
   public void Add(JPNote note) => _store.Add(note);

   /// <summary>Has <paramref name="action"/> done once this unit of work has committed, on the thread that ran it, and never if it rolls back.</summary>
   public void OnceCommitted(Action action) => _whatToDoOnceCommitted.Add(action);

   /// <summary>Does what was to be done once this unit of work committed (<see cref="OnceCommitted"/>).</summary>
   internal void Committed() => _whatToDoOnceCommitted.ForEach(it => it());
}
