using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="Note.KanjiNote"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class KanjiDto : CorpusNoteDto
{
   public string Kanji { get; init; } = string.Empty;
   public string CorpusAnswer { get; init; } = string.Empty;
   public KanjiReadingsDto Readings { get; init; } = new();
   public List<string> Radicals { get; init; } = [];
   public List<WordReferenceDto> PrimaryVocab { get; init; } = [];
   public string References { get; init; } = string.Empty;
   public string Mnemonic { get; init; } = string.Empty;
   public List<string> SimilarMeaning { get; init; } = [];
   public List<string> ConfusedWith { get; init; } = [];

   /// <summary>The kanji's images, the corpus's own: see <see cref="Note.KanjiNote.Images"/>.</summary>
   public List<ImageReferenceDto> Images { get; init; } = [];

   public KanjiSourceDataDto SourceData { get; init; } = new();
}
