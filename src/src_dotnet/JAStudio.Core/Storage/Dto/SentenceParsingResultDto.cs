using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable]
public partial class SentenceParsingResultDto
{
   public string Sentence { get; init; } = string.Empty;
   public string ParserVersion { get; init; } = string.Empty;
   public List<ParsedMatchDto> ParsedWords { get; init; } = [];
}
