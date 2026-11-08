using System;
using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// A corpus note as the corpus stores it: as a JSON file of its own, and in the snapshot cache JAStudio loads the corpus from quickly.<br/>
/// The note types' DTOs derive from it.
/// </summary>
/// <remarks>
/// The snapshot cache (MemoryPack) reads members by their position. An old snapshot reads correctly only while new members are added after<br/>
/// the existing ones; any other change to a DTO's members needs the snapshot caches deleted, as a migration of the corpus files does.
/// </remarks>
[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackUnion(0, typeof(VocabDto))]
[MemoryPackUnion(1, typeof(KanjiDto))]
[MemoryPackUnion(2, typeof(SentenceDto))]
public abstract partial class CorpusNoteDto : IIdentifiableByGuid
{
   public Guid Id { get; init; }
   public List<string> Tags { get; init; } = [];
}
