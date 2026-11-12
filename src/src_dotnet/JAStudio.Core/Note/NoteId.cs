using System;
using System.Collections.Concurrent;
using Compze.Abstractions;

namespace JAStudio.Core.Note;

/// <summary>
/// Base type for domain note identity. Inherits from <see cref="EntityId"/> which provides
/// hierarchy-aware equality: NoteId and VocabId with the same Guid are equal (parent↔child),
/// but VocabId and KanjiId with the same Guid are NOT (sibling types).
/// </summary>
public class NoteId(Guid id) : EntityId(id)
{
   public static NoteId Parse(string value) => new(Guid.Parse(value));
}

public class VocabId(Guid id) : NoteId(id)
{
   /// <summary>The one <see cref="VocabId"/> of each vocab's <see cref="Guid"/> that <see cref="Shared"/> gives: a few tens of thousands.</summary>
   static readonly ConcurrentDictionary<Guid, VocabId> SharedInstances = new();

   public static VocabId New() => new(Guid.NewGuid());

   /// <summary>
   /// The one <see cref="VocabId"/> of <paramref name="id"/> that all who ask here share: for what holds a vocab's id many times over, such as<br/>
   /// the sentences' stored matches, of which the corpus loaded 600,000 with an id each, 22 MB of them (2026-11-12). An id equals another of<br/>
   /// the same <see cref="Guid"/>, shared or not.
   /// </summary>
   public static VocabId Shared(Guid id) => SharedInstances.GetOrAdd(id, it => new VocabId(it));
}

public class KanjiId(Guid id) : NoteId(id)
{
   public static KanjiId New() => new(Guid.NewGuid());
}

public class SentenceId(Guid id) : NoteId(id)
{
   public static SentenceId New() => new(Guid.NewGuid());
}
