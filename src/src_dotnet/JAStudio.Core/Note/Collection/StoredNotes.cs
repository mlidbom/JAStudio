using System;
using System.Collections.Generic;
using System.Linq;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Teventive.StateStore;
using Compze.Teventive.TaggregateStores;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// The notes of one class that the state store keeps, as their collection reads them: every one, the one with an id, and those an index files<br/>
/// under a key (<see cref="IndexBy{TKey}"/>), each as it is now: inside a unit of work, as it has changed them. Every lookup passes through<br/>
/// <see cref="LookupRecorder"/>.
/// </summary>
/// <remarks>
/// The store is resolved at the first lookup, not when the collection is built: resolving it resolves its storage, and a host may only know<br/>
/// where the corpus is once it has started.
/// </remarks>
sealed class StoredNotes<TNote> where TNote : JPNote
{
   readonly NoteServices _noteServices;
   readonly IRootResolver _resolver;
   readonly Lazy<IStateStore> _store;
   readonly Lazy<ITaggregateStoreReader> _reader;

   public StoredNotes(NoteServices noteServices, IRootResolver resolver)
   {
      _noteServices = noteServices;
      _resolver = resolver;
      _store = new Lazy<IStateStore>(resolver.Resolve<IStateStore>);
      _reader = new Lazy<ITaggregateStoreReader>(resolver.Resolve<ITaggregateStoreReader>);
   }

   /// <summary>The recorder every lookup of these notes passes its result through.</summary>
   public LookupRecorder<TNote> LookupRecorder { get; } = new();

   public List<TNote> All() => LookupRecorder.LookedUp([.. _store.Value.All<TNote>().Select(it => it.Current)]);

   /// <summary>The note with the id <paramref name="noteId"/>, or null when there is none of this class.</summary>
   public TNote? WithIdOrNone(NoteId noteId) => LookupRecorder.LookedUp(_reader.Value.TryGet<JPNote>(noteId, out var note) ? note as TNote : null);

   /// <summary>The note that the study-note with the id <paramref name="studyNoteId"/> belongs to, or null when there is none such of this class.</summary>
   public TNote? WithTheStudyNoteOrNone(long studyNoteId) =>
      _noteServices.StudyNoteIds.NoteIdOfTheStudyNote(studyNoteId) is {} noteId ? WithIdOrNone(noteId) : null;

   /// <summary>The store's index of these notes by <typeparamref name="TKey"/>, declared where the store is composed.</summary>
   public NoteIndex<TNote, TKey> IndexBy<TKey>() where TKey : notnull => new(_resolver, LookupRecorder);
}
