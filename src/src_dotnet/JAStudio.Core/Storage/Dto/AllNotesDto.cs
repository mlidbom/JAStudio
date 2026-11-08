using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>Container for serializing/deserializing all notes as a single unit (JSON or binary snapshot).</summary>
[MemoryPackable] partial class AllNotesDto
{
   public List<KanjiDto> Kanji { get; set; } = [];
   public List<VocabDto> Vocab { get; set; } = [];
   public List<SentenceDto> Sentences { get; set; } = [];
}
