using System;
using Compze.Abstractions.Public;

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
   public static VocabId New() => new(Guid.NewGuid());
}

public class KanjiId(Guid id) : NoteId(id)
{
   public static KanjiId New() => new(Guid.NewGuid());
}

public class SentenceId(Guid id) : NoteId(id)
{
   public static SentenceId New() => new(Guid.NewGuid());
}
