using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable]
public partial class SentenceConfigDto
{
   public List<string> HighlightedWords { get; init; } = [];
   public List<WordExclusionDto> IncorrectMatches { get; init; } = [];
   public List<WordExclusionDto> HiddenMatches { get; init; } = [];
}
