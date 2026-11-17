using System;
using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// A corpus note as the corpus stores it, as a JSON file of its own (<see cref="CorpusNoteFiles"/>): the note's snapshot. The note types' DTOs<br/>
/// derive from it.
/// </summary>
/// <remarks>
/// MemoryPack serializes it only to compare two of them (<see cref="HoldTheSameState{TDto}"/>): nothing stores what it makes.
/// </remarks>
[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackUnion(0, typeof(VocabDto))]
[MemoryPackUnion(1, typeof(KanjiDto))]
[MemoryPackUnion(2, typeof(SentenceDto))]
public abstract partial class CorpusNoteDto
{
   public Guid Id { get; init; }
   public List<string> Tags { get; init; } = [];

   /// <summary>
   /// Whether <paramref name="one"/> and <paramref name="other"/> hold the same state: whether they serialize to the same bytes. A DTO is a note's<br/>
   /// snapshot, so this is how a taggregate store tells whether a note changed, as a reload does.
   /// </summary>
   public static bool HoldTheSameState<TDto>(TDto one, TDto other) where TDto : CorpusNoteDto =>
      MemoryPackSerializer.Serialize(one).AsSpan().SequenceEqual(MemoryPackSerializer.Serialize(other));
}
