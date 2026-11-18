using System;
using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// A corpus note as the corpus stores it, as a JSON file of its own (<see cref="CorpusNoteFiles"/>): the note's snapshot. The note types' DTOs<br/>
/// derive from it.
/// </summary>
/// <remarks>
/// MemoryPack serializes it for the memory image state store's load cache, a MemoryPack file of each class's snapshots that a load starts from<br/>
/// (<see cref="CorpusNoteFiles"/>).
/// </remarks>
[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackUnion(0, typeof(VocabDto))]
[MemoryPackUnion(1, typeof(KanjiDto))]
[MemoryPackUnion(2, typeof(SentenceDto))]
public abstract partial class CorpusNoteDto
{
   public Guid Id { get; init; }
   public List<string> Tags { get; init; } = [];
}
