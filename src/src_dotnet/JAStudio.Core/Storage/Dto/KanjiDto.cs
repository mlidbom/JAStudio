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
   public List<string> PrimaryVocab { get; init; } = [];
   public string References { get; init; } = string.Empty;
   public string Mnemonic { get; init; } = string.Empty;
   public List<string> SimilarMeaning { get; init; } = [];
   public List<string> ConfusedWith { get; init; } = [];

   /// <summary>
   /// An image for the kanji, as Anki's <c>&lt;img src="…"&gt;</c> markup, or empty. Only a few kanji have one: JAStudio only carries it, kept from<br/>
   /// the Anki notes before they were trimmed, since an image can't be made again the way the kanji's audio can be computed.
   /// </summary>
   public string Image { get; init; } = string.Empty;

   public KanjiSourceDataDto SourceData { get; init; } = new();
}
