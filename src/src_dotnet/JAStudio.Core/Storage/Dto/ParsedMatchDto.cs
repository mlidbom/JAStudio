using System;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable]
public partial class ParsedMatchDto
{
   public string Variant { get; init; } = string.Empty;
   public int StartIndex { get; init; }
   public bool IsDisplayed { get; init; }
   public string ParsedForm { get; init; } = string.Empty;
   public Guid? VocabId { get; init; }
}
