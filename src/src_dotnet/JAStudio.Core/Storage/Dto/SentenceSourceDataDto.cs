using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A sentence's <see cref="Note.Sentences.SentenceSourceData"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class SentenceSourceDataDto
{
   public string Question { get; init; } = string.Empty;
   public string Answer { get; init; } = string.Empty;
   public string Comments { get; init; } = string.Empty;
   public string ExternalId { get; init; } = string.Empty;
   public List<AudioReferenceDto> Audio { get; init; } = [];
   public List<ImageReferenceDto> Screenshots { get; init; } = [];
}
