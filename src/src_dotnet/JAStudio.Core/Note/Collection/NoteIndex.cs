using System;
using System.Collections.Generic;
using System.Linq;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Teventive.StateStore;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// One of the state store's indexes of the notes, such as vocab by form, as a collection reads it: the notes filed under a key, as they are<br/>
/// now, each lookup passed through the collection's <see cref="LookupRecorder{TNote}"/>. The store keeps the index as units of work commit,<br/>
/// filing each note under the keys its declaration where the store is composed gives (<see cref="VocabIndexes.DeclareIn"/>).
/// </summary>
/// <remarks>
/// The store's index is resolved at the first lookup, not when the collection is built: resolving it builds the store, which resolves its<br/>
/// storage, and a host may only know where the corpus is once it has started.
/// </remarks>
sealed class NoteIndex<TNote, TKey>
   where TNote : JPNote
   where TKey : notnull
{
   readonly Lazy<ITaggregateIndex<TNote, TKey>> _index;
   readonly LookupRecorder<TNote> _lookupRecorder;

   public NoteIndex(IRootResolver resolver, LookupRecorder<TNote> lookupRecorder)
   {
      _index = new Lazy<ITaggregateIndex<TNote, TKey>>(resolver.Resolve<ITaggregateIndex<TNote, TKey>>);
      _lookupRecorder = lookupRecorder;
   }

   /// <summary>
   /// The notes filed under <paramref name="key"/>, as they are now, inside a unit of work as it has changed them, in the order of their ids, as<br/>
   /// the store's index gives them: the order the corpus's notes load in. What takes the first of several, as the tokenizer's godan check does<br/>
   /// (<see cref="LanguageServices.JanomeEx.Tokenizing.PreProcessingStage.WordInfo"/>), depends on it.
   /// </summary>
   public List<TNote> NotesWith(TKey key) => _lookupRecorder.LookedUp([.. _index.Value.Lookup(key).Select(it => it.Current)]);

   /// <summary>Whether any note is filed under <paramref name="key"/>.</summary>
   public bool HasNotesWith(TKey key) => _index.Value.HasAny(key);
}
