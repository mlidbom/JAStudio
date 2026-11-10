using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A vocab's <see cref="Note.Vocabulary.VocabSourceData"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class VocabSourceDataDto
{
   public string Answer { get; init; } = string.Empty;
   public string Mnemonic { get; init; } = string.Empty;
   public string ReadingMnemonic { get; init; } = string.Empty;
   public List<AudioReferenceDto> Audio { get; init; } = [];
   public List<ImageReferenceDto> Images { get; init; } = [];
}
